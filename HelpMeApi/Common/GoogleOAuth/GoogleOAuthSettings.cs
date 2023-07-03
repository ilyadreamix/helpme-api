using System.ComponentModel.DataAnnotations;

namespace HelpMeApi.Common.GoogleOAuth;

public class GoogleOAuthSettings
{
    [Required]
    public string? AndroidClientId { get; set; }
    
    [Required]
    public string? IosClientId { get; set; }
    
    [Required]
    public string? WebClientId { get; set; }
}
