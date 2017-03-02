using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Provider.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine.App_Start
{
    public static class ConfigEnvironment
    {
        /// <summary>
        /// Gets the global environment.
        /// </summary>
        /// <returns>A <see cref="CommerceEnvironment"/></returns>
        public static CommerceEnvironment Register(IConfigurationRoot configuration, NodeContext node, IServiceCollection services)
        {
            var logger = ApplicationLogging.CreateLogger("ConfigEnvironment");

            logger.LogInformation($"Loading Global Environment using Filesystem Provider from: {node.BootStrapProviderPath}");
            var bootstrapProvider = new FileSystemEntityProvider(node.BootStrapProviderPath);

            var appSettingBootstrapFileValue = configuration.GetSection("AppSettings:BootStrapFile").Value;
            if (!string.IsNullOrEmpty(appSettingBootstrapFileValue))
            {
                node.BootStrapEnvironmentPath = appSettingBootstrapFileValue;
            }

            node.AddDataMessage("NodeStartup", $"GlobalEnvironmentFrom='{node.BootStrapEnvironmentPath}.json'");

            var environment = bootstrapProvider.Find<CommerceEnvironment>(node, node.BootStrapEnvironmentPath, false).Result;

            services.AddSingleton(environment);

            return environment;
        }
    }
}
