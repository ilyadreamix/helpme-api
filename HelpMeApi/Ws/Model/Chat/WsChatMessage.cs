using HelpMeApi.Chat.Model;

namespace HelpMeApi.Ws.Model.Chat;

public class WsChatMessage
{
    public ChatModel Chat { get; set; } = null!;
    public ChatMessageModel Message { get; set; } = null!;
}
