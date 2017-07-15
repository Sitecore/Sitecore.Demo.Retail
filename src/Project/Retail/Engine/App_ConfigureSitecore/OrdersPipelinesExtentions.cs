using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Plugin.Inventory.Cs;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Sitecore.Demo.Retail.Project.Engine.App_ConfigureSitecore
{
    public static class OrdersPipelinesExtentions
    {
        public static IServiceCollection ConfigureOrdersPipelines(this IServiceCollection services)
        {
            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IItemOrderedPipeline>(builder => builder
                    .Add<UpdateItemsOrderedInventoryBlock>()));

            //.ConfigurePipeline<IOrderPlacedPipeline>(builder => builder.Add<UpdateOrderedGiftCardBalanceBlock>()));

            return services;
        }
    }
}
