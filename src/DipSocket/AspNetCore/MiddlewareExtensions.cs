using Microsoft.AspNetCore.Builder;

namespace DipSocket.AspNetCore
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDipSocket(this IApplicationBuilder builder)
        {
            builder.UseWebSockets();

            return builder.UseMiddleware<DipSocketMiddleware>();
        }
    }
}
