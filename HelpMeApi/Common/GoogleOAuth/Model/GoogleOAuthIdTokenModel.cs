using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HelpMeApi.Common.GoogleOAuth.Model;

public class GoogleOAuthIdTokenModel
{
    [Required]
    [JsonPropertyName("iss")]
    public string? Issuer { get; set; }
    
    [Required]
    [JsonPropertyName("sub")]
    public string? Subject { get; set; }
    
    [Required]
    [JsonPropertyName("aud")]
    public string? Audience { get; set; }
    
    [Required]
    [JsonPropertyName("iat")]
    public string? IssuedAtTime { get; set; }
    
    [Required]
    [JsonPropertyName("exp")]
    public string? ExpirationTime { get; set; }
    
    [Required]
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [Required]
    [JsonPropertyName("email_verified")]
    public string? EmailVerified { get; set; }
    
    [Required]
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [Required]
    [JsonPropertyName("picture")]
    public string? Picture { get; set; }
    
    [Required]
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }
    
    [Required]
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }
}
