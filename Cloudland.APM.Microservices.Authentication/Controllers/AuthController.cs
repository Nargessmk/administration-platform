using System.Net;
using Asp.Versioning;
using io.fusionauth;
using io.fusionauth.domain.api.user;
using io.fusionauth.domain.search;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.Authentication.Services;
using Cloudland.APM.Models.Auth;
using Cloudland.APM.Models.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = io.fusionauth.domain.api.LoginRequest;

namespace Cloudland.APM.Microservices.Authentication.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class AuthController(
    IJwtService jwtService,
    IFusionAuthAsyncClient fusionClient,
    IUserManagementService userManagementService) : ControllerBase
{
    [HttpPost("Authenticate")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Authenticate(LoginModel model)
    {
        if (HttpContext.Connection.RemoteIpAddress == null)
            return BadRequest(new { model = ModelState, message = "نام کاربری و یا کلمه عبور اشتباه می باشد" });
        var clientResponse = await fusionClient.LoginAsync(new LoginRequest
        {
            loginId = model.userName,
            password = model.password,
            applicationId = Guid.Parse(model.applicationId),
            noJWT = false,
            ipAddress = HttpContext.GetServerVariable("X-Forwarded-For") ??
                        HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        switch (clientResponse)
        {
            case { statusCode: 203 }:
            {
                var changePasswordRequired = new
                {
                    clientResponse.successResponse.changePasswordId,
                    loginId = model.userName,
                    model.applicationId,
                    statusCode = 203
                };
                return BadRequest(changePasswordRequired);
            }
            case { statusCode: 242 }:
            {
                return BadRequest(new
                {
                    clientResponse.successResponse.twoFactorId,
                    model.applicationId,
                    statusCode = 242
                });
            }
        }

        if (clientResponse == null || !clientResponse.WasSuccessful()) return Unauthorized();

        model.roles =
            clientResponse.successResponse.user.registrations
                .First(q => q.applicationId == Guid.Parse(model.applicationId)).roles.ToArray();
        
        var accessToken = jwtService.GenerateSecurityToken(model, clientResponse.successResponse.user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Set the refresh token as an HttpOnly cookie
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true, // Makes the cookie inaccessible to client-side scripts
            Secure = true,   // Ensures the cookie is sent only over HTTPS
            SameSite = SameSiteMode.Lax, // Helps mitigate CSRF attacks
            Expires = DateTimeOffset.UtcNow.AddDays(7) // Set an appropriate expiration time
        });

        var userInfo = new
        {
            userName = model.userName.ToLower(),
            token = accessToken, // Access token is returned in the response body
            fullUserInfo = clientResponse.successResponse.user,
            model.roles,
            statusCode = 202
        };
        return Ok(userInfo);
    }

    [HttpPost("AuthenticateWithTwoFactor")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> AuthenticateWithTwoFactor(TwoFactorLoginDTO model)
    {
        if (HttpContext.Connection.RemoteIpAddress == null)
            return BadRequest(new { model = ModelState, message = "نام کاربری و یا کلمه عبور اشتباه می باشد" });
        model.ipAddress = HttpContext.GetServerVariable("X-Forwarded-For") ??
                          HttpContext.Connection.RemoteIpAddress?.ToString();
        var clientResponse = await userManagementService.LoginWithTwoFactor(model);
        if (clientResponse == null) return BadRequest();
        
        var accessToken = jwtService.GenerateSecurityToken(new LoginModel
        {
            applicationId = model.applicationId.ToString(),
            userName = clientResponse.user.username,
            roles = clientResponse.user.registrations
                .First(q => q.applicationId == model.applicationId).roles.ToArray()
        }, clientResponse.user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Set the refresh token as an HttpOnly cookie
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        var userInfo = new
        {
            userName = clientResponse.user.username,
            token = accessToken, // Access token is returned in the response body
            fullUserInfo = clientResponse.user,
            roles = clientResponse.user.registrations
                .First(q => q.applicationId == model.applicationId).roles.ToArray(),
            statusCode = 202
        };
        return Ok(userInfo);
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] TokenRequest request)
    {
        if (request.LoginInfo == null) return BadRequest();
        
        // Retrieve the refresh token from HttpOnly cookie
        if (!Request.Cookies.TryGetValue("refreshToken", out _))
        {
            return Unauthorized("Invalid refresh token.");
        }

        var newAccessToken = jwtService.GenerateSecurityToken(request.LoginInfo, request.User);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        
        // Set the new refresh token as an HttpOnly cookie
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new { AccessToken = newAccessToken }); // Only return the new access token
    }

    [HttpPost("ChangePasswordRequired")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [AllowAnonymous]
    public virtual async Task<IActionResult> ChangePasswordRequired(PasswordRequestRequired request)
    {
        return Ok(await userManagementService.ChangePassword(request.id, request.changePasswordId,
            request.currentPassword, request.password));
    }

    [HttpGet("ListOfUsers")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Get()
    {
        var result = await fusionClient.SearchUsersByQueryAsync(new SearchRequest
        {
            search = new UserSearchCriteria
            {
                accurateTotal = true,
                queryString = "* AND NOT registrations.applicationId:3c219e58-ed0e-4b18-ad48-f4f92793ae32",
                startRow = 0,
                numberOfResults = 1500
            }
        });
        if (result.WasSuccessful())
            return Ok(result.successResponse.users);
        return BadRequest();
    }

    [HttpGet("ListOfUsers/{numberOfResults:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ListOfUsers(int numberOfResults)
    {
        var result = await fusionClient.SearchUsersByQueryAsync(new SearchRequest
        {
            search = new UserSearchCriteria
            {
                accurateTotal = true,
                queryString = "* AND NOT registrations.applicationId:3c219e58-ed0e-4b18-ad48-f4f92793ae32",
                startRow = 0,
                numberOfResults = numberOfResults + 1
            }
        });
        if (result.WasSuccessful())
            return Ok(result.successResponse.users);

        return BadRequest();
    }
    
    [HttpPost("VerifySecurityQuestions")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> VerifySecurityQuestions([FromBody]UserSecurityQuestionsDTO questions)
    {
        var verificationResult = await userManagementService.VerifySecurityQuestions(questions);
        if (!verificationResult)
            return BadRequest("پاسخ سوالات درست نمی باشد.");

        var forgetPassword = await fusionClient.ForgotPasswordAsync(new ForgotPasswordRequest()
        {
            loginId = questions.Email,
            sendForgotPasswordEmail = false,
        });

        if (!forgetPassword.WasSuccessful())
            return  StatusCode(500, "An unexpected error occurred");
            
        return Ok(forgetPassword.successResponse.changePasswordId);
    }
    
    [HttpPost("ChangePassword/{changePasswordId}")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> ChangePasswordForLogin([FromRoute]string changePasswordId
        ,[FromQuery]string password,[FromQuery]string email)
    {
        var result = await userManagementService.ChangePasswordForLogin(changePasswordId, password, email);
        return Ok(result);
    }
    

    [HttpPost("Logout")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Logout(string refreshToken)
    {
        // Clear the refresh token cookie on logout
        Response.Cookies.Delete("refreshToken"); 
        var result = await fusionClient.LogoutAsync(true, refreshToken);
        return Ok(result);
    }

    [HttpGet("me/{userId:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Me(Guid userId)
    {
        return Ok(await userManagementService.GetUsersByIds([userId]));
    }
}