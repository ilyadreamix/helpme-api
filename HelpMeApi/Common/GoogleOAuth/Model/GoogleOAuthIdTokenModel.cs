using System.Text.Json.Serialization;

namespace HelpMeApi.Common.GoogleOAuth.Model;

public class GoogleOAuthIdTokenModel
{
    [JsonPropertyName("iss")]
    public string Issuer { get; set; } = null!;
    
    [JsonPropertyName("sub")]
    public string Subject { get; set; } = null!;
    
    [JsonPropertyName("aud")]
    public string Audience { get; set; } = null!;
    
    [JsonPropertyName("iat")]
    public string IssuedAtTime { get; set; } = null!;
    
    [JsonPropertyName("exp")]
    public string ExpirationTime { get; set; } = null!;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    
    [JsonPropertyName("email_verified")]
    public string EmailVerified { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("picture")]
    public string Picture { get; set; } = null!;
    
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; } = null!;
    
    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; } = null!;
}
