using System.Text.Json.Nodes;
using HelpMeApi.Common;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.GoogleOAuth;
using HelpMeApi.Common.Hash;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;
using HelpMeApi.User.Model.Request;
using HelpMeApi.User.Model.Response;
using Microsoft.EntityFrameworkCore;

namespace HelpMeApi.User;

public class UserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GoogleOAuthService _oauthService;
    private readonly HashService _hashService;
    private readonly AuthService _authService;

    public UserService(
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
    
    public async Task<(StateCode, UserEntity?)> SignUp(UserSignUpRequestModel body)
    {
        if (body.Age is < 14)
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.TooYoung, null);
        }
        
        var alreadyExistsNickname = await _dbContext.Users.AnyAsync(user =>
            user.Nickname == body.Nickname);

        if (alreadyExistsNickname)
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.NicknameUnavailable, null);
        }

        var (isIdTokenValid, idToken) = await _oauthService.VerifyIdToken(body.OAuthIdToken);

        if (!isIdTokenValid)
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.InvalidIdToken, null);
        }

        if (idToken!.EmailVerified != "true")
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.EmailIsNotVerified, null);
        }

        var alreadyExistsEmail = await _dbContext.Users.AnyAsync(record =>
            record.Email == idToken.Email);

        if (alreadyExistsEmail)
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.UserAlreadyExists, null);
        }

        var userId = Guid.NewGuid();

        var user = new UserEntity
        {
            Id = userId,
            GoogleId = idToken.Subject,
            Email = idToken.Email,
            PinCodeHash = _hashService.ComputePinCodeHash(body.PinCode),
            Nickname = body.Nickname,
            Age = body.Age
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return new ValueTuple<StateCode, UserEntity?>(StateCode.Ok, user);
    }

    public async Task<(StateCode, UserEntity?)> SignIn(UserSignInRequestModel body)
    {
        var (isIdTokenValid, idToken) = await _oauthService.VerifyIdToken(body.OAuthIdToken);

        if (!isIdTokenValid)
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.InvalidIdToken, null);
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == idToken!.Email);

        if (user == null)
        {
            return new ValueTuple<StateCode, UserEntity?>(StateCode.UserDoesNotExist, null);
        }

        user.LastSignedInAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await _dbContext.SaveChangesAsync();
        
        return user.PinCodeHash != _hashService.ComputePinCodeHash(body.PinCode)
            ? new ValueTuple<StateCode, UserEntity?>(StateCode.InvalidCredentials, null)
            : new ValueTuple<StateCode, UserEntity?>(StateCode.Ok, user);
    }

    public async Task SignOut(string userId, string tokenId)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == Guid.Parse(userId));
        user!.DisabledSessionIds.Add(tokenId);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<StateModel<JsonObject>> Delete(
        UserDeleteRequestModel body,
        UserEntity user)
    {
        if (_hashService.ComputePinCodeHash(body.PinCode) != user.PinCodeHash)
        {
            return DefaultState.InvalidCredentials;
        }

        await _dbContext.Users
            .Where(dbUser => dbUser.Id == user.Id)
            .ExecuteDeleteAsync();

        return DefaultState.Ok;
    }

    // I don't want to provide AuthService in UserController
    public StateModel<UserAuthResponseModel> ParseResponseState(StateCode resultState, UserEntity? user)
    {
        if (resultState == StateCode.Ok)
        {
            var okState = StateModel<UserAuthResponseModel>.ParseOk(new UserAuthResponseModel
            {
                User = (UserPrivateModel)user!,
                AuthToken = _authService.GenerateJwtToken(user!.Id.ToString())
            });
            
            return okState;
        }

        
        var errorState = new StateModel<UserAuthResponseModel>
        {
            Code = (int)resultState,
            State = resultState.ToString()
        };

        return errorState;
    }
    
    public async Task<bool> IsNicknameAvailable(string nickname) =>
        !await _dbContext.Users.AnyAsync(user => user.Nickname == nickname);
}
