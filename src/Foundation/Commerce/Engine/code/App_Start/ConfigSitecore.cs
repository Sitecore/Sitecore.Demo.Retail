using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Framework.Rules;

namespace Sitecore.Foundation.Commerce.Engine
{
    public static class ConfigSitecore
    {
        public static void Register(IServiceProvider hostServices, IServiceCollection services)
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
