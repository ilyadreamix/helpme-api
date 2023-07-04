using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HelpMeApi.Account.Model.Request;
using HelpMeApi.Common.Auth;
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

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] AccountSignUpRequestModel body)
    {
        var (resultState, account) = await _accountService.SignUp(body);
        var state = _accountService.ParseAccountResponseState(resultState, account);
        return new JsonResult(state);
    }
    
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] AccountSignInRequestModel body)
    {
        var (resultState, account) = await _accountService.SignIn(body);
        var state = _accountService.ParseAccountResponseState(resultState, account);
        return new JsonResult(state);
    }
    
    [HttpPost("sign-out")]
    [AuthRequired]
    public new async Task<IActionResult> SignOut()
    {
        var account = (AccountEntity)HttpContext.Items["Account"]!;
        var tokenId = (string)HttpContext.Items["TokenId"]!;
        
        await _accountService.SignOut(account.Id.ToString(), tokenId);

        return new JsonResult(DefaultState.Ok);
    }
}
