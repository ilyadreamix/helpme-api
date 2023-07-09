using System.Net;
using HelpMeApi.Common;
using HelpMeApi.Common.Enum;
using HelpMeApi.Common.State;
using HelpMeApi.Common.State.Model;
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

    public async Task<StateModel<ModerationModel>> Action(ModerationActionRequestModel body)
    {
        ModerationActionInfo actionInfo;

        try
        {
            actionInfo = body.Action.ToModerationActionInfo();
        }
        catch
        {
            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return StateModel<ModerationModel>.ParseFrom(StateCode.InvalidRequest);
        }
        
        return await _PerformAction(Guid.Parse(body.ObjectId), actionInfo);
    }

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
        Guid? moderatorId,
        Guid? objectId,
        ModerationAction? action,
        OrderingMethod? orderingMethod)
    {
        var query = _dbContext.Moderations.AsQueryable();

        if (moderatorId != null)
            query = query.Where(moderation => moderation.ModeratorId == moderatorId);

        if (objectId != null)
            query = query.Where(moderation => moderation.ObjectId == objectId);

        if (action != null)
            query = query.Where(moderation => moderation.Action == action);

        query = orderingMethod == OrderingMethod.ByName
            ? query.OrderBy(moderation => moderation.Action)
            : query.OrderByDescending(moderation => moderation.CreatedAt);

        var moderations = await query
            .Take(25)
            .ToListAsync();
        
        var models = moderations.ConvertAll(moderation => (ModerationModel)moderation);

        return StateModel<List<ModerationModel>>.ParseOk(models);
    }
    
    private async Task<StateModel<ModerationModel>> _PerformAction(
        Guid objectId,
        ModerationActionInfo actionInfo)
    {
        var moderator = (UserEntity)_contextAccessor.HttpContext!.Items["User"]!;
        
        switch (actionInfo.ObjectType)
        {
            case ObjectType.User:
                var user = await _dbContext.Users.FindAsync(objectId);
                if (user == null)
                {
                    _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                }
                
                switch (actionInfo.Action)
                {
                    case ModerationAction.UserBan:
                        if (moderator.Role == UserRole.Moderator && user.Role > UserRole.Default)
                        {
                            _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return StateModel<ModerationModel>.ParseFrom(StateCode.NoRights);
                        }
                        
                        user.IsBanned = !user.IsBanned;
                        break;
                    
                    case ModerationAction.UserHide:
                        user.IsHidden = !user.IsHidden;
                        break;
                }
                
                var userModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = actionInfo.Action
                };

                await _dbContext.Moderations.AddAsync(userModeration);
                await _dbContext.SaveChangesAsync();

                return StateModel<ModerationModel>.ParseOk((ModerationModel)userModeration);
            
            case ObjectType.Chat:
                var chat = await _dbContext.Chats.FindAsync(objectId);
                if (chat == null)
                {
                    _contextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StateModel<ModerationModel>.ParseFrom(StateCode.ContentNotFound);
                }

                switch (actionInfo.Action)
                {
                    case ModerationAction.ChatVerify:
                        chat.IsVerified = !chat.IsVerified;
                        break;
                    case ModerationAction.ChatHide:
                        chat.IsHidden = !chat.IsHidden;
                        break;
                }

                var chatModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = actionInfo.Action
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
                
                switch (actionInfo.Action)
                {
                    case ModerationAction.ChatMessageDelete:
                        await _dbContext.ChatMessages
                            .Where(dbMessage => dbMessage.Id == message.Id)
                            .ExecuteDeleteAsync();
                        break;
                }

                var messageModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = actionInfo.Action
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
                
                switch (actionInfo.Action)
                {
                    case ModerationAction.ChatMessageDelete:
                        await _dbContext.ChatMessages
                            .Where(dbTopic => dbTopic.Id == topic.Id)
                            .ExecuteDeleteAsync();
                        break;
                }

                var topicModeration = new ModerationEntity
                {
                    Id = Guid.NewGuid(),
                    ModeratorId = moderator.Id,
                    ObjectId = objectId,
                    Action = actionInfo.Action
                };

                await _dbContext.Moderations.AddAsync(topicModeration);
                await _dbContext.SaveChangesAsync();

                return StateModel<ModerationModel>.ParseOk((ModerationModel)topicModeration);
            
            default:
                throw new ArgumentOutOfRangeException(nameof(actionInfo.ObjectType), actionInfo.ObjectType, null);
        }
    }
}
