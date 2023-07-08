using HelpMeApi.Chat.Enum;
using HelpMeApi.User.Model;

namespace HelpMeApi.Chat.Model;

public class ChatMessageModel
{
    public Guid Id { get; set; }
    public UserPublicModel Creator { get; set; } = null!;
    public ChatMessageType Type { get; set; } = ChatMessageType.Text;
    public string? Content { get; set; }
    public long CreatedAt { get; set; }

    public ChatMessageModel()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
