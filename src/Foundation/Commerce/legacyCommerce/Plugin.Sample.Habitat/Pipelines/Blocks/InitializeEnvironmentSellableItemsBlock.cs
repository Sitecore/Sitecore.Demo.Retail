// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentSellableItemsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Habitat
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Availability;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which bootstraps sellable items the Habitat sample environment.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(HabitatConstants.Pipelines.Blocks.BootstrapAwSellableItemsBlock)]
    public class InitializeEnvironmentSellableItemsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IFindEntityPipeline _findEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentSellableItemsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">
        /// The persist entity pipeline.
        /// </param>
        /// <param name="findEntityPipeline">
        /// The find entity pipeline.
        /// </param>
        public InitializeEnvironmentSellableItemsBlock(IPersistEntityPipeline persistEntityPipeline, IFindEntityPipeline findEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this._findEntityPipeline = findEntityPipeline;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="arg">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.Habitat.SellableItems-1.0";

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
