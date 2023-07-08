using System.Net;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model.Request;
using HelpMeApi.User.Model.Response;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.User;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] UserSignUpRequestModel body)
    {
        var (resultState, user) = await _userService.SignUp(body);

        if (resultState != StateCode.Ok)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        var state = _userService.ParseResponseState(resultState, user);
        
        return new JsonResult(state);
    }
    
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] UserSignInRequestModel body)
    {
        var (resultState, user) = await _userService.SignIn(body);

        if (resultState != StateCode.Ok)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        var state = _userService.ParseResponseState(resultState, user);
        
        return new JsonResult(state);
    }
    
    [HttpPost("sign-out")]
    [AuthRequired(ForbidBanned = false)]
    public new async Task<IActionResult> SignOut()
    {
        var user = (UserEntity)HttpContext.Items["User"]!;
        var tokenId = (string)HttpContext.Items["TokenId"]!;
        
        await _userService.SignOut(user.Id.ToString(), tokenId);

        return new JsonResult(DefaultState.Ok);
    }
    
    [HttpPost("delete")]
    [AuthRequired(ForbidBanned = false)]
    public async Task<IActionResult> Delete([FromBody] UserDeleteRequestModel body)
    {
        var user = (UserEntity)HttpContext.Items["User"]!;
        var result = await _userService.Delete(body, user);
        return new JsonResult(result);
    }
    
    [HttpPost("check-nickname-availability")]
    public async Task<IActionResult> CheckNicknameAvailability(string nickname)
    {
        var isNicknameAvailable = await _userService.IsNicknameAvailable(nickname);
        var state = StateModel<UserNicknameAvailableResponseModel>.ParseOk(new UserNicknameAvailableResponseModel
        {
            Available = isNicknameAvailable
        });
        return new JsonResult(state);
    }
}
