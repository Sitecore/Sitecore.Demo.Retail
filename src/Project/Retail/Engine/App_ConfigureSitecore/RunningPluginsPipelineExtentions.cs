using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Project.Retail.Engine.Pipelines.Blocks;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Project.Retail.Engine.App_ConfigureSitecore
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
