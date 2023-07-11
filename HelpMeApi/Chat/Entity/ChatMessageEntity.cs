using System.ComponentModel.DataAnnotations;
using HelpMeApi.Chat.Enum;
using HelpMeApi.Chat.Model;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;

namespace HelpMeApi.Chat.Entity;

public class ChatMessageEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid ChatId { get; set; }
    public ChatEntity Chat { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public UserEntity Author { get; set; } = null!;

    public ChatMessageType Type { get; set; } = ChatMessageType.Text;
    public string? Content { get; set; }
    public long CreatedAt { get; set; }

    public List<Guid>? MentionedUserIds { get; set; } = null;
    public Guid? ReplyToId { get; set; }
    public ChatMessageEntity? ReplyTo { get; set; } = null;

    public ChatMessageEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    public static explicit operator ChatMessageModel(ChatMessageEntity entity) => new()
    {
        Id = entity.Id,
        Author = (UserPublicModel)entity.Author,
        Type = entity.Type,
        Content = entity.Content,
        CreatedAt = entity.CreatedAt,
        ReplyToId = entity.ReplyToId,
        ReplyTo = entity.ReplyTo == null ? null : (ChatMessageModel)entity.ReplyTo,
        MentionedUserIds = entity.MentionedUserIds
    };
}
