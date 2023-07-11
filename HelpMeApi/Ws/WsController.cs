using System.Net;
using System.Net.WebSockets;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.State;
using HelpMeApi.User.Entity;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.Ws;

[ApiController]
[Route("ws")]
[AuthRequired]
public class WsController : ControllerBase
{
    private readonly WsConnectionManager _connectionManager;
    
    // ReSharper disable once NotAccessedField.Local
    private Timer _pingTimer;
    private const int TimeoutMs = 240 * 1000;

    public WsController(WsConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
        _pingTimer = new Timer(
            callback: _CheckConnectionTimeout!,
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1));
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
        var connection = new WsConnection { WebSocket = websocket };
        
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

    private async Task Listen(Guid userId, WebSocket webSocket)
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Close:
                    await _RemoveConnection(userId);
                    break;
            }
        }
    }

    private async void _CheckConnectionTimeout(object state)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var connection in _connectionManager.Connections)
        {
            if (currentTimestamp < connection.Value.LastPing + TimeoutMs)
            {
                continue;
            }

            await _RemoveConnection(connection.Key);
        }
    }

    private async Task _RemoveConnection(Guid userId)
    {
        var connection = _connectionManager.Get(userId);

        if (connection == null ||
            connection.WebSocket.State == WebSocketState.Closed)
        {
            return;
        }

        await connection.Semaphore.WaitAsync();

        try
        {
            await connection.WebSocket.CloseAsync(
                closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Session timeout",
                CancellationToken.None);
        }
        finally
        {
            connection.Semaphore.Release();
            _connectionManager.Remove(userId);
        }
    }
}
