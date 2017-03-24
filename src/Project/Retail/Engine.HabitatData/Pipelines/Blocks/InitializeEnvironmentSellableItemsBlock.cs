using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Project.Commerce.Engine.Plugin.HabitatData.Pipelines.Blocks
{
    [PipelineDisplayName(HabitatDataConstants.Pipelines.Blocks.InitializeEnvironmentSellableItemsBlock)]
    public class InitializeEnvironmentSellableItemsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IFindEntityPipeline _findEntityPipeline;

        public InitializeEnvironmentSellableItemsBlock(IPersistEntityPipeline persistEntityPipeline, IFindEntityPipeline findEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this._findEntityPipeline = findEntityPipeline;
        }

        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = HabitatDataConstants.ArtifactSets.SellableItems;

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>()
                .InitialArtifactSets.Contains(artifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");


            // DEFAULT SUBSCRIPTION SELLABLE ITEM TEMPLATE
            var itemId = $"{CommerceEntity.IdPrefix<SellableItem>()}Subscription";
            var findResult = await this._findEntityPipeline.Run(new FindEntityArgument(typeof(SellableItem), itemId, false), context.CommerceContext.GetPipelineContextOptions());

            if (findResult == null)
            {
                var subscriptionSellableItem = new SellableItem
                {
                    Id = itemId,
                    Name = "Default Subscription",
                    Policies = new List<Policy>
                    {
                        new AvailabilityAlwaysPolicy()
                    },
                    Components = new List<Component>
                    {
                        new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } }
                    }
                };
                await this._persistEntityPipeline.Run(new PersistEntityArgument(subscriptionSellableItem), context);
            }

            // DEFAULT INSTALLATION SELLABLE ITEM TEMPLATE
            itemId = $"{CommerceEntity.IdPrefix<SellableItem>()}Installation";
            findResult = await this._findEntityPipeline.Run(new FindEntityArgument(typeof(SellableItem), itemId, false), context.CommerceContext.GetPipelineContextOptions());

            if (findResult == null)
            {
                var installationSellableItem = new SellableItem
                {
                    Id = itemId,
                    Name = "Default Installation",
                    Policies = new List<Policy>
                    {
                        new AvailabilityAlwaysPolicy()
                    },
                    Components = new List<Component>
                    {
                        new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } }
                    }
                };
                await this._persistEntityPipeline.Run(new PersistEntityArgument(installationSellableItem), context);
            }

            // DEFAULT WARRANTY SELLABLE ITEM TEMPLATE
            itemId = $"{CommerceEntity.IdPrefix<SellableItem>()}Warranty";
            findResult = await this._findEntityPipeline.Run(new FindEntityArgument(typeof(SellableItem), itemId, false), context.CommerceContext.GetPipelineContextOptions());

            if (findResult == null)
            {
                var warrantySellableItem = new SellableItem
                {
                    Id = itemId,
                    Name = "Default Warranty",
                    Policies = new List<Policy>
                    {
                        new AvailabilityAlwaysPolicy()
                    },
                    Components = new List<Component>
                    {
                        new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } }
                    }
                };
                await this._persistEntityPipeline.Run(new PersistEntityArgument(warrantySellableItem), context);
            }

            return arg;
        }
    }
}
