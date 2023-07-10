using System.Net;
using System.Net.WebSockets;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.State;
using HelpMeApi.User.Entity;
using Microsoft.AspNetCore.Mvc;
using SystemWebSocket = System.Net.WebSockets.WebSocket;

namespace HelpMeApi.WebSocket;

[ApiController]
[Route("ws")]
[AuthRequired]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketConnectionManager _connectionManager;
    
    private Timer _pingTimer;
    private const int TimeoutMs = 480 * 1000;

    public WebSocketController(WebSocketConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
        _pingTimer = new Timer(
            callback: CheckConnectionTimeout!,
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromMilliseconds(TimeoutMs));
    }

    [HttpGet("connect")]
    public async Task Connect()
    {
        var user = (UserEntity)HttpContext.Items["User"]!;
        
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        if (_connectionManager.Contains(user.Id))
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var connection = new WebSocketConnection { WebSocket = websocket };
        
        _connectionManager.Add(user.Id, connection);
        await Listen(user.Id, websocket);
    }

    [HttpPost("notify-online")]
    public IActionResult NotifyOnline()
    {
        var user = (UserEntity)HttpContext.Items["User"]!;
        
        if (!_connectionManager.Contains(user.Id))
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return new JsonResult(DefaultState.ConnectionDoesNotExist);
        }
        
        _connectionManager.UpdateTimestamp(user.Id);
        
        return new JsonResult(DefaultState.Ok);
    }

    private async Task Listen(Guid userId, SystemWebSocket webSocket)
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Close:
                    await RemoveConnection(userId);
                    break;
            }
        }
    }

    private async void CheckConnectionTimeout(object state)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var connection in _connectionManager.Connections)
        {
            if (currentTimestamp < connection.Value.LastPing + TimeoutMs)
            {
                continue;
            }

            await RemoveConnection(connection.Key);
        }
    }

    private async Task RemoveConnection(Guid userId)
    {
        var connection = _connectionManager.Get(userId);

        if (connection == null ||
            connection.WebSocket.State == WebSocketState.Closed)
        {
            return;
        }
        
        await connection.WebSocket.CloseAsync(
            closeStatus: WebSocketCloseStatus.NormalClosure,
            statusDescription: "Session timeout",
            CancellationToken.None);
            
        _connectionManager.Remove(userId);
    }
}
