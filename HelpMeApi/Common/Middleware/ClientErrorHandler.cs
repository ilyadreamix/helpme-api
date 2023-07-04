using System.Net;
using HelpMeApi.Common.State;

namespace HelpMeApi.Common.Middleware;

public static class ClientErrorHandler
{
    public static void ConfigureClientErrorHandler(this IApplicationBuilder application)
    {
        application.Use(async (context, next) =>
        {
            await next();
            var status = (HttpStatusCode)context.Response.StatusCode;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (status)
            {
                case HttpStatusCode.NotFound:
                    await context.Response.WriteAsJsonAsync(DefaultState.ContentNotFound);
                    return;
                
                case HttpStatusCode.UnsupportedMediaType:
                case HttpStatusCode.UnprocessableEntity:
                case HttpStatusCode.BadRequest:
                    if (!context.Response.HasStarted)
                    {
                        await context.Response.WriteAsJsonAsync(DefaultState.InvalidRequest);
                    }
                    return;
                
                case HttpStatusCode.MethodNotAllowed:
                    await context.Response.WriteAsJsonAsync(DefaultState.InvalidMethod);
                    return;
                
                default: return;
            }
        });
    }
}
