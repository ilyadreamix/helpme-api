using System.Net;
using System.Text.Json.Nodes;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Chat.Entity.Object;
using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Common;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.Common.Utility;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;
using HelpMeApi.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace HelpMeApi.Chat;

public class ChatService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly WebSocketService _webSocketService;

    public ChatService(
        ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor,
        WebSocketService webSocketService)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _webSocketService = webSocketService;
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

        query = query.Where(chat => !chat.IsHidden);

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
    
    public async Task<IntermediateState<ChatModel>> Get(Guid id)
    {
        var iState = new IntermediateState<ChatModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var chat = await _dbContext.Chats.FindAsync(id);
        if (chat == null)
        {
            iState.Model = StateModel<ChatModel>.ParseFrom(StateCode.ContentNotFound);
            return iState;
        }

        if (!chat.IsHidden)
        {
            iState.Model = StateModel<ChatModel>.ParseOk((ChatModel)chat);
            iState.StatusCode = HttpStatusCode.OK;
            return iState;
        }
        
        iState.Model = StateModel<ChatModel>.ParseFrom(StateCode.ThisChatIsDisabled);
        return iState;
    }
    
    public async Task<IntermediateState<List<UserPublicModel>>> JoinedUsers(
        Guid id,
        int offset,
        int size)
    {
        var iState = new IntermediateState<List<UserPublicModel>>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var chat = await _dbContext.Chats
            .Include(
                chat => chat.JoinedUsers
                    .Where(user => !user.IsBanned)
                    .Skip(offset.SafeOffset())
                    .Take(size.SafeSize(25)))
            .FirstOrDefaultAsync(chat => chat.Id == id);
        
        if (chat == null)
        {
            return iState.Copy(
                model: StateModel<List<UserPublicModel>>.ParseFrom(StateCode.ContentNotFound));
        }

        var users = chat.JoinedUsers.ConvertAll(user => (UserPublicModel)user);
        
        return iState.Ok(StateModel<List<UserPublicModel>>.ParseOk(users));
    }

    public async Task<IntermediateState<JsonObject>> Join(Guid id)
    {
        var iState = new IntermediateState<JsonObject>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            iState.Model = DefaultState.ContentNotFound;
            return iState;
        }

        if (chat.IsHidden)
        {
            return iState.Copy(model: DefaultState.ThisChatIsDisabled);
        }
        
        if (chat.JoinedUsers.Contains(user))
        {
            return iState.Copy(model: DefaultState.YouHaveAlreadyJoinedThisChat);
        }

        if (chat.BannedUsers.Any(ban => ban.UserId == user.Id))
        {
            return iState.Copy(
                statusCode: HttpStatusCode.Forbidden, 
                model: DefaultState.YouAreBanned);
        }
        
        if (chat.InvitedUsers.Any(invitation => invitation.UserId == user.Id))
        {
            chat.InvitedUsers.RemoveAll(invitation => invitation.UserId == user.Id);
        }
        else if (!chat.IsPublic)
        {
            return iState.Copy(
                statusCode: HttpStatusCode.Forbidden, 
                model: DefaultState.YouWasNotInvitedToThisChat);
        }

        chat.JoinedUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        return iState.Ok(DefaultState.Ok);
    }

    public async Task<IntermediateState<JsonObject>> InviteUser(Guid id, Guid userId)
    {
        var iState = new IntermediateState<JsonObject>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            return iState.Copy(model: DefaultState.ContentNotFound);
        }
        
        if (user.Id != chat.CreatorId)
        {
            return iState.Copy(
                model: DefaultState.YouCannotEditThisChat,
                statusCode: HttpStatusCode.Forbidden);
        }

        var invitedUser = await _dbContext.Users.FindAsync(userId);

        if (invitedUser == null)
        {
            return iState.Copy(model: DefaultState.UserDoesNotExist);
        }

        if (chat.JoinedUsers.Any(dbUser => dbUser.Id == invitedUser.Id))
        {
            return iState.Copy(model: DefaultState.UserHasAlreadyJoinedThisChat);
        }

        if (chat.InvitedUsers.Any(invitation => invitation.UserId == invitedUser.Id))
        {
            return iState.Copy(model: DefaultState.UserWasAlreadyInvited);
        }

        if (chat.BannedUsers.Any(ban => ban.UserId == invitedUser.Id))
        {
            return iState.Copy(model: DefaultState.UnbanUserToInvite);
        }
        
        chat.InvitedUsers.Add(new ChatEntityInvitation
        {
            UserId = userId
        });
        await _dbContext.SaveChangesAsync();

        await _webSocketService.NotifyUserChatInvite(
            sender: user,
            recipient: invitedUser,
            chat: chat).ConfigureAwait(false);

        return iState.Ok(DefaultState.Ok);
    }

    public async Task<IntermediateState<JsonObject>> UninviteUser(Guid id, Guid userId)
    {
        var iState = new IntermediateState<JsonObject>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            return iState.Copy(model: DefaultState.ContentNotFound);
        }
        
        if (user.Id != chat.CreatorId)
        {
            return iState.Copy(
                statusCode: HttpStatusCode.Forbidden,
                model: DefaultState.YouCannotEditThisChat);
        }

        chat.InvitedUsers = chat.InvitedUsers
            .Where(invitation => invitation.UserId != userId)
            .ToList();

        await _dbContext.SaveChangesAsync();

        return iState.Ok(DefaultState.Ok);
    }
}
