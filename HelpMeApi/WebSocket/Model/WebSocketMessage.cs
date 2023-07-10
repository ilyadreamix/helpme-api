using HelpMeApi.WebSocket.Enum;

namespace HelpMeApi.WebSocket.Model;

public class WebSocketMessage <T>
{
    public int Type { get; set; }
    public string TypeName { get; set; }
    public T? Data { get; set; }
    public long Timestamp { get; set; }

    public WebSocketMessage()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static WebSocketMessage<T> With(
        WebSocketMessageType type,
        T data) => new()
    {
        Type = (int)type,
        TypeName = type.ToString(),
        Data = data
    };
}
