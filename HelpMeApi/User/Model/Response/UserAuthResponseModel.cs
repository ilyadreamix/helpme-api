namespace HelpMeApi.User.Model.Response;

public class UserAuthResponseModel
{
    public UserPrivateModel User { get; set; } = null!;
    public string AuthToken { get; set; } = null!;
}
