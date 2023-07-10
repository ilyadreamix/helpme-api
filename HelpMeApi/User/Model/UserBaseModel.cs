using HelpMeApi.User.Enum;

namespace HelpMeApi.User.Model;

public abstract class UserBaseModel
{
    public Guid Id { get; set; }
    public bool IsBanned { get; set; }
    public UserRole Role { get; set; } = UserRole.Default;
    public long CreatedAt { get; set; }
    public string Nickname { get; set; } = null!;
    public bool IsHidden { get; set; }
    public long LastOnlineAt { get; set; }
}
