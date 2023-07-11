using HelpMeApi.Chat.Model;
using HelpMeApi.User.Model;

namespace HelpMeApi.Ws.Model.Chat;

public class WsChatInvite
{
    public ChatModel Chat { get; set; } = null!;
    public UserPublicModel Sender { get; set; } = null!;
}
