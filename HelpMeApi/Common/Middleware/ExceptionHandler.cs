using System.Net;
using HelpMeApi.Common.State;

namespace HelpMeApi.Common.Middleware;

public static class ExceptionHandler
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder application)
    {
        application.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsJsonAsync(DefaultState.InternalServerError);
            });
        });
    }
}
