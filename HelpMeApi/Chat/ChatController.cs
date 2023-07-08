using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.State.Model;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.Chat;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _service;

    public ChatController(ChatService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    [AuthRequired]
    public async Task<IActionResult> Create([FromBody] ChatCreateRequestModel body)
    {
        var chat = await _service.CreateChat(body);
        var state = StateModel<ChatModel>.ParseOk(chat);
        return new JsonResult(state);
    }
}
