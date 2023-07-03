namespace HelpMeApi.Account.Model.Response;

public class AccountResponseModel
{
    public AccountModel Account { get; set; } = null!;
    public string AuthToken { get; set; } = null!;
}
