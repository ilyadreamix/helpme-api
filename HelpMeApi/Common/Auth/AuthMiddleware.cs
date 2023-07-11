using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using HelpMeApi.Common.State;
using HelpMeApi.User.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace HelpMeApi.Common.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthRequiredAttribute : Attribute
{
    public bool ForbidBanned { get; set; } = true;
    public UserRole[] Roles { get; set; } = { UserRole.Default, UserRole.Moderator, UserRole.Support };
}

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        AuthService authService,
        ApplicationDbContext dbContext)
    {
        var endpoint = context.GetEndpoint();
        var decorator = endpoint?.Metadata.GetMetadata<AuthRequiredAttribute>();

        if (decorator is null)
        {
            await _next.Invoke(context);
            return;
        }

        var headers = context.Request.Headers;
        var isAuthHeaderPresent = headers.TryGetValue(HeaderNames.Authorization, out var authHeader);

        if (!isAuthHeaderPresent)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
            return;
        }

        var token = authHeader!.ToString().Split(" ").ElementAtOrDefault(1);

        if (token is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
            return;
        }
            
        var (isTokenValid, claims) = authService.ValidateJwtToken(token);

        if (!isTokenValid)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
            return;
        }

        var userId = claims!.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier);
        var tokenId = claims.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Jti);

        if (userId is null || tokenId is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
            return;
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(user =>
            user.Id == Guid.Parse(userId.Value));
            
        if (user is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
            return;
        }

        if (user.DisabledSessionIds.Contains(tokenId.Value))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
            return;
        }

        if (user.IsBanned && decorator.ForbidBanned)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(DefaultState.YouAreBanned);
            return;
        }
        
        if (!decorator.Roles.Contains(user.Role))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(DefaultState.NoRights);
            return;
        }

        user.LastOnlineAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await dbContext.SaveChangesAsync();

        context.Items["User"] = user;
        context.Items["AuthTokenId"] = tokenId.Value;

        await _next.Invoke(context);
    }
}
