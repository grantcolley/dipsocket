using Microsoft.AspNetCore.Hosting;

namespace Server
{
    public static class WebHostExtensions
    {
        public static IWebHostBuilder UseWebSocketTestStartup(this IWebHostBuilder webHost)
        {
            return webHost.UseStartup<Startup>();
        }
    }
}
