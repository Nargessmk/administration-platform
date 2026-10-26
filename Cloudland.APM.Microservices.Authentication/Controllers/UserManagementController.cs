using System.Net;
using Asp.Versioning;
using io.fusionauth.domain;
using io.fusionauth.domain.api;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.Authentication.Services;
using Cloudland.APM.Models;
using Cloudland.APM.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PasswordGenerator;

namespace Cloudland.APM.Microservices.Authentication.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize(Roles = "admin, user")]
public class UserManagementController(IUserManagementService userManagementService,
    IOptions<FileUploadSettings> uploadFileSettings) : ControllerBase
{
    [HttpGet("{searchKey}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [Authorize(Roles = "admin")]
    public virtual async Task<IActionResult> Get(string? searchKey)
    {
        return Ok(await userManagementService.GetUsers(searchKey));
    }

    [HttpPost("ByIds")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [Authorize(Roles = "admin")]
    public virtual async Task<IActionResult> GetByIds([FromBody] Guid[] ids)
    {
        return Ok(await userManagementService.GetUsersByIds(ids));
    }

    [HttpGet("Paging/{searchKey}/{pageIndex:int}/{pageSize:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetPaging(string searchKey, int pageIndex, int pageSize)
    {
        return Ok(await userManagementService.GetUsersPaging(new PagingQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            QueryString = searchKey
        }));
    }

    [HttpGet("Applications")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetApplications()
    {
        return Ok(await userManagementService.GetApplications());
    }

    [HttpGet("ApplicationById/{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetApplicationById(Guid? id)
    {
        return Ok(await userManagementService.GetApplicationById(id));
    }

    [HttpPatch("UpdateApplication/{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> UpdateApplication(Guid? id, [FromBody] ApplicationRequest request)
    {
        return Ok(await userManagementService.UpdateApplication(id, request));
    }

    [HttpPost("CreateApplication")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> CreateApplication([FromBody] ApplicationRequest request)
    {
        return Ok(await userManagementService.CreateApplication(request));
    }

    [HttpPost("ReActivateApplication/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> ReActivateApplication([FromRoute] Guid? applicationId)
    {
        return Ok(await userManagementService.ReActiveApplication(applicationId));
    }
    
    [HttpPost("DeActivateApplication/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> DeActivateApplication([FromRoute] Guid? applicationId)
    {
        return Ok(await userManagementService.DeActiveApplication(applicationId));
    }

    [HttpDelete("DeleteApplication/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> DeleteApplication(Guid? applicationId)
    {
        return Ok(await userManagementService.DeleteApplication(applicationId));
    }

    [HttpPost("CreateApplicationRole/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> CreateApplicationRole(Guid? applicationId,
        [FromBody] ApplicationRole applicationRole)
    {
        return Ok(await userManagementService.CreateApplicationRole(applicationId, applicationRole));
    }

    [HttpPatch("UpdateApplicationRole/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> UpdateApplicationRole(Guid? applicationId, [FromBody] ApplicationRole role)
    {
        return Ok(await userManagementService.UpdateApplicationRole(applicationId, role));
    }

    [HttpDelete("DeleteApplicationRole/{applicationId:guid}/{roleId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> DeleteApplicationRole(Guid? applicationId, Guid? roleId)
    {
        return Ok(await userManagementService.DeleteApplicationRole(applicationId, roleId));
    }

    [HttpGet("GetApplicationRoles/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetApplicationRoles(Guid? applicationId)
    {
        return Ok(await userManagementService.GetApplicationRolesByAppId(applicationId));
    }

    [HttpGet("GetApplicationRoleUsers/{applicationId:guid}/{roleId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetApplicationRoleUsers(Guid? applicationId, Guid? roleId)
    {
        return Ok(await userManagementService.GetApplicationRoleWithUsers(applicationId, roleId));
    }

    [HttpGet("GetApplicationRole/{applicationId:guid}/{roleId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetApplicationRole(Guid? applicationId, Guid? roleId)
    {
        return Ok(await userManagementService.GetApplicationRoleById(applicationId, roleId));
    }

    [HttpGet("Groups")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> GetGroups()
    {
        return Ok(await userManagementService.GetGroups());
    }

    [HttpGet("AddIntoGroup/{userId:guid}/{groupId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> AddIntoGroup(Guid userId, Guid groupId)
    {
        return Ok(await userManagementService.AddUserIntoGroup(userId, groupId));
    }
    
    [HttpPost("CreateGroup")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> CreateGroup(GroupRequest request)
    {
        return Ok(await userManagementService.CreateGroup(request));
    }
    
    [HttpPut("UpdateGroup/{groupId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> UpdateGroup(Guid? groupId,GroupRequest request)
    {
        return Ok(await userManagementService.UpdateGroup(groupId, request));
    }
    

    [HttpDelete("DeleteGroup/{groupId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> DeleteGroup(Guid groupId)
    {
        return Ok(await userManagementService.DeleteGroup(groupId));
    }

    [HttpPost("AddUser")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> AddUser(UserRequest request)
    {
        var result = await userManagementService.AddUser(request);
        if (result.Errors?.Count > 0 && result.Errors != null)
            return BadRequest(result.Errors);
        return Ok(await userManagementService.AddUser(request));
    }

    [HttpPut("UploadUserProfile/{userId:guid}")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> UploadUserProfile([FromRoute]Guid? userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("فایلی بارگذاری نشد");
        
        var result = await userManagementService.UploadUserProfile(userId, file);
        
        if (result.IsSuccess)
            return Ok(new { message = "فایل با موفقیت بارگذاری شد", imageUrl = result.ImageUrl });
        
        return result.ErrorType switch
        {
            UploadErrorType.UserNotFound => BadRequest("کاربر پیدا نشد"),
            UploadErrorType.InvalidFile => BadRequest("فایل نامعتبر"),
            UploadErrorType.FileTooLarge => BadRequest($"حجم فایل زیاد است. حداکثر حجم: {uploadFileSettings.Value.MaxFileSizeMB}MB"),
            UploadErrorType.InvalidFileType => BadRequest($"نوع فایل نامعتبر. نوع قابل قبول: {string.Join(", ", uploadFileSettings.Value.AllowedExtensions)}"),
            UploadErrorType.FusionAuthError => BadRequest("بروزرسانی فایل کاربر با شکست مواجه شد"),
            _ => BadRequest(result.ErrorMessage ?? "بارگذاری با شکست مواجه شد")
        };
    }

    [HttpDelete("DeleteUserProfile/{userId:guid}")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> DeleteUserProfile([FromRoute]Guid? userId)
    {
        if (userId == null || userId == Guid.Empty)
            return BadRequest("Invalid user ID");
        
        var result = await userManagementService.DeleteUserProfile(userId);
        if (result)
            return Ok(new { message = "User profile image deleted successfully" });
        
        return BadRequest("Failed to delete user profile image");
    }

    [HttpPut("SetUserSecurityQuestions/{userId:guid}")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> SetUserSecurityQuestions(Guid? userId,[FromBody]UserSecurityQuestionsDTO questions)
    {
        return Ok(await userManagementService.SetUserSecurityQuestions(userId, questions));
    }
    
    [HttpPatch("UpdateUser")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> Post(UserRequest request)
    {
        return Ok(await userManagementService.UpdateUser(request.user.id, request));
    }

    [HttpDelete("DeleteUser/{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> DeleteUser(Guid? id)
    {
        return Ok(await userManagementService.DeleteUser(id));
    }

    [HttpGet("DeActiveUser/{Id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> DeActiveUser(Guid? id)
    {
        return Ok(await userManagementService.DeaActiveUser(id));
    }

    [HttpGet("ReActiveUser/{Id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> ReActiveUser(Guid? id)
    {
        return Ok(await userManagementService.ActiveUser(id));
    }

    [HttpPost("ChangePassword")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [Authorize]
    public virtual async Task<IActionResult> ChangePassword(PasswordRequest request)
    {
        return Ok(await userManagementService.ChangePassword(request.id, request.currentPassword, request.password));
    }

    [HttpPost("ChangePasswordRequired")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [AllowAnonymous]
    public virtual async Task<IActionResult> ChangePasswordRequired(PasswordRequestRequired request)
    {
        return Ok(await userManagementService.ChangePassword(request.id, request.changePasswordId,
            request.currentPassword, request.password));
    }

    [HttpPost("ResetPassword")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> ResetPassword(ResetPasswordRequest passwordRequest)
    {
        var pwd = new Password(true, true, true,
            false, 8);
        passwordRequest.newPassword = pwd.Next();
        var userInfo = await userManagementService.GetUser(passwordRequest.Id);
        if (userInfo == null) return BadRequest();
        var result = await userManagementService.ResetPassword(passwordRequest, userInfo);
        return result != null ? Ok(passwordRequest.newPassword) : BadRequest();
    }

    [HttpPost("RemoveAppFromUser")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public virtual async Task<IActionResult> RemoveAppFromUser(UserRemoveApplicationRequest passwordRequest)
    {
        var result = await userManagementService.RemoveFromApplication(passwordRequest);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpGet("RegisterApplication/{userId:guid}/{applicationId:guid}")]
    [Authorize(Roles = "admin")]
    public virtual async Task<IActionResult> RegisterApplication(Guid userId, Guid applicationId)
    {
        var user = await userManagementService.GetUser(userId);
        var result = await userManagementService.RegisterApplication(user, applicationId);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpPost("UpdateRoles")]
    [Authorize(Roles = "admin")]
    public virtual async Task<IActionResult> UpdateRoles(UserRoles userRoles)
    {
        var user = await userManagementService.GetUser(userRoles.UserId);
        var result = await userManagementService.RegisterRole(user, userRoles.ApplicationId, userRoles.Roles);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpGet("GetTwoFactorSecretCode")]
    [Authorize(Roles = "admin, user")]
    public virtual async Task<IActionResult> GetTwoFactorSecretCode()
    {
        var details = await userManagementService.GetTwoFactorSecretCode();
        var responseData = new TwoFactorSecretDetailDTO
        {
            secret = details?.secret,
            secretBase32Encoded = details?.secretBase32Encoded
        };
        return Ok(responseData);
    }

    [HttpPost("EnableTwoFactorAuthentication")]
    [Authorize(Roles = "admin, user")]
    public virtual async Task<IActionResult> EnableTwoFactorAuthentication(TwoFactorRequestFromClientDTO request)
    {
        var response = await userManagementService.EnableUserTwoFactorMethod(request);
        return Ok(response);
    }

    [HttpPost("DisableTwoFactorAuthentication")]
    [Authorize(Roles = "admin, user")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> DisableTwoFactorAuthentication([FromBody] TwoFactorDisableRequestDTO request)
    {
        var response = await userManagementService.DisableUserTwoFactorMethod(request);
        if (response == null)
            return Ok(new { message = "احراز هویت دو مرحله‌ای با موفقیت غیرفعال شد" });
        
        return BadRequest("غیرفعال کردن احراز هویت دو مرحله‌ای با شکست مواجه شد");
    }

    [HttpGet("GetTwoFactorAuthenticationStatus")]
    [Authorize(Roles = "admin, user")]
    public virtual async Task<IActionResult> GetTwoFactorAuthenticationStatus()
    {
        var userId = new Guid(User.Claims.ToList()[10].Value);
        var response = await userManagementService.CheckEnabledTwoFactorAuthentication(userId);
        return Ok(response);
    }
}