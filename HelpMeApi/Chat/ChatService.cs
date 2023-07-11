using System.Net;
using System.Text.Json.Nodes;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Chat.Entity.Object;
using HelpMeApi.Chat.Enum;
using HelpMeApi.Chat.Model;
using HelpMeApi.Chat.Model.Request;
using HelpMeApi.Chat.Model.Response;
using HelpMeApi.Common;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.Common.Utility;
using HelpMeApi.Topic.Entity;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Model;
using HelpMeApi.Ws;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace HelpMeApi.Chat;

public class ChatService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly WsService _wsService;
    
    private static readonly ChatMessageType[] AllowedMessageTypes = 
    {
        ChatMessageType.Image,
        ChatMessageType.Text,
        ChatMessageType.Video,
        ChatMessageType.Voice
    };

    private static readonly ChatMessageType[] ReplyableMessageTypes =
    {
        ChatMessageType.Image,
        ChatMessageType.Text,
        ChatMessageType.Video,
        ChatMessageType.Voice
    };

    public ChatService(
        ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor,
        WsService wsService)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _wsService = wsService;
    }

    public async Task<ChatModel> Create(ChatCreateRequestModel body)
    {
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var topicsTask = body.TopicIds.ConvertAll(async topic =>
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

    public async Task<IntermediateState<ChatModel>> Edit(
        Guid id,
        ChatUpdateRequestModel body)
    {
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        
        var iState = new IntermediateState<ChatModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var chat = await _dbContext.Chats
            .Include(chat => chat.Topics)
            .FirstOrDefaultAsync(chat => chat.Id == id);
        
        if (chat == null)
        {
            return iState.Copy(
                model: StateModel<ChatModel>.ParseFrom(StateCode.ContentNotFound));
        }

        if (chat.CreatorId != user.Id)
        {
            return iState.Copy(
                statusCode: HttpStatusCode.Forbidden,
                model: StateModel<ChatModel>.ParseFrom(StateCode.YouCannotEditThisChat));
        }

        if (body.Title != null)
            chat.Title = body.Title;

        chat.Description = body.Description;

        if (body.TopicIds is { Count: > 0 })
        {
            var topics = await _dbContext.Topics
                .Where(topic => body.TopicIds.Contains(topic.Id) && !topic.IsReadOnly)
                .ToListAsync();
            
            chat.Topics = topics;
        }
        else if (body.TopicIds?.Count is 0)
        {
            chat.Topics = new List<TopicEntity>();
        }

        chat.IsPublic = body.IsPublic;
        chat.IsVerified = body.IsPublic;

        await _dbContext.SaveChangesAsync();

        return iState.Ok(StateModel<ChatModel>.ParseOk((ChatModel)chat));
    }

    public async Task<StateModel<List<ChatModel>>> List(
        List<Guid>? topicIds,
        Guid? creatorId,
        string? title,
        int offset,
        int size,
        OrderingMethod orderingMethod)
    {
        var query = _dbContext.Chats
            .Include(chat => chat.Topics)
            .Include(chat => chat.Creator)
            .AsQueryable();

        if (topicIds != null)
        {
            query = topicIds.Aggregate(query, (current, topicId) =>
                current.Where(chat =>
                    chat.Topics.Any(topic => topic.Id == topicId)));
        }

        if (creatorId != null)
            query = query.Where(chat => chat.CreatorId == creatorId);
        
        if (title != null)
            query = query.Where(chat => chat.Title.Contains(title));

        query = query.Where(chat => !chat.IsHidden && chat.IsVerified);

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
        
        var chat = await _dbContext.Chats
            .Include(dbChat => dbChat.Creator)      
            .Include(dbChat => dbChat.Topics)
            .FirstOrDefaultAsync(dbChat => dbChat.Id == id);
        
        if (chat == null)
        {
            iState.Model = StateModel<ChatModel>.ParseFrom(StateCode.ContentNotFound);
            return iState;
        }

        if (chat is { IsHidden: false, IsVerified: true })
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

        if (chat.IsHidden || !chat.IsVerified)
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

        await _wsService.NotifyChatInvite(
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

    public async Task<IntermediateState<JsonObject>> Leave(Guid id)
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

        chat.JoinedUsers = chat.JoinedUsers
            .Where(joinedUser => joinedUser.Id != user.Id)
            .ToList();

        await _dbContext.SaveChangesAsync();

        iState.StatusCode = HttpStatusCode.OK;
        iState.Model = DefaultState.Ok;

        return iState;
    }

    public async Task<IntermediateState<ChatSendMessageResponseModel>> SendMessage(
        Guid id,
        ChatSendMessageRequestModel body)
    {
        var iState = new IntermediateState<ChatSendMessageResponseModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var user = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var chat = await _dbContext.Chats
            .Include(chat => chat.JoinedUsers)
            .Include(chat => chat.Creator)
            .Include(chat => chat.Topics)
            .FirstOrDefaultAsync(chat => chat.Id == id);

        if (chat == null)
        {
            return iState.Copy(
                model: StateModel<ChatSendMessageResponseModel>.ParseFrom(StateCode.ContentNotFound));
        }

        if (!chat.IsVerified || chat.IsHidden)
        {
            return iState.Copy(
                model: StateModel<ChatSendMessageResponseModel>.ParseFrom(StateCode.ThisChatIsDisabled));
        }

        if (!chat.JoinedUsers.ConvertAll(joinedUser => joinedUser.Id).Contains(user.Id))
        {
            return iState.Copy(
                model: StateModel<ChatSendMessageResponseModel>.ParseFrom(StateCode.YouHaveNotJoinedThisChat));
        }

        if (!AllowedMessageTypes.Contains(body.MessageType))
        {
            return iState.Copy(
                model: StateModel<ChatSendMessageResponseModel>.ParseFrom(StateCode.InvalidRequest));
        }
        
        var message = new ChatMessageEntity
        {
            Id = Guid.NewGuid(),
            ChatId = chat.Id,
            Author = user
        };

        if (body.Content != null)
        {
            if (body.MessageType != ChatMessageType.Text && !Uri.IsWellFormedUriString(body.Content, UriKind.Absolute))
            {
                return iState.Copy(
                    model: StateModel<ChatSendMessageResponseModel>.ParseFrom(StateCode.InvalidRequest));
            }

            message.Type = body.MessageType;
        }
        
        message.Content = body.Content;

        if (body.ReplyToId != null)
        {
            var replyMessage = await _dbContext.ChatMessages
                .Include(replyMessage => replyMessage.Chat)
                .Include(replyMessage => replyMessage.Author)
                .FirstOrDefaultAsync(replyMessage => replyMessage.Id == body.ReplyToId);

            if (replyMessage == null || replyMessage.Chat.Id != chat.Id)
            {
                return iState.Copy(
                    model: StateModel<ChatSendMessageResponseModel>.ParseFrom(StateCode.ContentNotFound));
            }

            message.ReplyToId = replyMessage.Id;
        }

        message.MentionedUserIds = body.MentionedUserIds;

        _dbContext.ChatMessages.Add(message);
        await _dbContext.SaveChangesAsync();

        await _wsService.NotifyChatMessage(message).ConfigureAwait(false);

        return iState.Ok(StateModel<ChatSendMessageResponseModel>.ParseOk(new ChatSendMessageResponseModel
        {
            Chat = (ChatModel)chat,
            Message = (ChatMessageModel)message
        }));
    }
}
