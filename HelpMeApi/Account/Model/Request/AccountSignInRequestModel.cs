using System.ComponentModel.DataAnnotations;

namespace HelpMeApi.Account.Model.Request;

public class AccountSignInRequestModel
{
    public string OAuthIdToken { get; set; } = null!;

    [RegularExpression("^[0-9]*$")]
    [MinLength(4)]
    [MaxLength(4)]
    public string PinCode { get; set; } = null!;
}
