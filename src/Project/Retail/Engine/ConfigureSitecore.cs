using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Demo.Retail.Project.Engine.App_ConfigureSitecore;
using Sitecore.Framework.Configuration;

namespace Sitecore.Demo.Retail.Project.Engine
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.RegisterAllPipelineBlocks(assembly);
            services.ConfigureInitializeEnvironmentPipeline();
            services.ConfigureRunningPluginsPipeline();
            services.ConfigureCartPipelines();
            services.ConfigureOrdersPipelines();
        }
    }
}
