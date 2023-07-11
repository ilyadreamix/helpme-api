using HelpMeApi.Ws.Enum;

namespace HelpMeApi.Ws.Model;

public class WsMessage <T>
{
    public int Type { get; set; }
    public string TypeName { get; set; } = null!;
    public T? Data { get; set; }
    public long Timestamp { get; set; }

    public WsMessage()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static WsMessage<T> With(
        WsMessageType type,
        T data) => new()
    {
        Type = (int)type,
        TypeName = type.ToString(),
        Data = data
    };
}
