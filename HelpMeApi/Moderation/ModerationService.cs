using System.Net;
using HelpMeApi.Chat.Entity.Object;
using HelpMeApi.Common;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.Object;
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

    public async Task<IntermediateState<ModerationModel>> Action(ModerationActionRequestModel body) =>
        await _PerformAction(
            objectId: Guid.Parse(body.ObjectId),
            objectType: body.ObjectType,
            action: body.Action,
            extras: body.Extras);

    public async Task<IntermediateState<ModerationModel>> Get(Guid id)
    {
        var iState = new IntermediateState<ModerationModel>()
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        var moderation = await _dbContext.Moderations.FindAsync(id);
        if (moderation != null)
        {
            iState.StatusCode = HttpStatusCode.OK;
            iState.Model = StateModel<ModerationModel>.ParseOk((ModerationModel)moderation);
            return iState;
        }

        iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
        return iState;
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
    
    private async Task<IntermediateState<ModerationModel>> _PerformAction(
        Guid objectId,
        ObjectType objectType,
        List<Extra> extras,
        ModerationAction action)
    {
        var moderator = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        var iState = new IntermediateState<ModerationModel>
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        
        switch (objectType)
        {
            case ObjectType.User:
                var user = await _dbContext.Users.FindAsync(objectId);
                if (user == null)
                {
                    iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                    return iState;
                }
                
                switch (action)
                {
                    case ModerationAction.Ban:
                        if (moderator.Id == user.Id) // This can only be attempted by third party clients
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.YouCannotPunishYourself);
                            return iState;
                        }
                        
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                            return iState;
                        }
                        
                        user.IsBanned = true;
                        break;
                    
                    case ModerationAction.Unban:
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                            return iState;
                        }
                        
                        user.IsBanned = false;
                        break;
                    
                    case ModerationAction.Hide:
                        if (moderator.Id == user.Id) // This can only be attempted by third party clients
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.YouCannotPunishYourself);
                            return iState;
                        }
                        
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                            return iState;
                        }
                        
                        user.IsHidden = true;
                        break;
                    
                    case ModerationAction.Show:
                        if (moderator.Role == UserRole.Moderator &&
                            user.Role > UserRole.Default)
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                            return iState;
                        }
                        
                        user.IsHidden = false;
                        break;
                    
                    default:
                        iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                        return iState;
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

                iState.StatusCode = HttpStatusCode.OK;
                iState.Model = StateModel<ModerationModel>.ParseOk((ModerationModel)userModeration);

                return iState;
            
            case ObjectType.Chat:
                var chat = await _dbContext.Chats
                    .Include(dbChat => dbChat.JoinedUsers)
                    .FirstOrDefaultAsync(dbChat => dbChat.Id == objectId);
                
                if (chat == null)
                {
                    iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                    return iState;
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
                        var userToBanId = extras.Get("userId")?.Value ?? "none";

                        if (!Guid.TryParse(userToBanId, out var userToBanIdParsed))
                        {
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
                            return iState;
                        }

                        if (moderator.Id == userToBanIdParsed)
                        {
                            iState.StatusCode = HttpStatusCode.Forbidden;
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.YouCannotPunishYourself);
                            return iState;
                        }

                        var userExists = chat.JoinedUsers.Any(dbUser => dbUser.Id == userToBanIdParsed);

                        if (!userExists)
                        {
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.UserDoesNotExist);
                            return iState;
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
                        var userToUnbanId = extras.Get("userId")?.Value ?? "none";
                        
                        if (!Guid.TryParse(userToUnbanId, out var userToUnbanIdParsed))
                        {
                            iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
                            return iState;
                        }

                        chat.BannedUsers = chat.BannedUsers
                            .Where(ban => ban.UserId != userToUnbanIdParsed)
                            .ToList();
                        break;
                    
                    default:
                        iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                        return iState;
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
                
                iState.StatusCode = HttpStatusCode.OK;
                iState.Model = StateModel<ModerationModel>.ParseOk((ModerationModel)chatModeration);

                return iState;
            
            case ObjectType.ChatMessage:
                var message = await _dbContext.ChatMessages.FindAsync(objectId);
                if (message == null)
                {
                    iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                    return iState;
                }
                
                switch (action)
                {
                    case ModerationAction.Delete:
                        await _dbContext.ChatMessages
                            .Where(dbMessage => dbMessage.Id == message.Id)
                            .ExecuteDeleteAsync();
                        break;
                    
                    default:
                        iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                        return iState;
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
                
                iState.StatusCode = HttpStatusCode.OK;
                iState.Model = StateModel<ModerationModel>.ParseOk((ModerationModel)messageModeration);

                return iState;
            
            case ObjectType.Topic:
                var topic = await _dbContext.Topics.FindAsync(objectId);
                if (topic == null)
                {
                    iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                    return iState;
                }
                
                switch (action)
                {
                    case ModerationAction.Delete:
                        await _dbContext.ChatMessages
                            .Where(dbTopic => dbTopic.Id == topic.Id)
                            .ExecuteDeleteAsync();
                        break;
                    
                    default:
                        iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidAction);
                        return iState;
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

                return iState;
            
            default:
                iState.Model = StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
                return iState;
        }
    }
}
