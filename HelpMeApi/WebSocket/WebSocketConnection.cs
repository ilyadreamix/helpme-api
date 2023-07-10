using SystemWebSocket = System.Net.WebSockets.WebSocket;

namespace HelpMeApi.WebSocket;

public class WebSocketConnection
{
    public SystemWebSocket WebSocket { get; set; } = null!;
    public long LastPing { get; set; }

    public WebSocketConnection()
    {
        LastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
