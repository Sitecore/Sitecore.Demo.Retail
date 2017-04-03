using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Engine.App_Startup
{
    public static class ApplicationInsightsExtentions
    {
        public static IServiceCollection ConfigureApplicationInsights(this IServiceCollection services, IConfigurationRoot configuration)
        {
            // TODO uncomment for Application Insights
            services.AddApplicationInsightsTelemetry(configuration);

            TelemetryConfiguration.Active.DisableTelemetry = true;

            // TODO uncomment for Application Insights
            services.Add(new ServiceDescriptor(typeof(TelemetryClient), typeof(TelemetryClient), ServiceLifetime.Singleton));

            return services;
        }
    }
}
