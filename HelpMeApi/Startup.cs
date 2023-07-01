using System.Text.Json;
using HelpMeApi.Common.Middleware;

namespace HelpMeApi;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
        services.ConfigureValidationErrorHandler();
    }

    public static void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
    {
        application.ConfigureExceptionHandler();
        application.ConfigureClientErrorHandler();

        application.UseHttpsRedirection();
        application.UseRouting();
        application.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
