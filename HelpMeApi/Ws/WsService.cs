using System.Net.WebSockets;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Chat.Model;
using HelpMeApi.Common.Utility;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;
using HelpMeApi.Ws.Enum;
using HelpMeApi.Ws.Model;
using HelpMeApi.Ws.Model.Chat;

namespace HelpMeApi.Ws;

public class WsService
{
    private readonly WsConnectionManager _connectionManager;

    public WsService(WsConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task NotifyChatInvite(
        UserEntity sender,
        UserEntity recipient,
        ChatEntity chat)
    {
        var data = WsMessage<WsChatInvite>.With(
            type: WsMessageType.ChatInvite,
            data: new WsChatInvite
            {
                Sender = (UserPublicModel)sender,
                Chat = (ChatModel)chat
            });
        var byteData = data.SerializeObjectToByteArray();

        await SendAsync(recipient.Id, byteData);
    }

    public async Task NotifyChatMessage(ChatMessageEntity message)
    {
        var chat = message.Chat;
        var mentionedUserIds = message.MentionedUserIds;

        var wsMessage = new WsMessage<WsChatMessage>();

        foreach (var member in chat.JoinedUsers)
        {
            if (mentionedUserIds != null && mentionedUserIds.Contains(member.Id))
            {
                wsMessage.Type = (int)WsMessageType.ChatMentionedMessage;
                wsMessage.TypeName = WsMessageType.ChatMentionedMessage.ToString();
            }
            else
            {
                wsMessage.Type = (int)WsMessageType.ChatMessage;
                wsMessage.TypeName = WsMessageType.ChatMessage.ToString();
            }

            wsMessage.Data = new WsChatMessage
            {
                Chat = (ChatModel)chat,
                Message = (ChatMessageModel)message
            };

            var data = wsMessage.SerializeObjectToByteArray();

            await SendAsync(member.Id, data).ConfigureAwait(false);
        }
    }
    
    private async Task SendAsync(
        Guid userId,
        byte[] data)
    {
        var recipient = _connectionManager.Get(userId);

        if (recipient == null)
        {
            return;
        }

        await recipient.Semaphore.WaitAsync();

        try
        {
            if (recipient.WebSocket.State == WebSocketState.Open)
            {
                await recipient.WebSocket.SendAsync(
                    buffer: data,
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
            }
        }
        finally
        {
            recipient.Semaphore.Release();
        }
    }
}
