using System.Text.Json;
using HelpMeApi.Chat;
using HelpMeApi.Common;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.GoogleOAuth;
using HelpMeApi.Common.Hash;
using HelpMeApi.Common.Middleware;
using HelpMeApi.User;

namespace HelpMeApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
    
        services.AddHttpContextAccessor();
        services.AddSingleton<HttpClient>();
        services.AddDbContext<ApplicationDbContext>();

        services.Configure<AuthSettings>(_configuration.GetSection("Auth"));
        services.AddSingleton<AuthService>();
        
        services.Configure<HashSettings>(_configuration.GetSection("Hash"));
        services.AddSingleton<HashService>();
        
        services.Configure<GoogleOAuthSettings>(_configuration.GetSection("GoogleOAuth"));
        services.AddSingleton<GoogleOAuthService>();

        services.AddScoped<UserService>();
        services.AddScoped<ChatService>();
        
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        services.ConfigureValidationErrorHandler();
    }

    public void Configure(IApplicationBuilder application, IWebHostEnvironment environment, ApplicationDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();
        
        application.ConfigureExceptionHandler();
        application.ConfigureClientErrorHandler();
        
        // application.UseHttpsRedirection();
        application.UseRouting();
        application.UseMiddleware<AuthMiddleware>();
        application.UseWebSockets();
        application.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
