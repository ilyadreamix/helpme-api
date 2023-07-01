namespace HelpMeApi.Common.State;

public enum StateCode
{
    Ok,
    InternalServerError,
    InvalidRequest,
    ContentNotFound,
    InvalidMethod,
    ServiceUnavailable,
    UserAlreadyExists,
    UserDoesNotExist,
    Unauthorized,
    InvalidCredentials,
    TooManyRequests
}
