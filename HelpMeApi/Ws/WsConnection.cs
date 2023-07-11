using System.Net.WebSockets;

namespace HelpMeApi.Ws;

public class WsConnection
{
    public WebSocket WebSocket { get; set; } = null!;
    public long LastPing { get; set; }
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    public WsConnection()
    {
        LastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
