using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using HelpMeApi.Common.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace HelpMeApi.Common.Auth
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthRequired : Attribute
    {
        // ...
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
            var decorator = endpoint?.Metadata.GetMetadata<AuthRequired>();

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

            var account = await dbContext.Accounts.SingleOrDefaultAsync(account =>
                account.Id == Guid.Parse(userId.Value));
            
            if (account is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
                return;
            }

            if (account.DisabledSessionIds.Contains(tokenId.Value))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(DefaultState.Unauthorized);
                return;
            }

            context.Items["Account"] = account;
            context.Items["TokenId"] = tokenId.Value;

            await _next.Invoke(context);
        }
    }
}
