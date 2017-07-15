using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Provider.FileSystem;

namespace Sitecore.Demo.Retail.Project.Engine.App_Startup
{
    public static class CommerceEnvironmentExtentions
    {
        public static CommerceEnvironment ConfigureCommerceEnvironment(this IServiceCollection services, IConfigurationRoot configuration, NodeContext node)
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

        public static void Start (this IStartEnvironmentPipeline startEnvironmentPipeline, NodeContext node, IConfigurationRoot configuration)
        {
            // Starting the environment to register Minion policies and run Minions
            var environmentName = configuration.GetSection("AppSettings:EnvironmentName").Value;

            node.AddDataMessage("EnvironmentStartup", $"StartEnvironment={environmentName}");

            startEnvironmentPipeline.Run(environmentName, node.GetPipelineContextOptions()).Wait();
        }
    }
}
