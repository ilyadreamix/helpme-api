using Microsoft.AspNetCore;

namespace HelpMeApi;

public static class Program
{
    public static async Task Main(string[] arguments)
    {
        var webHost = WebHost
            .CreateDefaultBuilder(arguments)
            .UseStartup<Startup>()
            .Build();
        
        await webHost.RunAsync();
    }
}
