using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Engine.App_Startup
{
    public static class SettingsExtentions
    {
        public static IConfigurationRoot ConfigureSettings(this IHostingEnvironment hostEnv)
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
