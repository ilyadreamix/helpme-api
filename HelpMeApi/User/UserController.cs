using HelpMeApi.Common.Auth;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
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
        var iState = await _userService.SignUp(body);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
    
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] UserSignInRequestModel body)
    {
        var iState = await _userService.SignIn(body);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
    
    [HttpPost("sign-out")]
    [AuthRequired(ForbidBanned = false)]
    public new async Task<IActionResult> SignOut()
    {
        await _userService.SignOut();
        return new JsonResult(DefaultState.Ok);
    }
    
    [HttpPost("delete")]
    [AuthRequired(ForbidBanned = false)]
    public async Task<IActionResult> Delete([FromBody] UserDeleteRequestModel body)
    {
        var iState = await _userService.Delete(body);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
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
