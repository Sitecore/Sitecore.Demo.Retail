using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Project.Retail.Engine.Pipelines.Blocks;

namespace Sitecore.Project.Retail.Engine.App_ConfigureSitecore
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
