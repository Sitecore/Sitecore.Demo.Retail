using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Demo.Retail.Feature.HabitatData.Engine.Pipelines.Blocks;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Sitecore.Demo.Retail.Feature.HabitatData.Engine
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);
            services.RegisterAllCommands(assembly);

            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IInitializeEnvironmentPipeline>(c =>
                {
                    c.Add<InitializeEnvironmentGiftCardsBlock>()
                        .Add<InitializeEnvironmentSellableItemsBlock>()
                        .Add<InitializeEnvironmentPricingBlock>()
                        .Add<InitializeEnvironmentPromotionsBlock>();
                })
            );
        }
    }
}
