using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Foundation.Commerce.Engine.App_ConfigureSitecore;
using Sitecore.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.RegisterAllPipelineBlocks(assembly);
            services.ConfigureInitializeEnvironmentPipeline();
            services.ConfigureRunningPluginsPipeline();
            //services.ConfigureCartPipelines();
            //services.ConfigureOrdersPipelines();
        }
    }
}
