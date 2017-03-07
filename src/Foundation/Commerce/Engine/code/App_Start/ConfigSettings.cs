using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Engine
{
    public static class ConfigSettings
    {
        public static IConfigurationRoot Register(IHostingEnvironment hostEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostEnv.WebRootPath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();

            // TODO uncomment for Application Insights
            if (hostEnv.IsDevelopment())
            {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            return builder.Build();
        }
    }
}
