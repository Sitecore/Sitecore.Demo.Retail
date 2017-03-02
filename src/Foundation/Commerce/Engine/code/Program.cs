using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Sitecore.Foundation.Commerce.Engine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<App_Start.Startup>()
                .Build();

            host.Run();
        }
    }
}
