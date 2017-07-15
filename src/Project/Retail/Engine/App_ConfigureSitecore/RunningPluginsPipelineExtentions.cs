using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Demo.Retail.Project.Engine.Pipelines.Blocks;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Sitecore.Demo.Retail.Project.Engine.App_ConfigureSitecore
{
    public static class RunningPluginsPipelineExtentions
    {
        public static IServiceCollection ConfigureRunningPluginsPipeline(this IServiceCollection services)
        {
            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IRunningPluginsPipeline>(c =>
                {
                    c.Add<RegisteredPluginBlock>().After<RunningPluginsBlock>();
                })

            );

            return services;
        }
    }
}
