using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HelpMeApi.Chat.Entity.Json;
using HelpMeApi.Chat.Model;
using HelpMeApi.Topic.Entity;
using HelpMeApi.Topic.Model;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;

namespace HelpMeApi.Chat.Entity;

public class ChatEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid CreatorId { get; set; }
    public UserEntity Creator { get; set; } = null!;
    
    public List<UserEntity> JoinedUsers { get; set; } = new();
    public List<ChatMessageEntity> Messages { get; set; } = new();
    public List<TopicEntity> Topics { get; set; } = new();

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public long CreatedAt { get; set; }
    public bool IsHidden { get; set; }
    public bool IsPublic { get; set; }
    public bool IsVerified { get; set; }
    
    [Column(TypeName = "jsonb[]")]
    public List<ChatEntityBan> BannedUsers { get; set; } = null!;
    
    [Column(TypeName = "jsonb[]")]
    public List<ChatEntityInvitation> InvitedUsers { get; set; } = null!;

    public ChatEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    public static explicit operator ChatModel(ChatEntity entity) => new()
    {
        Id = entity.Id,
        Creator = (UserPublicModel)entity.Creator,
        Topics = entity.Topics.ConvertAll(topic => (TopicModel)topic),
        Title = entity.Title,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        IsHidden = entity.IsHidden,
        IsPublic = entity.IsPublic,
        IsVerified = entity.IsVerified
    };
}
