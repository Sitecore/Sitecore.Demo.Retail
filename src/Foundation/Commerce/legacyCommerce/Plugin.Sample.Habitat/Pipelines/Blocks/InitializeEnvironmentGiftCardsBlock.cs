// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentGiftCardsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Habitat
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Availability;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Entitlements;
    using Sitecore.Commerce.Plugin.GiftCards;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Pricing;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which initializes pricing.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName("Habitat.InitializeEnvironmentGiftCardsBlock")]
    public class InitializeEnvironmentGiftCardsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IFindEntityPipeline _findEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentGiftCardsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        /// <param name="findEntityPipeline">
        /// The find entity pipeline.
        /// </param>
        public InitializeEnvironmentGiftCardsBlock(
            IPersistEntityPipeline persistEntityPipeline, IFindEntityPipeline findEntityPipeline)
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
        /// The <see cref="string"/>.
        /// </returns>
        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            if (arg != "Habitat" && arg != "HabitatShops")
            {
                return arg;
            }

            var itemId = $"{CommerceEntity.IdPrefix<SellableItem>()}6042986";
            var findResult = await this._findEntityPipeline.Run(new FindEntityArgument(typeof(SellableItem), itemId, false), context.CommerceContext.GetPipelineContextOptions());

            if (findResult == null)
            {
                var giftCard = new SellableItem
                {
                    Id = itemId,
                    Name = "Default GiftCard",
                    Policies = new List<Policy>
                {
                    new AvailabilityAlwaysPolicy(),
                    new ListPricingPolicy(new List<Money> {new Money("USD", 0M), new Money("CAD", 0M)})
                },
                    Components = new List<Component>
                {
                    new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                            {
                                new ItemVariationComponent
                                {
                                    Id = "56042986",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 25M), new Money("CAD", 26M) })
                                    }
                                },
                                new ItemVariationComponent
                                {
                                    Id = "56042987",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                    }
                                },
                                new ItemVariationComponent
                                {
                                    Id = "56042988",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 100M), new Money("CAD", 101M) })
                                    }
                                }
                            }
                     }
                }
                };

                await this._persistEntityPipeline.Run(new PersistEntityArgument(giftCard), context);
            }
            return arg;
        }
    }
}
