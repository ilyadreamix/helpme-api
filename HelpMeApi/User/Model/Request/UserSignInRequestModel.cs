using System.ComponentModel.DataAnnotations;

namespace HelpMeApi.User.Model.Request;

public class UserSignInRequestModel
{
    public string OAuthIdToken { get; set; } = null!;

    [RegularExpression("^[0-9]*$")]
    [MinLength(4)]
    [MaxLength(4)]
    public string PinCode { get; set; } = null!;
}
