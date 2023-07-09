using HelpMeApi.Common.Enum;

namespace HelpMeApi.Moderation.Enum;

public sealed class ModerationActionInfo
{
    public static readonly ModerationActionInfo UserBan = new(
        ModerationAction.UserBan,
        ObjectType.User);
    public static readonly ModerationActionInfo UserHide = new(
        ModerationAction.UserHide,
        ObjectType.User);
    
    public static readonly ModerationActionInfo ChatVerify = new(
        ModerationAction.ChatVerify,
        ObjectType.Chat);
    public static readonly ModerationActionInfo ChatHide = new(
        ModerationAction.ChatHide,
        ObjectType.Chat);
    
    public static readonly ModerationActionInfo ChatMessageDelete = new(
        ModerationAction.ChatMessageDelete,
        ObjectType.ChatMessage);
    
    public static readonly ModerationActionInfo TopicDelete = new(
        ModerationAction.TopicDelete,
        ObjectType.Topic);

    public ModerationAction Action { get; }
    public ObjectType ObjectType { get; }

    private ModerationActionInfo(
        ModerationAction action,
        ObjectType objectType)
    {
        Action = action;
        ObjectType = objectType;
    }
}
