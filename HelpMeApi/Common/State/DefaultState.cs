using System.Text.Json.Nodes;
using HelpMeApi.Common.State.Model;

namespace HelpMeApi.Common.State;

public static class DefaultState
{
    public static readonly StateModel<JsonObject> Ok = new()
    {
        Code = (int)StateCode.Ok,
        State = StateCode.Ok.ToString(),
        HasError = false
    };

    public static readonly StateModel<JsonObject> InternalServerError = new()
    {
        Code = (int)StateCode.InternalServerError,
        State = StateCode.InternalServerError.ToString()
    };

    public static readonly StateModel<JsonObject> InvalidRequest = new()
    {
        Code = (int)StateCode.InvalidRequest,
        State = StateCode.InvalidRequest.ToString()
    };

    public static readonly StateModel<JsonObject> ContentNotFound = new()
    {
        Code = (int)StateCode.ContentNotFound,
        State = StateCode.ContentNotFound.ToString()
    };
    
    public static readonly StateModel<JsonObject> InvalidMethod = new()
    {
        Code = (int)StateCode.InvalidMethod,
        State = StateCode.InvalidMethod.ToString()
    };
    
    public static readonly StateModel<JsonObject> Unauthorized = new()
    {
        Code = (int)StateCode.Unauthorized,
        State = StateCode.Unauthorized.ToString()
    };
    
    public static readonly StateModel<JsonObject> YouAreBanned = new()
    {
        Code = (int)StateCode.YouAreBanned,
        State = StateCode.YouAreBanned.ToString()
    };
    
    public static readonly StateModel<JsonObject> InvalidCredentials = new()
    {
        Code = (int)StateCode.InvalidCredentials,
        State = StateCode.InvalidCredentials.ToString()
    };
    
    public static readonly StateModel<JsonObject> NoRights = new()
    {
        Code = (int)StateCode.NoRights,
        State = StateCode.NoRights.ToString()
    };
    
    public static readonly StateModel<JsonObject> YouHaveAlreadyJoinedThisChat = new()
    {
        Code = (int)StateCode.YouHaveAlreadyJoinedThisChat,
        State = StateCode.YouHaveAlreadyJoinedThisChat.ToString()
    };
    
    public static readonly StateModel<JsonObject> YouAreBannedInThisChat = new()
    {
        Code = (int)StateCode.YouAreBannedInThisChat,
        State = StateCode.YouAreBannedInThisChat.ToString()
    };
    
    public static readonly StateModel<JsonObject> ThisChatIsDisabled = new()
    {
        Code = (int)StateCode.ThisChatIsDisabled,
        State = StateCode.ThisChatIsDisabled.ToString()
    };
    
    public static readonly StateModel<JsonObject> YouWasNotInvitedToThisChat = new()
    {
        Code = (int)StateCode.YouWasNotInvitedToThisChat,
        State = StateCode.YouWasNotInvitedToThisChat.ToString()
    };
    
    public static readonly StateModel<JsonObject> UserDoesNotExist = new()
    {
        Code = (int)StateCode.UserDoesNotExist,
        State = StateCode.UserDoesNotExist.ToString()
    };
    
    public static readonly StateModel<JsonObject> UserHasAlreadyJoinedThisChat = new()
    {
        Code = (int)StateCode.UserHasAlreadyJoinedThisChat,
        State = StateCode.UserHasAlreadyJoinedThisChat.ToString()
    };
    
    public static readonly StateModel<JsonObject> UserWasAlreadyInvited = new()
    {
        Code = (int)StateCode.UserWasAlreadyInvited,
        State = StateCode.UserWasAlreadyInvited.ToString()
    };
    
    public static readonly StateModel<JsonObject> YouCannotEditThisChat = new()
    {
        Code = (int)StateCode.YouCannotEditThisChat,
        State = StateCode.YouCannotEditThisChat.ToString()
    };
    
    public static readonly StateModel<JsonObject> UnbanUserToInvite = new()
    {
        Code = (int)StateCode.UnbanUserToInvite,
        State = StateCode.UnbanUserToInvite.ToString()
    };
}
