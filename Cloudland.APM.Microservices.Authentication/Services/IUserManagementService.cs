using io.fusionauth.domain;
using io.fusionauth.domain.api;
using io.fusionauth.domain.api.twoFactor;
using Cloudland.APM.DTO;
using Cloudland.APM.Models;
using Cloudland.APM.Models.Auth;
using Cloudland.APM.Models.ResultModel;

namespace Cloudland.APM.Microservices.Authentication.Services;

public interface IUserManagementService
{
    Task<List<User>> GetUsers(string? searchKey);
    Task<List<User>> GetUsersByIds(Guid[] ids);
    Task<List<User>?> GetUsersByIdsPaging(PagingQuery searchRequest);
    Task<PagedResult<User>> GetUsersPaging(PagingQuery searchRequest);
    Task<User?> GetUser(Guid id);
    Task<List<Application>?> GetApplications();
    Task<Application?> GetApplicationById(Guid? id);
    Task<Application?> UpdateApplication(Guid? id, ApplicationRequest request);
    Task<Application?> CreateApplication(ApplicationRequest application);
    Task<bool> ReActiveApplication(Guid? applicationId);
    Task<bool> DeActiveApplication(Guid? applicationId);
    Task<bool> DeleteApplication(Guid? applicationId);
    Task<ApplicationRole?> CreateApplicationRole(Guid? applicationId, ApplicationRole applicationRole);
    Task<bool> DeleteApplicationRole(Guid? applicationId, Guid? roleId);
    Task<ApplicationRole?> UpdateApplicationRole(Guid? applicationId, ApplicationRole applicationRole);
    Task<List<ApplicationRole>?> GetApplicationRolesByAppId(Guid? applicationId);
    Task<ApplicationRole?> GetApplicationRoleById(Guid? applicationId, Guid? roleId);
    Task<RoleResult?> GetApplicationRoleWithUsers(Guid? applicationId, Guid? roleId);
    Task<List<Group>?> GetGroups();
    Task<User?> AddUserIntoGroup(Guid id, Guid groupId);
    Task<Group?> CreateGroup(GroupRequest request);
    Task<bool> DeleteGroup(Guid groupId);
    Task<Group?> UpdateGroup(Guid? groupId, GroupRequest request);
    Task<bool> DeaActiveUser(Guid? id);
    Task<bool> ActiveUser(Guid? id);
    Task<UserCreationResult> AddUser(UserRequest request);
    Task<UploadResult> UploadUserProfile(Guid? userId, IFormFile file);
    Task<bool> DeleteUserProfile(Guid? userId);
    Task<bool> SetUserSecurityQuestions(Guid? userId, UserSecurityQuestionsDTO questions);
    Task<bool> VerifySecurityQuestions(UserSecurityQuestionsDTO questions);
    Task<bool> ChangePasswordForLogin(string changePasswordId,string password,string email);
    Task<User?> UpdateUser(Guid? id, UserRequest request);
    Task<bool> DeleteUser(Guid? id);
    Task<IDictionary<string, object>?> ChangePassword(string id, string currentPassword, string password);

    Task<IDictionary<string, object>?> ChangePassword(string id, string changePasswordId, string currentPassword,
        string password);

    Task<User?> ResetPassword(ResetPasswordRequest passwordRequest, User userInfo);
    Task<UserRemoveApplicationRequest?> RemoveFromApplication(UserRemoveApplicationRequest? passwordRequest);
    Task<User?> RegisterApplication(User? user, Guid applicationId);
    Task<User?> RegisterRole(User? user, Guid? applicationId, List<string>? roles);
    Task<SecretResponse?> GetTwoFactorSecretCode();
    Task<TwoFactorResponse?> EnableUserTwoFactorMethod(TwoFactorRequestFromClientDTO request);
    Task<TwoFactorResponse?> DisableUserTwoFactorMethod(TwoFactorDisableRequestDTO request);
    Task<TwoFactorStatusResponse?> CheckEnabledTwoFactorAuthentication(Guid userId);
    Task<LoginResponse?> LoginWithTwoFactor(TwoFactorLoginDTO request);
}