using System.Net;
using HelpMeApi.Common.State;
using Microsoft.AspNetCore.Mvc;

namespace HelpMeApi.Common.Middleware;

public static class ValidationErrorHandler
{
    public static void ConfigureValidationErrorHandler(this IServiceCollection services)
    {
        services.PostConfigure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = (context =>
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new JsonResult(DefaultState.InvalidRequest);
            });
        });
    }
}
