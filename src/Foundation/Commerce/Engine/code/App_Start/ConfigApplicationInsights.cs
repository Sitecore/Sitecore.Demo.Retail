using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine.App_Start
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
