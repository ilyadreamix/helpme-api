using Google.Apis.Discovery;
using HelpMeApi.Common.Auth;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpMeApi.Common.Filter;

public class SwaggerAuthFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isAuthRequired = context.MethodInfo.DeclaringType != null &&
                             context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                                 .Union(context.MethodInfo.GetCustomAttributes(true))
                                 .OfType<AuthRequiredAttribute>()
                                 .Any();

        if (!isAuthRequired)
        {
            return;
        }

        var parameter = new OpenApiParameter
        {
            Name = "Authorization",
            Description = "Bearer token",
            Required = true,
            In = ParameterLocation.Header
        };

        operation.Parameters.Add(parameter);
    }
}
