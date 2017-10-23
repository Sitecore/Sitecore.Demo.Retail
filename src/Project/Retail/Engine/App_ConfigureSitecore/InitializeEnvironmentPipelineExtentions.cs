using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Project.Commerce.Retail.Engine.Pipelines.Blocks;

namespace Sitecore.Project.Commerce.Retail.Engine.App_ConfigureSitecore
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
