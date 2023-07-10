using System.Net.WebSockets;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Chat.Model;
using HelpMeApi.Common.Utility;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;
using HelpMeApi.WebSocket.Model;
using HelpMeApi.WebSocket.Model.Chat;
using SystemWebSocket = System.Net.WebSockets.WebSocket;

namespace HelpMeApi.WebSocket;

public class WebSocketService
{
    private readonly WebSocketConnectionManager _connectionManager;

    public WebSocketService(WebSocketConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task NotifyUserChatInvite(
        UserEntity sender,
        UserEntity recipient,
        ChatEntity chat)
    {
        var data = WebSocketMessage<WebSocketChatInvite>.With(
            type: Enum.WebSocketMessageType.UserChatInvite,
            data: new WebSocketChatInvite
            {
                Sender = (UserPublicModel)sender,
                Chat = (ChatModel)chat
            });
        var byteData = data.SerializeObjectToByteArray();

        await SendAsync(recipient.Id, byteData);
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
        
        if (recipient.WebSocket.State == WebSocketState.Open)
        {
            await recipient.WebSocket.SendAsync(
                buffer: new ArraySegment<byte>(data),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);
            _connectionManager.UpdateTimestamp(userId);
        }
    }
}
