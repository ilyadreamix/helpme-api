using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.State.Model;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.Chat;

[ApiController]
[Route("chat")]
[AuthRequired]
public class ChatController : ControllerBase
{
    private readonly ChatService _service;

    public ChatController(ChatService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ChatCreateRequestModel body)
    {
        var chat = await _service.CreateChat(body);
        var state = StateModel<ChatModel>.ParseOk(chat);
        return new JsonResult(state);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var state = await _service.Get(id);
        return new JsonResult(state);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        Guid? topicId,
        Guid? creatorId,
        string? title,
        int offset = 0,
        int size = 50,
        OrderingMethod orderingMethod = OrderingMethod.ByTime)
    {
        var state = await _service.List(
            topicId: topicId,
            creatorId: creatorId,
            title: title,
            offset: offset,
            size: size,
            orderingMethod: orderingMethod);
        return new JsonResult(state);
    }
    
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id)
    {
        var state = await _service.Join(id);
        return new JsonResult(state);
    }
    
    [HttpGet("{id:guid}/joined-users")]
    public async Task<IActionResult> JoinedUsers(
        Guid id,
        int offset = 0,
        int size = 25)
    {
        var state = await _service.JoinedUsers(id, offset, size);
        return new JsonResult(state);
    }
    
    [HttpPost("{id:guid}/invite-user")]
    public async Task<IActionResult> InviteUser(
        Guid id,
        Guid userId)
    {
        var state = await _service.InviteUser(id, userId);
        return new JsonResult(state);
    }
    
    [HttpPost("{id:guid}/uninvite-user")]
    public async Task<IActionResult> UninviteUser(
        Guid id,
        Guid userId)
    {
        var state = await _service.UninviteUser(id, userId);
        return new JsonResult(state);
    }
}
