using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.Binder;
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
        var chat = await _service.Create(body);
        var state = StateModel<ChatModel>.ParseOk(chat);
        return new JsonResult(state);
    }
    
    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(
        Guid id,
        [FromBody] ChatUpdateRequestModel body)
    {
        var iState = await _service.Edit(id, body);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var iState = await _service.Get(id);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        [QueryList<Guid>] List<Guid>? topicIds,
        Guid? creatorId,
        string? title,
        int offset = 0,
        int size = 50,
        OrderingMethod orderingMethod = OrderingMethod.ByTime)
    {
        var state = await _service.List(
            topicIds: topicIds,
            creatorId: creatorId,
            title: title,
            offset: offset,
            size: size,
            orderingMethod: orderingMethod);
        return new JsonResult(state);
    }
    
    [HttpGet("list/joined-me")]
    public async Task<IActionResult> MyChats(
        [QueryList<Guid>] List<Guid>? topicIds,
        int offset = 0,
        int size = 50,
        OrderingMethod orderingMethod = OrderingMethod.ByTime)
    {
        var state = await _service.MyChats(
            topicIds: topicIds,
            offset: offset,
            size: size,
            orderingMethod: orderingMethod);
        return new JsonResult(state);
    }
    
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id)
    {
        var iState = await _service.Join(id);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id)
    {
        var iState = await _service.Leave(id);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
    
    [HttpGet("{id:guid}/joined-users")]
    public async Task<IActionResult> JoinedUsers(
        Guid id,
        int offset = 0,
        int size = 25)
    {
        var iState = await _service.JoinedUsers(id, offset, size);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
    
    [HttpPost("{id:guid}/invite-user")]
    public async Task<IActionResult> InviteUser(
        Guid id,
        Guid userId)
    {
        var iState = await _service.InviteUser(id, userId);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
    
    [HttpPost("{id:guid}/uninvite-user")]
    public async Task<IActionResult> UninviteUser(
        Guid id,
        Guid userId)
    {
        var iState = await _service.UninviteUser(id, userId);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
    
    [HttpPost("{id:guid}/message")]
    public async Task<IActionResult> SendMessage(
        Guid id,
        [FromBody] ChatSendMessageRequestModel body)
    {
        var iState = await _service.SendMessage(id, body);
        HttpContext.Response.StatusCode = (int)iState.StatusCode;
        return new JsonResult(iState.Model);
    }
}
