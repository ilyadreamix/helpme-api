using System.ComponentModel.DataAnnotations;
using HelpMeApi.Chat.Enum;
using HelpMeApi.User.Entity;

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

    public ChatMessageEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
