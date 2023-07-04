using System.Text.Json;
using HelpMeApi.Account;
using HelpMeApi.Common;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.GoogleOAuth;
using HelpMeApi.Common.Hash;
using HelpMeApi.Common.Middleware;

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
        services.AddSingleton<HttpClient>();
        
        services.AddDbContext<ApplicationDbContext>();

        /* services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                var issuer = _configuration["Auth:JwtIssuer"];
                var audience = _configuration["Auth:JwtAudience"];
                var key = Encoding.UTF8.GetBytes(_configuration["Auth:JwtKey"]!);
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true
                };
            });
        services.AddAuthorization(); */

        services.Configure<AuthSettings>(_configuration.GetSection("Auth"));
        services.AddSingleton<AuthService>();
        
        services.Configure<HashSettings>(_configuration.GetSection("Hash"));
        services.AddSingleton<HashService>();
        
        services.Configure<GoogleOAuthSettings>(_configuration.GetSection("GoogleOAuth"));
        services.AddSingleton<GoogleOAuthService>();

        services.AddScoped<AccountService>();
        
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        services.ConfigureValidationErrorHandler();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder application, IWebHostEnvironment environment, ApplicationDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();
        
        application.ConfigureExceptionHandler();
        application.ConfigureClientErrorHandler();
        
        // application.UseHttpsRedirection();
        application.UseRouting();
        application.UseMiddleware<AuthMiddleware>();
        application.UseEndpoints(endpoints => endpoints.MapControllers());

        /* application.UseAuthentication();
        application.UseAuthorization(); */
    }
}
