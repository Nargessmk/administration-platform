using io.fusionauth;
using io.fusionauth.domain;
using io.fusionauth.domain.api;
using io.fusionauth.domain.api.twoFactor;
using io.fusionauth.domain.api.user;
using io.fusionauth.domain.search;
using Cloudland.APM.DTO;
using Cloudland.APM.Models;
using Cloudland.APM.Models.Auth;
using Cloudland.APM.Models.ResultModel;
using Cloudland.APM.Utils.ErrorTransform;
using Microsoft.Extensions.Options;
using System.Text.Json;
using UserCreationResult = Cloudland.APM.Models.ResultModel.UserCreationResult;

namespace Cloudland.APM.Microservices.Authentication.Services;

public class UserManagementService(
    IFusionAuthAsyncClient fusionAuthAsyncClient,
    ITransformErrorService transFormErrorService,
    IOptions<FileUploadSettings> uploadFileSettings) : IUserManagementService
{
    private readonly ITransformErrorService _transformErrorService = transFormErrorService;

    public async Task<List<User>> GetUsers(string? searchKey)
    {
        var query = searchKey == "*" || string.IsNullOrWhiteSpace(searchKey)
            ? $"NOT registrations.applicationId:3c219e58-ed0e-4b18-ad48-f4f92793ae32"
            : $"{searchKey} AND NOT registrations.applicationId:3c219e58-ed0e-4b18-ad48-f4f92793ae32";
        
        var result = await fusionAuthAsyncClient.SearchUsersByQueryAsync(new SearchRequest
        {
            search = new UserSearchCriteria
            {
                queryString = query,
                startRow = 0,
                numberOfResults = 10000
            }
        });
        return result.successResponse?.users?.OrderBy(c => c.username).ToList() ?? new List<User>();
    }

    public async Task<List<User>> GetUsersByIds(Guid[] ids)
    {
        var result = await fusionAuthAsyncClient.SearchUsersByQueryAsync(new SearchRequest
        {
            search = new UserSearchCriteria
            {
                startRow = 0,
                numberOfResults = 500,
                ids = [..ids]
            }
        });
        return result.successResponse?.users?.OrderBy(c => c.username).ToList() ?? new List<User>();
    }

    public async Task<List<User>?> GetUsersByIdsPaging(PagingQuery searchRequest)
    {
        if (searchRequest.Ids == null) return null;
        var result = await fusionAuthAsyncClient.SearchUsersByQueryAsync(new SearchRequest
        {
            search = new UserSearchCriteria
            {
                startRow = searchRequest.PageIndex,
                numberOfResults = searchRequest.PageSize,
                ids = [..searchRequest.Ids]
            }
        });
        return result.successResponse?.users?.OrderBy(c => c.username).ToList() ?? new List<User>();
    }

    public async Task<PagedResult<User>> GetUsersPaging(PagingQuery searchRequest)
    {
        var query = searchRequest.QueryString == "*" || string.IsNullOrWhiteSpace(searchRequest.QueryString)
            ? $"NOT registrations.applicationId:3c219e58-ed0e-4b18-ad48-f4f92793ae32"
            : $"{searchRequest.QueryString} AND NOT registrations.applicationId:3c219e58-ed0e-4b18-ad48-f4f92793ae32";
        
        var searchResponse = await fusionAuthAsyncClient.SearchUsersByQueryAsync(new SearchRequest
        {
            search = new UserSearchCriteria
            {
                queryString = query,
                startRow = (searchRequest.PageIndex - 1) * searchRequest.PageSize,
                numberOfResults = searchRequest.PageSize
            }
        });

        // Get total users 
        var allUsers = await GetUsers(searchRequest.QueryString);

        return new PagedResult<User>
        {
            Items = searchResponse.successResponse?.users?.OrderBy(u => u.username).ToList() ?? new List<User>(),
            PageIndex = searchRequest.PageIndex,
            PageSize = searchRequest.PageSize,
            TotalCount = allUsers.Count()
        };
    }

    public async Task<User?> GetUser(Guid id)
    {
        var result = await fusionAuthAsyncClient.RetrieveUserAsync(id);
        return result.WasSuccessful() ? result.successResponse.user : null;
    }

    public async Task<List<Application>?> GetApplications()
    {
        var activeApplications = await fusionAuthAsyncClient.RetrieveApplicationsAsync();
        var deActiveApplications = await fusionAuthAsyncClient.RetrieveInactiveApplicationsAsync();

        if (deActiveApplications.successResponse.applications.Count > 0)
            activeApplications.successResponse.applications.AddRange(deActiveApplications.successResponse.applications);

        return activeApplications.WasSuccessful() ? activeApplications.successResponse.applications.ToList() : null;
    }

    public async Task<Application?> GetApplicationById(Guid? id)
    {
        var result = await fusionAuthAsyncClient.RetrieveApplicationAsync(id);
        return result.WasSuccessful() ? result.successResponse.application : null;
    }

    public async Task<Application?> UpdateApplication(Guid? id, ApplicationRequest request)
    {
        var result = await fusionAuthAsyncClient.UpdateApplicationAsync(id, request);
        return result.WasSuccessful() ? result.successResponse.application : null;
    }

    public async Task<Application?> CreateApplication(ApplicationRequest request)
    {
        var result = await fusionAuthAsyncClient.CreateApplicationAsync(null, request);

        if (result.WasSuccessful())
        {
            var defaultRole = new ApplicationRole()
            {
                isDefault = true,
                isSuperRole = false,
                name = "user",
                description = "Default Application Role"
            };

            var defaultRoleResult = await CreateApplicationRole(result.successResponse.application.id, defaultRole);
            if (defaultRoleResult == null)
                return null;
        }

        return result.WasSuccessful() ? result.successResponse.application : null;
    }

    public async Task<bool> ReActiveApplication(Guid? applicationId)
    {
        var result = await fusionAuthAsyncClient.ReactivateApplicationAsync(applicationId);
        return result.WasSuccessful();
    }

    public async Task<bool> DeActiveApplication(Guid? applicationId)
    {
        var result = await fusionAuthAsyncClient.DeactivateApplicationAsync(applicationId);
        return result.WasSuccessful();
    }

    public async Task<bool> DeleteApplication(Guid? applicationId)
    {
        var result = await fusionAuthAsyncClient.DeleteApplicationAsync(applicationId);

        return result.WasSuccessful();
    }

    public async Task<ApplicationRole?> CreateApplicationRole(Guid? applicationId, ApplicationRole applicationRole)
    {
        var application = await fusionAuthAsyncClient.RetrieveApplicationAsync(applicationId);

        var appRequest = new ApplicationRequest
        {
            application = application.successResponse.application,
            role = applicationRole
        };
        var result = await fusionAuthAsyncClient.CreateApplicationRoleAsync(applicationId, null, appRequest);
        return result.WasSuccessful() ? result.successResponse.role : null;
    }

    public async Task<ApplicationRole?> UpdateApplicationRole(Guid? applicationId, ApplicationRole applicationRole)
    {
        var application = await fusionAuthAsyncClient.RetrieveApplicationAsync(applicationId);

        var appRequest = new ApplicationRequest
        {
            application = application.successResponse.application,
            role = applicationRole
        };
        appRequest.role.name = applicationRole.name;
        var result =
            await fusionAuthAsyncClient.UpdateApplicationRoleAsync(applicationId, applicationRole.id, appRequest);
        return result.WasSuccessful() ? result.successResponse.role : null;
    }

    public async Task<bool> DeleteApplicationRole(Guid? applicationId, Guid? roleId)
    {
        var result = await fusionAuthAsyncClient.DeleteApplicationRoleAsync(applicationId, roleId);
        return result.WasSuccessful();
    }

    public async Task<List<ApplicationRole>?> GetApplicationRolesByAppId(Guid? applicationId)
    {
        var result = await fusionAuthAsyncClient.RetrieveApplicationAsync(applicationId);
        return result.WasSuccessful() ? result.successResponse.application.roles : null;
    }

    public async Task<ApplicationRole?> GetApplicationRoleById(Guid? applicationId, Guid? roleId)
    {
        var result = await fusionAuthAsyncClient.RetrieveApplicationAsync(applicationId);
        var role = result.successResponse.application.roles.FirstOrDefault(r => r.id == roleId);
        return role ?? null;
    }

    public async Task<RoleResult?> GetApplicationRoleWithUsers(Guid? applicationId, Guid? roleId)
    {
        var result = await fusionAuthAsyncClient.RetrieveApplicationAsync(applicationId);
        var role = result.successResponse.application.roles.FirstOrDefault(r => r.id == roleId);
        if (role == null) return null;

        //Do a search on Fusion Users
        var searchRequest = new SearchRequest
        {
            search = new UserSearchCriteria
                { queryString = $"registrations.applicationId:{applicationId} AND registrations.roles:{role.name}" }
        };
        var usersResponse = await fusionAuthAsyncClient.SearchUsersByQueryAsync(searchRequest);
        if (!usersResponse.WasSuccessful()) return null;

        return new RoleResult
        {
            Users = usersResponse.successResponse.users, ApplicationName = result.successResponse.application.name,
            Role = role
        };
    }

    public async Task<List<Group>?> GetGroups()
    {
        var result = await fusionAuthAsyncClient.RetrieveGroupsAsync();
        return result.WasSuccessful() ? result.successResponse.groups : null;
    }

    public async Task<User?> AddUserIntoGroup(Guid id, Guid groupId)
    {
        var userFounded = await GetUser(id);
        var request = new UserRequest
        {
            user = userFounded
        };
        request.user?.memberships.Add(new GroupMember
        {
            userId = id,
            groupId = groupId
        });
        var result = await fusionAuthAsyncClient.UpdateUserAsync(id, request);
        return result.WasSuccessful() ? result.successResponse.user : null;
    }

    public async Task<Group?> CreateGroup(GroupRequest request)
    {
        var result = await fusionAuthAsyncClient.CreateGroupAsync(null, request);
        return result.WasSuccessful() ? result.successResponse.group : null;
    }

    public async Task<bool> DeleteGroup(Guid id)
    {
        var result = await fusionAuthAsyncClient.DeleteGroupAsync(id);
        return result.WasSuccessful();
    }

    public async Task<Group?> UpdateGroup(Guid? groupId, GroupRequest request)
    {
        var result = await fusionAuthAsyncClient.UpdateGroupAsync(groupId, request);
        return result.WasSuccessful() ? result.successResponse.group : null;
    }

    public async Task<bool> DeaActiveUser(Guid? id)
    {
        var result = await fusionAuthAsyncClient.DeactivateUserAsync(id);
        return result.WasSuccessful();
    }

    public async Task<bool> ActiveUser(Guid? id)
    {
        var result = await fusionAuthAsyncClient.ReactivateUserAsync(id);
        return result.WasSuccessful();
    }

    public async Task<UserCreationResult> AddUser(UserRequest request)
    {
        var result = await fusionAuthAsyncClient.CreateUserAsync(null, request);
        if (!result.WasSuccessful())
            return new UserCreationResult
                { Errors = _transformErrorService.TransformFieldErrors(result.errorResponse?.fieldErrors) };

        //get the received roles
        var existingRoles = request.user.registrations
            .FirstOrDefault(r => r.roles?.Count > 0)?.roles ?? [];

        var resultRegisterApp = await fusionAuthAsyncClient.RegisterAsync(result.successResponse.user.id,
            new RegistrationRequest
            {
                registration = new UserRegistration
                {
                    applicationId = request.applicationId,
                    roles = new List<string> { "user" }.Concat(existingRoles)
                        .ToList(), //adding selected roles to the default one("user")
                    username = request.user.username
                }
            });

        return !resultRegisterApp.WasSuccessful()
            ? new UserCreationResult
            {
                Errors = _transformErrorService.TransformFieldErrors(resultRegisterApp.errorResponse?.fieldErrors),
                User = result.successResponse.user
            }
            : new UserCreationResult { User = result.successResponse.user };
    }

    public async Task<UploadResult> UploadUserProfile(Guid? userId, IFormFile file)
    {
        try
        {
            // Validate user exists first
            var user = await fusionAuthAsyncClient.RetrieveUserAsync(userId);
            if (user.successResponse.user == null)
                return UploadResult.Failure("کاربر پیدا نشد", UploadErrorType.UserNotFound);

            // Validate file content (not just extension)
            if (!await IsValidImageFile(file))
                return UploadResult.Failure("فایل عکس نامعتبر", UploadErrorType.InvalidFile);

            string? oldFilePath = null;
            string newFilePath = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(user.successResponse.user.imageUrl))
                {
                    oldFilePath = await DeleteExistingFile(user.successResponse.user.imageUrl);
                }

                // Save new file
                newFilePath = await SaveNewFile(file);

                var tempImageUrl = $"{uploadFileSettings.Value.BaseUrl}/{Path.GetFileName(newFilePath)}";
                var updateRequest = new UserRequest
                {
                    user = new User
                    {
                        id = user.successResponse.user.id,
                        imageUrl = tempImageUrl
                    }
                };

                var updateResponse = await fusionAuthAsyncClient.UpdateUserAsync(userId, updateRequest);
                if (updateResponse.WasSuccessful()) return UploadResult.Success(tempImageUrl);
                await RollbackFileUpload(newFilePath);
                return UploadResult.Failure("بروزرسانی پروفایل کاربر با شکست مواجه شد", UploadErrorType.FusionAuthError);

            }
            catch (Exception ex)
            {
                await CleanupOnError(newFilePath, oldFilePath);
                return UploadResult.Failure($"بارگذاری شکست خورد: {ex.Message}", UploadErrorType.SystemError);
            }
        }
        catch (Exception ex)
        {
            return UploadResult.Failure($"خطای غیر قابل پیشبینی: {ex.Message}", UploadErrorType.SystemError);
        }
    }

    private async Task<bool> IsValidImageFile(IFormFile file)
    {
        try
        {
            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!uploadFileSettings.Value.AllowedExtensions.Contains(extension))
                return false;

            // Check file size
            if (file.Length > uploadFileSettings.Value.MaxFileSizeMB * 1024 * 1024)
                return false;

            // Basic file content validation - check file header
            await using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            await stream.ReadExactlyAsync(buffer, 0, 8);
            stream.Position = 0; // Reset position for later use

            // Check for common image file signatures
            return extension switch
            {
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".gif" => buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private async Task<string?> DeleteExistingFile(string imageUrl)
    {
        var existingFileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
        var existingFilePath = Path.Combine(uploadFileSettings.Value.UploadPath, existingFileName);

        if (!File.Exists(existingFilePath)) return null;
        await Task.Run(() => File.Delete(existingFilePath));
        return existingFilePath;
    }

    private async Task<string> SaveNewFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFileSettings.Value.UploadPath, safeFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

    private Task RollbackFileUpload(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    private async Task CleanupOnError(string newFilePath, string? oldFilePath)
    {
        if (!string.IsNullOrEmpty(newFilePath) && File.Exists(newFilePath))
        {
            await Task.Run(() => File.Delete(newFilePath));
        }

        if (!string.IsNullOrEmpty(oldFilePath) && !File.Exists(oldFilePath))
        {
        }
    }

    public async Task<bool> DeleteUserProfile(Guid? userId)
    {
        var userResult = await fusionAuthAsyncClient.RetrieveUserAsync(userId);
        if (!userResult.WasSuccessful() || userResult.successResponse.user == null) 
        {
            return false;
        }

        var user = userResult.successResponse.user;

        if (string.IsNullOrEmpty(user.imageUrl))
        {
            return true;
        }

        var existingFileName = Path.GetFileName(new Uri(user.imageUrl).LocalPath);
        var existingFilePath = Path.Combine(uploadFileSettings.Value.UploadPath, existingFileName);

        try
        {
            if (File.Exists(existingFilePath))
            {
                await Task.Run(() => File.Delete(existingFilePath));
            }
        }
        catch (Exception)
        {
            return false;
        }
    
        user.imageUrl = null;
        var updateRequest = new UserRequest
        {
            user = user
        };
    
        var updateResponse = await fusionAuthAsyncClient.UpdateUserAsync(userId, updateRequest);

        return updateResponse.WasSuccessful();
    }
    
    public async Task<bool> SetUserSecurityQuestions(Guid? userId, UserSecurityQuestionsDTO questions)
    {
        //Test
        var userResult = await fusionAuthAsyncClient.RetrieveUserAsync(userId);
        if (userResult.successResponse.user == null) return false;

        userResult.successResponse.user.data["securityQuestions"] = JsonSerializer.Serialize(questions);
        var updateRequest = new UserRequest
        {
            user = userResult.successResponse.user
        };
        var updateResponse = await fusionAuthAsyncClient.UpdateUserAsync(userId, updateRequest);

        return updateResponse.WasSuccessful();
    }

    public async Task<bool> VerifySecurityQuestions(UserSecurityQuestionsDTO questions)
    {
        var userResult = await fusionAuthAsyncClient.RetrieveUserByEmailAsync(questions.Email);

        var userData = userResult.successResponse.user?.data;
        if (userData == null || !userData.TryGetValue("securityQuestions", out var value))
        {
            return false;
        }

        if (value == null) return false;
        var jsonString = value.ToString();
        if (jsonString == null)
            return false;
        
        var storedQuestions = JsonSerializer.Deserialize<UserSecurityQuestionsDTO>(jsonString);
        if (storedQuestions == null) return false;
        var storedQuestionsDict = storedQuestions.Questions.ToDictionary(q => q.QuestionId, q => q.Answer);

        foreach (var submittedQuestion in questions.Questions)
        {
            if (!storedQuestionsDict.TryGetValue(submittedQuestion.QuestionId, out var storedAnswer))
            {
                return false;
            }

            if (!string.Equals(storedAnswer, submittedQuestion.Answer, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<User?> UpdateUser(Guid? id, UserRequest request)
    {
        var result = await fusionAuthAsyncClient.UpdateUserAsync(id, request);
        return result.WasSuccessful() ? result.successResponse.user : null;
    }

    public async Task<bool> DeleteUser(Guid? id)
    {
        var result = await fusionAuthAsyncClient.DeleteUserAsync(id);
        return result.WasSuccessful();
    }

    public async Task<bool> ChangePasswordForLogin(string changePasswordId, string password, string email)
    {
        var userResult = await fusionAuthAsyncClient.RetrieveUserByEmailAsync(email);
        if (userResult.successResponse.user == null) return false;

        var result = await fusionAuthAsyncClient.ChangePasswordAsync(changePasswordId, new ChangePasswordRequest
        {
            changePasswordId = changePasswordId,
            currentPassword = userResult.successResponse.user.password,
            password = password,
            loginId = email
        });

        return result.WasSuccessful();
    }

    public async Task<IDictionary<string, object>?> ChangePassword(string id, string currentPassword, string password)
    {
        var resultForgotPassword = await fusionAuthAsyncClient.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            loginId = id,
            username = id
        });
        if (!resultForgotPassword.WasSuccessful()) return null;
        var result = await fusionAuthAsyncClient.ChangePasswordAsync(id, new ChangePasswordRequest
        {
            changePasswordId = resultForgotPassword.successResponse.changePasswordId,
            currentPassword = currentPassword,
            password = password,
            loginId = id
        });
        return result.successResponse.state;
    }

    public async Task<IDictionary<string, object>?> ChangePassword(string id, string changePasswordId,
        string currentPassword, string password)
    {
        var resultForgotPassword = await fusionAuthAsyncClient.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            loginId = id,
            username = id
        });
        if (!resultForgotPassword.WasSuccessful()) return null;
        var result = await fusionAuthAsyncClient.ChangePasswordAsync(id, new ChangePasswordRequest
        {
            changePasswordId = resultForgotPassword.successResponse.changePasswordId,
            currentPassword = currentPassword,
            password = password,
            loginId = id
        });
        return result.successResponse.state;
    }

    public async Task<User?> ResetPassword(ResetPasswordRequest passwordRequest, User userInfo)
    {
        userInfo.password = passwordRequest.newPassword;
        var request = new UserRequest
        {
            user = userInfo
        };
        var result = await fusionAuthAsyncClient.UpdateUserAsync(passwordRequest.Id, request);
        return result.WasSuccessful() ? result.successResponse.user : null;
    }

    public async Task<UserRemoveApplicationRequest?> RemoveFromApplication(
        UserRemoveApplicationRequest? userRemoveApplicationRequest)
    {
        if (userRemoveApplicationRequest == null) return null;
        var result = await fusionAuthAsyncClient.DeleteRegistrationWithRequestAsync(userRemoveApplicationRequest.Id,
            userRemoveApplicationRequest.ApplicationId, null);

        return result.WasSuccessful() ? userRemoveApplicationRequest : null;
    }

    public async Task<User?> RegisterApplication(User? user, Guid applicationId)
    {
        if (user == null) return null;
        var registrationRequest = new RegistrationRequest();
        var registration = new UserRegistration
        {
            applicationId = applicationId
        };
        registrationRequest.sendSetPasswordEmail = false;
        registrationRequest.skipRegistrationVerification = true;
        registrationRequest.skipVerification = true;
        registrationRequest.registration = registration;

        var result = await fusionAuthAsyncClient.RegisterAsync(user.id, registrationRequest);
        return result.WasSuccessful() ? await GetUser(user.id ?? Guid.Empty) : null;
    }

    public async Task<User?> RegisterRole(User? user, Guid? applicationId, List<string>? roles)
    {
        var registrationRequest = new RegistrationRequest();
        var registration = new UserRegistration
        {
            applicationId = applicationId,
            roles = roles
        };
        registrationRequest.sendSetPasswordEmail = false;
        registrationRequest.skipRegistrationVerification = true;
        registrationRequest.skipVerification = true;
        registrationRequest.registration = registration;
        if (user == null) return null;
        var result = await fusionAuthAsyncClient.UpdateRegistrationAsync(user.id, registrationRequest);
        return result.WasSuccessful() ? await GetUser(user.id ?? Guid.Empty) : null;
    }

    public async Task<SecretResponse?> GetTwoFactorSecretCode()
    {
        var response = await fusionAuthAsyncClient.GenerateTwoFactorSecretAsync();
        return response.WasSuccessful() ? response.successResponse : null;
    }

    public async Task<TwoFactorResponse?> EnableUserTwoFactorMethod(TwoFactorRequestFromClientDTO request)
    {
        var response = await fusionAuthAsyncClient.EnableTwoFactorAsync(request.UserId, new TwoFactorRequest
        {
            method = "authenticator",
            secret = request.secret,
            secretBase32Encoded = request.secretBase32Encoded,
            code = request.code
        });
        return response.WasSuccessful() ? response.successResponse : null;
    }

    public async Task<TwoFactorResponse?> DisableUserTwoFactorMethod(TwoFactorDisableRequestDTO request )
    { 
        var response = await fusionAuthAsyncClient.DisableTwoFactorAsync(request.UserId, "authenticator", request.Code);
        return response.WasSuccessful() ? null : new TwoFactorResponse(); 
    }

    public async Task<TwoFactorStatusResponse?> CheckEnabledTwoFactorAuthentication(Guid userId)
    {
        var response = await fusionAuthAsyncClient.RetrieveTwoFactorStatusAsync(userId, null, null);
        return response.WasSuccessful() ? response.successResponse : null;
    }

    public async Task<LoginResponse?> LoginWithTwoFactor(TwoFactorLoginDTO request)
    {
        var response = await fusionAuthAsyncClient.TwoFactorLoginAsync(new TwoFactorLoginRequest
        {
            code = request.code,
            applicationId = request.applicationId,
            ipAddress = request.ipAddress,
            twoFactorId = request.twoFactorId,
            noJWT = true,
            newDevice = true,
            trustComputer = false
        });
        return response.WasSuccessful() ? response.successResponse : null;
    }
}