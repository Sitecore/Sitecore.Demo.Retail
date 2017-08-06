using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Framework.Rules;

namespace Sitecore.Project.Commerce.Retail.Engine.App_Startup
{
    public static class SitecoreExtentions
    {
        public static void ConfigureSitecore(this IServiceCollection services, IServiceProvider hostServices)
        {
            var logger = ApplicationLogging.CreateLogger("ConfigSitecore");

            logger.LogInformation("BootStrapping Services...");

            services.Sitecore()
                .Eventing()
                //// .Bootstrap(this._hostServices)
                //// .AddServicesDiagnostics()
                .Caching(config => config
                    .AddMemoryStore("GlobalEnvironment")
                    .ConfigureCaches("GlobalEnvironment.*", "GlobalEnvironment"))
            //// .AddCacheDiagnostics()
            .Rules(config => config
                .IgnoreNamespaces(n => n.Equals("Sitecore.Commerce.Plugin.Tax")))
            .RulesSerialization();
            services.Add(new ServiceDescriptor(typeof(IRuleBuilderInit), typeof(RuleBuilder), ServiceLifetime.Transient));

            logger.LogInformation("BootStrapping application...");
            services.Sitecore().Bootstrap(hostServices);
        }
    }
}
