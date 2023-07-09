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
}
