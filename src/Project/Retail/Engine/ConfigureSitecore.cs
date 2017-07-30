using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Configuration;
using Sitecore.Project.Retail.Engine.App_ConfigureSitecore;

namespace Sitecore.Project.Retail.Engine
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
