using HelpMeApi.Account.Enum;
using HelpMeApi.Profile.Model;

namespace HelpMeApi.Account.Model;

public class AccountModel
{
    public Guid Id { get; set; }
    public bool IsBanned { get; set; }
    public AccountRole Role { get; set; } = AccountRole.User;
    public long CreatedAt { get; set; }
    public long LastSignedInAt { get; set; }
    public ProfileModel Profile { get; set; } = null!;
}
