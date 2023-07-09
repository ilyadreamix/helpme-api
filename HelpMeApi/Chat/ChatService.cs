using System.Net;
using System.Text.Json.Nodes;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Chat.Entity.Json;
using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Common;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.Common.Utility;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;
using Microsoft.EntityFrameworkCore;

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

    public async Task<StateModel<List<ChatModel>>> List(
        Guid? topicId,
        Guid? creatorId,
        string? title,
        int offset,
        int size,
        OrderingMethod orderingMethod)
    {
        var query = _dbContext.Chats
            .Include(chat => chat.Topics)
            .AsQueryable();

        if (topicId != null)
        {
            query = query.Where(chat =>
                chat.Topics.Any(topic => topic.Id == topicId));
        }

        if (creatorId != null)
            query = query.Where(chat => chat.CreatorId == creatorId);
        
        if (title != null)
            query = query.Where(chat => chat.Title.Contains(title));

        query = orderingMethod switch
        {
            OrderingMethod.ByName => query.OrderBy(chat => chat.Title),
            _ => query.OrderByDescending(chat => chat.CreatedAt)
        };
        
        var chats = await query
            .Skip(offset.SafeOffset())
            .Take(size.SafeSize(50))
            .ToListAsync();

        var models = chats.ConvertAll(chat => (ChatModel)chat);
        
        return StateModel<List<ChatModel>>.ParseOk(models);
    }
    
    public async Task<StateModel<ChatModel>> Get(Guid id)
    {
        var chat = await _dbContext.Chats.FindAsync(id);
        if (chat != null)
        {
            return StateModel<ChatModel>.ParseOk((ChatModel)chat);
        }
        
        _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return StateModel<ChatModel>.ParseFrom(StateCode.ContentNotFound);
    }
    
    public async Task<StateModel<List<UserPublicModel>>> JoinedUsers(
        Guid id,
        int offset,
        int size)
    {
        var chat = await _dbContext.Chats
            .Include(
                chat => chat.JoinedUsers
                    .Skip(offset.SafeOffset())
                    .Take(size.SafeSize(25)))
            .FirstOrDefaultAsync(chat => chat.Id == id);
        
        if (chat == null)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return StateModel<List<UserPublicModel>>.ParseFrom(StateCode.ContentNotFound);
        }

        var users = chat.JoinedUsers.ConvertAll(user => (UserPublicModel)user);
        
        return StateModel<List<UserPublicModel>>.ParseOk(users);
    }

    public async Task<StateModel<JsonObject>> Join(Guid id)
    {
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.ContentNotFound;
        }

        if (chat.IsHidden)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.ThisChatIsDisabled;
        }
        
        if (chat.JoinedUsers.Contains(user))
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.YouHaveAlreadyJoinedThisChat;
        }

        if (chat.BannedUsers.Any(ban => ban.UserId == user.Id))
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return DefaultState.YouAreBannedInThisChat;
        }
        
        if (chat.InvitedUsers.Any(invitation => invitation.UserId == user.Id))
        {
            chat.InvitedUsers.RemoveAll(invitation => invitation.UserId == user.Id);
        }
        else if (!chat.IsPublic)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return DefaultState.YouWasNotInvitedToThisChat;
        }

        chat.JoinedUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        return DefaultState.Ok;
    }

    public async Task<StateModel<JsonObject>> InviteUser(Guid id, Guid userId)
    {
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.ContentNotFound;
        }
        
        if (user.Id != chat.CreatorId)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return DefaultState.YouCannotEditThisChat;
        }

        var invitedUser = await _dbContext.Users.FindAsync(userId);

        if (invitedUser == null)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.UserDoesNotExist;
        }

        if (chat.JoinedUsers.Any(dbUser => dbUser.Id == invitedUser.Id))
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.UserHasAlreadyJoinedThisChat;
        }

        if (chat.InvitedUsers.Any(invitation => invitation.UserId == invitedUser.Id))
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.UserWasAlreadyInvited;
        }

        if (chat.BannedUsers.Any(ban => ban.UserId == invitedUser.Id))
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.UnbanUserToInvite;
        }
        
        chat.InvitedUsers.Add(new ChatEntityInvitation
        {
            UserId = userId
        });
        await _dbContext.SaveChangesAsync();

        return DefaultState.Ok;
    }

    public async Task<StateModel<JsonObject>> UninviteUser(Guid id, Guid userId)
    {
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return DefaultState.ContentNotFound;
        }
        
        if (user.Id != chat.CreatorId)
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return DefaultState.YouCannotEditThisChat;
        }

        chat.InvitedUsers = chat.InvitedUsers
            .Where(invitation => invitation.UserId != userId)
            .ToList();

        await _dbContext.SaveChangesAsync();

        return DefaultState.Ok;
    }
}
