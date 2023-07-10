namespace HelpMeApi.Chat.Entity.Object;

public class ChatEntityInvitation
{
    public Guid UserId { get; set; }
    public long InvitedAt { get; set; }

    public ChatEntityInvitation()
    {
        InvitedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
