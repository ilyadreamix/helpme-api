using HelpMeApi.Chat.Entity;
using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Common;
using HelpMeApi.User.Entity;

namespace HelpMeApi.Chat;

public class ChatService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public ChatService(
        ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
    }

    public async Task<ChatModel> CreateChat(ChatCreateRequestModel body)
    {
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var topicsTask = body.Topics.ConvertAll(async topic =>
            await _dbContext.Topics.FindAsync(topic));
        var topics = (await Task.WhenAll(topicsTask))
            .Where(topic => topic != null)
            .ToList();

        var chat = new ChatEntity
        {
            Id = Guid.NewGuid(),
            CreatorId = user.Id,
            JoinedUsers = new List<UserEntity> { user },
            Topics = topics!,
            Title = body.Title,
            Description = body.Description,
            IsPublic = body.IsPublic,
            IsVerified = !body.IsPublic
        };

        await _dbContext.Chats.AddAsync(chat);
        await _dbContext.SaveChangesAsync();

        return (ChatModel)chat;
    }
}
