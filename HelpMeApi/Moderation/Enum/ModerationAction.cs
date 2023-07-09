namespace HelpMeApi.Moderation.Enum;

public enum ModerationAction
{
    UserBan,
    UserHide,
    ChatVerify,
    ChatHide,
    ChatMessageDelete,
    TopicDelete
}

public static class ModerationActionExtension
{
    public static ModerationActionInfo ToModerationActionInfo(this ModerationAction action) =>
        action switch
        {
            ModerationAction.UserBan => ModerationActionInfo.UserBan,
            ModerationAction.UserHide => ModerationActionInfo.UserHide,
            ModerationAction.ChatVerify => ModerationActionInfo.ChatVerify,
            ModerationAction.ChatHide => ModerationActionInfo.ChatHide,
            ModerationAction.ChatMessageDelete => ModerationActionInfo.ChatMessageDelete,
            ModerationAction.TopicDelete => ModerationActionInfo.TopicDelete,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
}
