using System.ComponentModel.DataAnnotations;

namespace HelpMeApi.User.Model.Request;

public class UserSignUpRequestModel
{
    public string OAuthIdToken { get; set; } = null!;

    [RegularExpression("^[0-9]*$")]
    [MinLength(4)]
    [MaxLength(4)]
    public string PinCode { get; set; } = null!;

    [RegularExpression("^(?!_+$)[a-zA-Z0-9_]+$")]
    [MinLength(4)]
    [MaxLength(36)]
    public string Nickname { get; set; } = null!;
    
    [Range(1, 100)]
    public int? Age { get; set; }
}
