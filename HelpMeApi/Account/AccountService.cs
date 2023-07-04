using System.Text.Json.Nodes;
using HelpMeApi.Account.Model;
using HelpMeApi.Account.Model.Request;
using HelpMeApi.Account.Model.Response;
using HelpMeApi.Common;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.GoogleOAuth;
using HelpMeApi.Common.Hash;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.Profile;
using HelpMeApi.Profile.Model;
using Microsoft.EntityFrameworkCore;

namespace HelpMeApi.Account;

public class AccountService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GoogleOAuthService _oauthService;
    private readonly HashService _hashService;
    private readonly AuthService _authService;

    public AccountService(
        ApplicationDbContext dbContext,
        GoogleOAuthService oauthService,
        HashService hashService,
        AuthService authService)
    {
        _dbContext = dbContext;
        _oauthService = oauthService;
        _hashService = hashService;
        _authService = authService;
    }
    
    public async Task<(StateCode, AccountEntity?)> SignUp(AccountSignUpRequestModel body)
    {
        if (body.Age is < 14)
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.TooYoung, null);
        }
        
        var alreadyExistsNickname = await _dbContext.Profiles.AnyAsync(record =>
            record.Nickname == body.Nickname);

        if (alreadyExistsNickname)
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.NicknameUnavailable, null);
        }

        var (isIdTokenValid, idToken) = await _oauthService.VerifyIdToken(body.OAuthIdToken);

        if (!isIdTokenValid)
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.InvalidIdToken, null);
        }

        if (idToken!.EmailVerified != "true")
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.EmailIsNotVerified, null);
        }

        var alreadyExistsEmail = await _dbContext.Accounts.AnyAsync(record =>
            record.Email == idToken.Email);

        if (alreadyExistsEmail)
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.UserAlreadyExists, null);
        }

        var accountId = Guid.NewGuid();

        var account = new AccountEntity
        {
            Id = accountId,
            GoogleId = idToken.Subject!,
            Email = idToken.Email!,
            PinCodeHash = _hashService.ComputePinCodeHash(body.PinCode),
            Profile = new ProfileEntity
            {
                Id = accountId,
                Nickname = body.Nickname,
                Age = body.Age
            }
        };

        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        return new ValueTuple<StateCode, AccountEntity?>(StateCode.Ok, account);
    }

    public async Task<(StateCode, AccountEntity?)> SignIn(AccountSignInRequestModel body)
    {
        var (isIdTokenValid, idToken) = await _oauthService.VerifyIdToken(body.OAuthIdToken);

        if (!isIdTokenValid)
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.InvalidIdToken, null);
        }

        var account = await _dbContext.Accounts.SingleOrDefaultAsync(account => account.Email == idToken!.Email);

        if (account == null)
        {
            return new ValueTuple<StateCode, AccountEntity?>(StateCode.UserDoesNotExist, null);
        }

        account.LastSignedInAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await _dbContext.SaveChangesAsync();
        
        return account.PinCodeHash != _hashService.ComputePinCodeHash(body.PinCode)
            ? new ValueTuple<StateCode, AccountEntity?>(StateCode.InvalidCredentials, null)
            : new ValueTuple<StateCode, AccountEntity?>(StateCode.Ok, account);
    }

    public async Task SignOut(string userId, string tokenId)
    {
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(account => account.Id == Guid.Parse(userId));
        account!.DisabledSessionIds.Add(tokenId);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<StateModel<JsonObject>> Delete(
        AccountDeleteRequestModel body,
        AccountEntity account)
    {
        if (_hashService.ComputePinCodeHash(body.PinCode) != account.PinCodeHash)
        {
            return DefaultState.InvalidCredentials;
        }

        await _dbContext.Accounts
            .Where(dbAccount => dbAccount.Id == account.Id)
            .ExecuteDeleteAsync();

        return DefaultState.Ok;
    }

    public StateModel<AccountResponseModel> ParseAccountResponseState(StateCode resultState, AccountEntity? account)
    {
        if (resultState == StateCode.Ok)
        {
            var okState = StateModel<AccountResponseModel>.ParseOk(new AccountResponseModel
            {
                Account = new AccountModel
                {
                    Id = account!.Id,
                    IsBanned = account.IsBanned,
                    Role = account.Role,
                    CreatedAt = account.CreatedAt,
                    LastSignedInAt = account.LastSignedInAt,
                    Profile = new ProfileModel
                    {
                        Nickname = account.Profile.Nickname,
                        Age = account.Profile.Age,
                        IsHidden = account.Profile.IsHidden
                    }
                },
                AuthToken = _authService.GenerateJwtToken(account.Id.ToString())
            });
            
            return okState;
        }

        var errorState = new StateModel<AccountResponseModel>
        {
            Code = (int)resultState,
            State = resultState.ToString()
        };

        return errorState;
    }
}
