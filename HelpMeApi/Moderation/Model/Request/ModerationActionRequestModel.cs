using HelpMeApi.Common.Enum;
using HelpMeApi.Common.Object;
using HelpMeApi.Moderation.Enum;

namespace HelpMeApi.Moderation.Model.Request;

public class ModerationActionRequestModel
{
    public ModerationAction Action { get; set; }
    public ObjectType ObjectType { get; set; }
    public string ObjectId { get; set; } = null!;
    public List<Extra> Extras { get; set; } = new();
}
