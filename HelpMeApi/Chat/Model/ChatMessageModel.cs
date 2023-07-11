using System.Text.Json.Serialization;
using HelpMeApi.Chat.Enum;
using HelpMeApi.User.Model;

namespace HelpMeApi.Chat.Model;

public class ChatMessageModel
{
    public Guid Id { get; set; }
    public UserPublicModel Author { get; set; } = null!;
    public ChatMessageType Type { get; set; } = ChatMessageType.Text;
    public string? Content { get; set; }
    public long CreatedAt { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Guid>? MentionedUserIds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ReplyToId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChatMessageModel? ReplyTo { get; set; }

    public ChatMessageModel()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
