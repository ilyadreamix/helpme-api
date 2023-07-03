namespace HelpMeApi.Common.Auth;

public class AuthSettings
{
    public string JwtIssuer { get; set; } = null!;
    public string JwtAudience { get; set; } = null!;
    public string JwtKey { get; set; } = null!;
}
