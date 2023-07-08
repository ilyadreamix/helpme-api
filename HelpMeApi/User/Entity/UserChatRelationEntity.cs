using HelpMeApi.Chat.Entity;

namespace HelpMeApi.User.Entity;

public class UserChatRelationEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid ChatId { get; set; }
    public ChatEntity Chat { get; set; } = null!;
}