using System.Net;
using HelpMeApi.Chat.Entity.Json;
using HelpMeApi.Common;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
using HelpMeApi.Common.Utility;
using HelpMeApi.Moderation.Enum;
using HelpMeApi.Moderation.Model;
using HelpMeApi.Moderation.Model.Request;
using HelpMeApi.User.Entity;
using HelpMeApi.User.Enum;
using Microsoft.EntityFrameworkCore;

namespace HelpMeApi.Moderation;

public class ModerationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public ModerationService(
        ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
    }

    public async Task<StateModel<ModerationModel>> Action(ModerationActionRequestModel body) =>
        await _PerformAction(
            objectId: Guid.Parse(body.ObjectId),
            objectType: body.ObjectType,
            action: body.Action,
            extras: body.Extras);

    public async Task<StateModel<ModerationModel>> Get(Guid id)
    {
        var moderation = await _dbContext.Moderations.FindAsync(id);
        if (moderation != null)
        {
            return StateModel<ModerationModel>.ParseOk((ModerationModel)moderation);
        }
        
        _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
    }

    public async Task<StateModel<List<ModerationModel>>> List(
        int offset,
        int size,
        Guid? moderatorId,
        Guid objectId,
        ModerationAction? action,
        OrderingMethod? orderingMethod)
    {
        var query = _dbContext.Moderations
            .AsQueryable()
            .Where(moderation => moderation.ObjectId == objectId);

        if (moderatorId != null)
            query = query.Where(moderation => moderation.ModeratorId == moderatorId);

        if (action != null)
            query = query.Where(moderation => moderation.Action == action);

        query = orderingMethod == OrderingMethod.ByName
            ? query.OrderBy(moderation => moderation.Action)
            : query.OrderByDescending(moderation => moderation.CreatedAt);

        var moderations = await query
            .Skip(offset.SafeOffset())
            .Take(size.SafeSize(50))
            .ToListAsync();
        
        var models = moderations.ConvertAll(moderation => (ModerationModel)moderation);

        return StateModel<List<ModerationModel>>.ParseOk(models);
    }
    
    private async Task<StateModel<ModerationModel>> _PerformAction(
        Guid objectId,
        ObjectType objectType,
        List<string> extras,
        ModerationAction action)
    {
        var moderator = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        
        switch (objectType)
        {
            case ObjectType.User:
                var user = await _dbContext.Users.FindAsync(objectId);
                if (user == null)
                {
                    _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                }
                
                switch (action)
                {
                    case ModerationAction.Ban:
                        if (moderator.Id == user.Id) // This can only be attempted by third party clients
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.YouCantLimitYourself);
                        }
                        
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                        }
                        
                        user.IsBanned = true;
                        break;
                    
                    case ModerationAction.Unban:
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                        }
                        
                        user.IsBanned = false;
                        break;
                    
                    case ModerationAction.Hide:
                        if (moderator.Id == user.Id) // This can only be attempted by third party clients
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.YouCantLimitYourself);
                        }
                        
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                        }
                        
                        user.IsHidden = true;
                        break;
                    
                    case ModerationAction.Show:
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                        }
                        
                        user.IsHidden = false;
                        break;
                    
                    default:
                        _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                }
                
                var userModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = action,
                    ObjectType = objectType
                };

                await _dbContext.Moderations.AddAsync(userModeration);
                await _dbContext.SaveChangesAsync();

                return StateModel<ModerationModel>.ParseOk((ModerationModel)userModeration);
            
            case ObjectType.Chat:
                var chat = await _dbContext.Chats
                    .Include(dbChat => dbChat.JoinedUsers)
                    .FirstOrDefaultAsync(dbChat => dbChat.Id == objectId);
                
                if (chat == null)
                {
                    _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                }

                switch (action)
                {
                    case ModerationAction.Verify:
                        chat.IsVerified = true;
                        break;
                    
                    case ModerationAction.Hide:
                        chat.IsHidden = true;
                        break;
                    
                    case ModerationAction.Show:
                        chat.IsHidden = false;
                        break;
                    
                    case ModerationAction.Ban:
                        var userToBanId = extras.ElementAtOrDefault(0) ?? "none";

                        if (!Guid.TryParse(userToBanId, out var userToBanIdParsed))
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
                        }

                        var userExists = chat.JoinedUsers.Any(dbUser => dbUser.Id == userToBanIdParsed);

                        if (!userExists)
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.UserDoesNotExist);
                        }
                        
                        chat.BannedUsers.Add(new ChatEntityBan
                        {
                            UserId = userToBanIdParsed,
                            BannedByAdmin = true
                        });
                        chat.JoinedUsers = chat.JoinedUsers
                            .Where(dbUser => dbUser.Id != userToBanIdParsed)
                            .ToList();
                        
                        break;
                    
                    case ModerationAction.Unban:
                        var userToUnbanId = extras.ElementAtOrDefault(0) ?? "none";
                        if (!Guid.TryParse(userToUnbanId, out var userToUnbanIdParsed))
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
                        }

                        chat.BannedUsers = chat.BannedUsers
                            .Where(ban => ban.UserId != userToUnbanIdParsed)
                            .ToList();
                        break;
                    
                    default:
                        _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                }

                var chatModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = action,
                    ObjectType = objectType,
                    Extras = extras
                };

                await _dbContext.Moderations.AddAsync(chatModeration);
                await _dbContext.SaveChangesAsync();

                return StateModel<ModerationModel>.ParseOk((ModerationModel)chatModeration);
            
            case ObjectType.ChatMessage:
                var message = await _dbContext.ChatMessages.FindAsync(objectId);
                if (message == null)
                {
                    _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                }
                
                switch (action)
                {
                    case ModerationAction.Delete:
                        await _dbContext.ChatMessages
                            .Where(dbMessage => dbMessage.Id == message.Id)
                            .ExecuteDeleteAsync();
                        break;
                    
                    default:
                        _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                }

                var messageModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = action,
                    ObjectType = objectType
                };

                await _dbContext.Moderations.AddAsync(messageModeration);
                await _dbContext.SaveChangesAsync();

                return StateModel<ModerationModel>.ParseOk((ModerationModel)messageModeration);
            
            case ObjectType.Topic:
                var topic = await _dbContext.Topics.FindAsync(objectId);
                if (topic == null)
                {
                    _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                }
                
                switch (action)
                {
                    case ModerationAction.Delete:
                        await _dbContext.ChatMessages
                            .Where(dbTopic => dbTopic.Id == topic.Id)
                            .ExecuteDeleteAsync();
                        break;
                    
                    default:
                        _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                }

                var topicModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = action,
                    ObjectType = objectType
                };

                await _dbContext.Moderations.AddAsync(topicModeration);
                await _dbContext.SaveChangesAsync();

                return StateModel<ModerationModel>.ParseOk((ModerationModel)topicModeration);
            
            default:
                _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
        }
    }
}
