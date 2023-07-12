using System.Net;
using System.Text.Json.Nodes;
using HelpMeApi.Common;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.GoogleOAuth;
using HelpMeApi.Common.Hash;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Enum;
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
    private readonly IHttpContextAccessor _contextAccessor;
    public UserService(
        ApplicationDbContext dbContext,
        GoogleOAuthService oauthService,
        HashService hashService,
        AuthService authService,
        IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _oauthService = oauthService;
        _hashService = hashService;
        _authService = authService;
        _contextAccessor = contextAccessor;
    }
    
    public async Task<IntermediateState<UserAuthResponseModel>> SignUp(UserSignUpRequestModel body)
    {
        var iState = new IntermediateState<UserAuthResponseModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        }; 
        
        if (body.Age is < 14)
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.TooYoung);
            return iState;
        }
        
        var alreadyExistsNickname = await _dbContext.Users.AnyAsync(user =>
            user.Nickname == body.Nickname);

        if (alreadyExistsNickname)
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.NicknameUnavailable);
            return iState;
        }

        var (isIdTokenValid, idToken) = await _oauthService.VerifyIdToken(body.OAuthIdToken);

        if (!isIdTokenValid)
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.InvalidIdToken);
            return iState;
        }

        if (idToken!.EmailVerified != "true")
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.EmailIsNotVerified);
            return iState;
        }

        var alreadyExistsEmail = await _dbContext.Users.AnyAsync(record =>
            record.Email == idToken.Email);

        if (alreadyExistsEmail)
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.UserAlreadyExists);
            return iState;
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

        iState.StatusCode = HttpStatusCode.OK;
        iState.Model = StateModel<UserAuthResponseModel>.ParseOk(new UserAuthResponseModel
        {
            User = (UserPrivateModel)user,
            AuthToken = _authService.GenerateJwtToken(user.Id.ToString())
        });

        return iState;
    }

    public async Task<IntermediateState<UserAuthResponseModel>> SignIn(UserSignInRequestModel body)
    {
        var iState = new IntermediateState<UserAuthResponseModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        }; 
        
        var (isIdTokenValid, idToken) = await _oauthService.VerifyIdToken(body.OAuthIdToken);

        if (!isIdTokenValid)
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.InvalidIdToken);
            return iState;
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == idToken!.Email);

        if (user == null)
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.UserDoesNotExist);
            return iState;
        }

        user.LastSignedInAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await _dbContext.SaveChangesAsync();

        if (user.PinCodeHash != _hashService.ComputePinCodeHash(body.PinCode))
        {
            iState.Model = StateModel<UserAuthResponseModel>.ParseFrom(StateCode.InvalidCredentials);
            return iState;
        }

        iState.StatusCode = HttpStatusCode.OK;
        iState.Model = StateModel<UserAuthResponseModel>.ParseOk(new UserAuthResponseModel
        {
            User = (UserPrivateModel)user,
            AuthToken = _authService.GenerateJwtToken(user.Id.ToString())
        });
        
        return iState;
    }

    public async Task SignOut()
    {
        var authUser = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var authTokenId = (string)_contextAccessor.HttpContext!.Items["AuthTokenId"]!;
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == authUser.Id);
        user!.DisabledSessionIds.Add(authTokenId);
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IntermediateState<JsonObject>> Delete(UserDeleteRequestModel body)
    {
        var authUser = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        
        var iState = new IntermediateState<JsonObject>
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        if (authUser.Role > UserRole.Default)
        {
            iState.Model = DefaultState.NoRights;
            return iState;
        }
        
        if (_hashService.ComputePinCodeHash(body.PinCode) != authUser.PinCodeHash)
        {
            iState.Model = DefaultState.InvalidCredentials;
            return iState;
        }

        await _dbContext.Users
            .Where(dbUser => dbUser.Id == authUser.Id)
            .ExecuteDeleteAsync();

        iState.StatusCode = HttpStatusCode.OK;
        iState.Model = DefaultState.Ok;

        return iState;
    }

    public async Task<IntermediateState<UserPublicModel>> Get(Guid id)
    {
        var iState = new IntermediateState<UserPublicModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return iState.Copy(
                model: StateModel<UserPublicModel>.ParseFrom(StateCode.ContentNotFound));
        }

        return iState.Ok(StateModel<UserPublicModel>.ParseOk((UserPublicModel)user));
    }
    
    public async Task<bool> IsNicknameAvailable(string nickname) =>
        !await _dbContext.Users.AnyAsync(user => user.Nickname == nickname);
}
