namespace HelpMeApi.Chat.Model.Response;

public class ChatSendMessageResponseModel
{
    public ChatModel Chat { get; set; } = null!;
    public ChatMessageModel Message { get; set; } = null!;
}
