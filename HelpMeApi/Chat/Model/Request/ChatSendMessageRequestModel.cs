using HelpMeApi.Chat.Enum;

namespace HelpMeApi.Chat.Model.Request;

public class ChatSendMessageRequestModel
{
    public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;
    public string? Content { get; set; }
    public Guid? ReplyToId { get; set; }
    public List<Guid>? MentionedUserIds { get; set; }
}
