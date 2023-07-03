using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HelpMeApi.Account.Model.Request;
using HelpMeApi.Common.State;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.Account;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    // TODO: Custom authorization middleware
    [AllowAnonymous]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] AccountSignUpRequestModel body)
    {
        var (resultState, account) = await _accountService.SignUp(body);
        var state = _accountService.ParseAccountResponseState(resultState, account);
        return new JsonResult(state);
    }
    
    // TODO: Custom authorization middleware
    [AllowAnonymous]
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] AccountSignInRequestModel body)
    {
        var (resultState, account) = await _accountService.SignIn(body);
        var state = _accountService.ParseAccountResponseState(resultState, account);
        return new JsonResult(state);
    }
    
    // TODO: Custom authorization middleware
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("sign-out")]
    public new async Task<IActionResult> SignOut()
    {
        var claims = User.Claims.ToList();

        var sessionId = claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var userId = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        await _accountService.SignOut(userId!, sessionId!);

        return new JsonResult(DefaultState.Ok);
    }
}
