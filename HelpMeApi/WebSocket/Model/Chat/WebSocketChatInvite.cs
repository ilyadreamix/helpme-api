using HelpMeApi.Chat.Model;
using HelpMeApi.User.Model;

namespace HelpMeApi.WebSocket.Model.Chat;

public class WebSocketChatInvite
{
    public ChatModel Chat { get; set; } = null!;
    public UserPublicModel Sender { get; set; } = null!;
}
