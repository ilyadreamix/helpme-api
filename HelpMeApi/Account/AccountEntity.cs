using System.ComponentModel.DataAnnotations;
using HelpMeApi.Account.Enum;
using HelpMeApi.Profile;

namespace HelpMeApi.Account;

public class AccountEntity
{
    [Key]
    public Guid Id { get; set; }

    public virtual ProfileEntity Profile { get; init; } = null!;

    public bool IsBanned { get; set; }
    public AccountRole Role { get; set; } = AccountRole.User;
    public List<string> DisabledSessionIds { get; set; } = new();
    public string GoogleId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PinCodeHash { get; set; } = null!;
    public long CreatedAt { get; set; }
    public long LastSignedInAt { get; set; }

    public AccountEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        LastSignedInAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
