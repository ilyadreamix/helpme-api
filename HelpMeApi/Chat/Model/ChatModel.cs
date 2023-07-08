using HelpMeApi.Topic.Model;
using HelpMeApi.User.Model;

namespace HelpMeApi.Chat.Model;

public class ChatModel
{
    public Guid Id { get; set; }
    
    public UserPublicModel Creator { get; set; } = null!;
    public List<UserPublicModel> JoinedUsers { get; set; } = new();
    public List<ChatMessageModel> Messages { get; set; } = new();
    public List<TopicModel> Topics { get; set; } = new();
    
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsHidden { get; set; }
    public bool IsPublic { get; set; }
    public bool IsVerified { get; set; }
    public long CreatedAt { get; set; }

    public ChatModel()
    {
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
