using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            string url;

            if (args == null
                || args.Length.Equals(0))
            {
                url = "http://+:6000";
            }
            else
            {
                url = args[0];
            }

            try
            {
                var webHost = WebHost.CreateDefaultBuilder()
                    .UseUrls(url)                    
                    .UseWebSocketTestStartup()
                    .Build();

                var task = webHost.RunAsync();
                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var read = Console.ReadLine();
            }
        }
    }
    }
}
