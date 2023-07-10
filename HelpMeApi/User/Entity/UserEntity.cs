using System.ComponentModel.DataAnnotations;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Topic.Entity;
using HelpMeApi.User.Enum;
using HelpMeApi.User.Model;

namespace HelpMeApi.User.Entity;

public class UserEntity
{
    [Key]
    public Guid Id { get; set; }

    public List<ChatEntity> Chats { get; set; } = new();
    public List<TopicEntity> Topics { get; set; } = new();

    public bool IsBanned { get; set; }
    public UserRole Role { get; set; } = UserRole.Default;
    public List<string> DisabledSessionIds { get; set; } = new();
    public string GoogleId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PinCodeHash { get; set; } = null!;

    public string Nickname { get; set; } = null!;
    public int? Age { get; set; }
    public bool IsHidden { get; set; }
    
    public long CreatedAt { get; set; }
    public long LastSignedInAt { get; set; }
    public long LastOnlineAt { get; set; }

    public UserEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        LastSignedInAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        LastOnlineAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static explicit operator UserPrivateModel(UserEntity entity) => new()
    {
        Id = entity.Id,
        IsBanned = entity.IsBanned,
        Role = entity.Role,
        CreatedAt = entity.CreatedAt,
        LastSignedInAt = entity.LastSignedInAt,
        Nickname = entity.Nickname,
        Age = entity.Age,
        IsHidden = entity.IsHidden,
        LastOnlineAt = entity.LastOnlineAt
    };
    
    public static explicit operator UserPublicModel(UserEntity entity) => new()
    {
        Id = entity.Id,
        IsBanned = entity.IsBanned,
        Role = entity.Role,
        CreatedAt = entity.CreatedAt,
        Nickname = entity.Nickname,
        IsHidden = entity.IsHidden,
        LastOnlineAt = entity.LastOnlineAt
    };
}
