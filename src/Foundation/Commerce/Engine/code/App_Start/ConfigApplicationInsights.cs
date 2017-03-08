using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Engine
{
    public static class ConfigApplicationInsights
    {
        public static void Register(IConfigurationRoot configuration, IServiceCollection services)
        {
            // TODO uncomment for Application Insights
            services.AddApplicationInsightsTelemetry(configuration);

            TelemetryConfiguration.Active.DisableTelemetry = true;

            // TODO uncomment for Application Insights
            services.Add(new ServiceDescriptor(typeof(TelemetryClient), typeof(TelemetryClient), ServiceLifetime.Singleton));
        }
    }
}
