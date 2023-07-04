using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace HelpMeApi.Common.Auth;

public class AuthService
{
    private readonly AuthSettings _authSettings;

    public AuthService(IOptions<AuthSettings> authSettings)
    {
        _authSettings = authSettings.Value;
    }

    public string GenerateJwtToken(string userId)
    {
        var key = Encoding.UTF8.GetBytes(_authSettings.JwtKey);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _authSettings.JwtIssuer,
            Audience = _authSettings.JwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
    }

    public (bool, ClaimsPrincipal?) ValidateJwtToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_authSettings.JwtKey);
        
        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = _authSettings.JwtIssuer,
            ValidAudience = _authSettings.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true
        };

        var validator = new JwtSecurityTokenHandler();

        if (!validator.CanReadToken(token))
        {
            return new ValueTuple<bool, ClaimsPrincipal?>(false, null);
        }

        try
        {
            var principal = validator.ValidateToken(token, validationParams, out _)!;
            return new ValueTuple<bool, ClaimsPrincipal?>(true, principal);
        }
        catch
        {
            return new ValueTuple<bool, ClaimsPrincipal?>(false, null);
        }
    }
}
