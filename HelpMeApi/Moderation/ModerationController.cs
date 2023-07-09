using HelpMeApi.Common.Auth;
using HelpMeApi.Common.Enum;
using HelpMeApi.Moderation.Enum;
using HelpMeApi.Moderation.Model.Request;
using HelpMeApi.User.Enum;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.Moderation;

[Route("moderation")]
[AuthRequired(Roles = new[] { UserRole.Moderator, UserRole.Support })]
[ApiController]
public class ModerationController : ControllerBase
{
    private readonly ModerationService _moderationService;

    public ModerationController(ModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpPost("action")]
    public async Task<IActionResult> Action([FromBody] ModerationActionRequestModel body)
    {
        var state = await _moderationService.Action(body);
        return new JsonResult(state);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var state = await _moderationService.Get(id);
        return new JsonResult(state);
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> List(
        Guid? moderatorId,
        Guid? objectId,
        ModerationAction? action,
        OrderingMethod orderingMethod = OrderingMethod.ByTime)
    {
        var state = await _moderationService.List(moderatorId, objectId, action, orderingMethod);
        return new JsonResult(state);
    }
}
