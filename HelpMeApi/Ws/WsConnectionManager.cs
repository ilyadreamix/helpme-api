using System.Collections.Concurrent;

namespace HelpMeApi.Ws;

public class WsConnectionManager
{
    public readonly ConcurrentDictionary<Guid, WsConnection> Connections = new();

    public void Add(Guid userId, WsConnection connection)
    {
        Connections.TryAdd(userId, connection);
    }
    
    public void Remove(Guid userId)
    {
        Connections.TryRemove(userId, out _);
    }

    public WsConnection? Get(Guid userId)
    {
        Connections.TryGetValue(userId, out var connection);
        return connection;
    }

    public bool Contains(Guid userId)
    {
        return Connections.ContainsKey(userId);
    }
    
    public void UpdateTimestamp(Guid userId)
    {
        var connection = Get(userId);
        if (connection == null)
        {
            return;
        }

        connection.LastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
