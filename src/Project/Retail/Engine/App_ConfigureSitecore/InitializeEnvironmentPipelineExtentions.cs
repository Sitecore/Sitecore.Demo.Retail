using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Demo.Retail.Project.Engine.Pipelines.Blocks;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Sitecore.Demo.Retail.Project.Engine.App_ConfigureSitecore
{
    public static class InitializeEnvironmentPipelineExtentions
    {
        public static IServiceCollection ConfigureInitializeEnvironmentPipeline(this IServiceCollection services)
        {
            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IInitializeEnvironmentPipeline>(builder => builder
                    .Add<BootstrapManagedListsBlock>()
                )

            );

            return services;
        }
    }
}
