namespace HelpMeApi.Chat.Entity.Object;

public class ChatEntityBan
{
    public Guid UserId { get; set; }
    public long BannedAt { get; set; }
    public bool BannedByAdmin { get; set; }

    public ChatEntityBan()
    {
        BannedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
