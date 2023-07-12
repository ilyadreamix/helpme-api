using System.Text.Json;
using HelpMeApi.Chat;
using HelpMeApi.Common;
using HelpMeApi.Common.Auth;
using HelpMeApi.Common.Filter;
using HelpMeApi.Common.GoogleOAuth;
using HelpMeApi.Common.Hash;
using HelpMeApi.Common.Middleware;
using HelpMeApi.Moderation;
using HelpMeApi.User;
using HelpMeApi.Ws;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

services.AddHttpContextAccessor();
services.AddSingleton<HttpClient>();
services.AddDbContext<ApplicationDbContext>();

services.Configure<AuthSettings>(configuration.GetSection("Auth"));
services.AddSingleton<AuthService>();
        
services.Configure<HashSettings>(configuration.GetSection("Hash"));
services.AddSingleton<HashService>();
        
services.Configure<GoogleOAuthSettings>(configuration.GetSection("GoogleOAuth"));
services.AddSingleton<GoogleOAuthService>();

services.AddScoped<UserService>();
services.AddScoped<ChatService>();
services.AddScoped<ModerationService>();

services.AddSingleton<WsConnectionManager>();
services.AddSingleton<WsService>();

services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
services.ConfigureValidationErrorHandler();
services.AddSwaggerGen(options =>
{
    options.SchemaFilter<SwaggerEnumFilter>();
    options.OperationFilter<SwaggerAuthFilter>();
});

var application = builder.Build();

application.ConfigureExceptionHandler();
application.ConfigureClientErrorHandler();

if (application.Environment.IsDevelopment())
{
    application.UseSwagger();
    application.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "docs";
    });
}
        
// application.UseHttpsRedirection();
application.UseRouting();
application.UseMiddleware<AuthMiddleware>();
application.UseWebSockets();
application.MapControllers();

await application.RunAsync();
