using System.ComponentModel.DataAnnotations;
using HelpMeApi.Common.Enum;
using HelpMeApi.Moderation.Enum;
using HelpMeApi.Moderation.Model;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;

namespace HelpMeApi.Moderation;

public class ModerationEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid ModeratorId { get; set; }
    public UserEntity Moderator { get; set; } = null!;
    
    public Guid ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public ModerationAction Action { get; set; }
    
    public long CreatedAt { get; set; }

    public ModerationEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static explicit operator ModerationModel(ModerationEntity entity) => new()
    {
        Action = entity.Action,
        ActionId = entity.Id,
        ObjectId = entity.ObjectId,
        CreatedAt = entity.CreatedAt,
        Moderator = (UserPublicModel)entity.Moderator,
        ObjectType = entity.ObjectType
    };
}
