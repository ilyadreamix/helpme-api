using Microsoft.AspNetCore;

namespace HelpMeApi;

public static class Program
{
    public static void Main(string[] arguments)
    {
        var webHost = WebHost
            .CreateDefaultBuilder(arguments)
            .UseStartup<Startup>()
            .Build();
        
        webHost.Run();
    }
}
