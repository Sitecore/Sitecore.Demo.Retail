using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Demo.Retail.Feature.HabitatData.Engine.Pipelines.Blocks
{
    [PipelineDisplayName(HabitatDataConstants.Pipelines.Blocks.InitializeEnvironmentPricingBlock)]
    public class InitializeEnvironmentPricingBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IAddPriceBookPipeline _addPriceBookPipeline;
        private readonly IAddPriceCardPipeline _addPriceCardPipeline;
        private readonly IAddPriceSnapshotPipeline _addPriceSnapshotPipeline;
        private readonly IAddPriceTierPipeline _addPriceTierPipeline;
        private readonly IAddPriceSnapshotTagPipeline _addPriceSnapshotTagPipeline;
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        public InitializeEnvironmentPricingBlock(
            IAddPriceBookPipeline addPriceBookPipeline,
            IAddPriceCardPipeline addPriceCardPipeline,
            IAddPriceSnapshotPipeline addPriceSnapshotPipeline,
            IAddPriceTierPipeline addPriceTierPipeline,
            IAddPriceSnapshotTagPipeline addPriceSnapshotTagPipeline,
            IPersistEntityPipeline persistEntityPipeline)
        {
            this._addPriceBookPipeline = addPriceBookPipeline;
            this._addPriceCardPipeline = addPriceCardPipeline;
            this._addPriceSnapshotPipeline = addPriceSnapshotPipeline;
            this._addPriceTierPipeline = addPriceTierPipeline;
            this._addPriceSnapshotTagPipeline = addPriceSnapshotTagPipeline;
            this._persistEntityPipeline = persistEntityPipeline;
        }

        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.Habitat.Pricing-1.0";

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>().InitialArtifactSets.Contains(artifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");

            try
            {

                // BOOK
                var book = await this._addPriceBookPipeline.Run(
                    new AddPriceBookArgument("Habitat_PriceBook")
                    {
                        ParentBook = string.Empty,
                        Description = "Habitat price book",
                        DisplayName = "Habitat",
                        CurrencySetId = "{4B494292-598E-4A61-A156-D7501F7953ED}"
                    },
                    context);

                this.CreateProductsCard(book, context);

                this.CreateVariantsCard(book, context);

                this.CreateTagsCard(book, context);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(new EventId(), ex, $"{this.Name}.Exception: Message={ex.Message}");
            }

            return arg;
        }

        /// <summary>
        /// Creates the products card.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateProductsCard(PriceBook book, CommercePipelineExecutionContext context)
        {
            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            var date = DateTimeOffset.UtcNow;

            // CARD
            var adventureCard = this._addPriceCardPipeline.Run(new AddPriceCardArgument(book, "Habitat_PriceCard"), context).Result;

            // READY FOR APPROVAL SNAPSHOT
            adventureCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureCard, new PriceSnapshotComponent(date.AddMinutes(-10))), context).Result;
            var readyForApprovalSnapshot = adventureCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, readyForApprovalSnapshot, new PriceTier("USD", 1, 2000M)), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            // CARD FIRST SNAPSHOT
            adventureCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureCard, new PriceSnapshotComponent(date.AddHours(-1))), context).Result;
            var firstSnapshot = adventureCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("USD", 1, 10M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("USD", 5, 5M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("USD", 10, 1M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("CAD", 1, 15M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("CAD", 5, 10M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("CAD", 10, 5M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, firstSnapshot, new PriceTier("EUR", 1, 1M)), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            // DRAFT SNAPSHOT
            adventureCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureCard, new PriceSnapshotComponent(date)), context).Result;
            var draftSnapshot = adventureCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, draftSnapshot, new PriceTier("USD", 1, 1000M)), context).Result;

            adventureCard = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(adventureCard, draftSnapshot, new Tag("new pricing")), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            // CARD SECOND SNAPSHOT
            adventureCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureCard, new PriceSnapshotComponent(date.AddDays(30))), context).Result;
            var secondSnapshot = adventureCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("USD", 1, 7M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("USD", 5, 4M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("USD", 10, 3M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("CAD", 1, 6M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("CAD", 5, 3M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("CAD", 10, 2M)), context).Result;
            adventureCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureCard, secondSnapshot, new PriceTier("EUR", 1, 1M)), context).Result;

            adventureCard = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(adventureCard, secondSnapshot, new Tag("future pricing")), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            readyForApprovalSnapshot?.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().ReadyForApproval));
            firstSnapshot?.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            secondSnapshot?.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));

            this._persistEntityPipeline.Run(new PersistEntityArgument(adventureCard), context).Wait();
        }

        /// <summary>
        /// Creates the variants card.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateVariantsCard(PriceBook book, CommercePipelineExecutionContext context)
        {
            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            var date = DateTimeOffset.UtcNow;

            // VARIANTS CARD
            var adventureVariantsCard = this._addPriceCardPipeline.Run(new AddPriceCardArgument(book, "Habitat_VariantsPriceCard"), context).Result;

            // READY FOR APPROVAL SNAPSHOT
            adventureVariantsCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureVariantsCard, new PriceSnapshotComponent(date.AddMinutes(-10))), context).Result;
            var readyForApprovalSnapshot = adventureVariantsCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, readyForApprovalSnapshot, new PriceTier("USD", 1, 2000M)), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            // FIRST APPROVED SNAPSHOT
            adventureVariantsCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureVariantsCard, new PriceSnapshotComponent(date.AddHours(-1))), context).Result;
            var firstSnapshot = adventureVariantsCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("USD", 1, 9M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("USD", 5, 6M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("USD", 10, 3M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("CAD", 1, 7M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("CAD", 5, 4M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("CAD", 10, 2M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, firstSnapshot, new PriceTier("EUR", 1, 2M)), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            // DRAFT SNAPSHOT
            adventureVariantsCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureVariantsCard, new PriceSnapshotComponent(date)), context).Result;
            var draftSnapshot = adventureVariantsCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, draftSnapshot, new PriceTier("USD", 1, 1000M)), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            // SECOND APPROVED SNAPSHOT
            adventureVariantsCard = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(adventureVariantsCard, new PriceSnapshotComponent(date.AddDays(30))), context).Result;
            var secondSnapshot = adventureVariantsCard.Snapshots.FirstOrDefault(s => s.Id.Equals(context.CommerceContext.Models.OfType<PriceSnapshotAdded>().FirstOrDefault()?.PriceSnapshotId, StringComparison.OrdinalIgnoreCase));

            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("USD", 1, 8M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("USD", 5, 4M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("USD", 10, 2M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("CAD", 1, 7M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("CAD", 5, 3M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("CAD", 10, 1M)), context).Result;
            adventureVariantsCard = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(adventureVariantsCard, secondSnapshot, new PriceTier("EUR", 1, 2M)), context).Result;

            context.CommerceContext.Models.RemoveAll(m => m is PriceSnapshotAdded || m is PriceTierAdded);

            readyForApprovalSnapshot?.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().ReadyForApproval));
            firstSnapshot?.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            secondSnapshot?.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(adventureVariantsCard), context).Wait();
        }

        /// <summary>
        /// Creates the tags card.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateTagsCard(PriceBook book, CommercePipelineExecutionContext context)
        {
            // TAGS CARD
            var card = this._addPriceCardPipeline.Run(new AddPriceCardArgument(book, "Habitat_TagsPriceCard"), context).Result;

            // TAGS CARD FIRST SNAPSHOT
            card = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(card, new PriceSnapshotComponent(DateTimeOffset.UtcNow)), context).Result;
            var firstSnapshot = card.Snapshots.FirstOrDefault();

            // TAGS CARD FIRST SNAPSHOT  TIERS
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, firstSnapshot, new PriceTier("USD", 1, 250M)), context).Result;
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, firstSnapshot, new PriceTier("USD", 5, 200M)), context).Result;
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, firstSnapshot, new PriceTier("CAD", 1, 251M)), context).Result;
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, firstSnapshot, new PriceTier("CAD", 5, 201M)), context).Result;

            // TAGS CARD FIRST SNAPSHOT TAGS
            card = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(card, firstSnapshot, new Tag("Habitat")), context).Result;
            card = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(card, firstSnapshot, new Tag("Habitat 2")), context).Result;
            card = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(card, firstSnapshot, new Tag("common")), context).Result;

            // TAGS CARD SECOND SNAPSHOT
            card = this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(card, new PriceSnapshotComponent(DateTimeOffset.UtcNow.AddSeconds(1))), context).Result;
            var secondSnapshot = card.Snapshots.FirstOrDefault(s => !s.Id.Equals(firstSnapshot?.Id));

            // TAGS CARD SECOND SNAPSHOT TIERS
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, secondSnapshot, new PriceTier("USD", 1, 150M)), context).Result;
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, secondSnapshot, new PriceTier("USD", 5, 100M)), context).Result;
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, secondSnapshot, new PriceTier("CAD", 1, 101M)), context).Result;
            card = this._addPriceTierPipeline.Run(new PriceCardSnapshotTierArgument(card, secondSnapshot, new PriceTier("CAD", 5, 151M)), context).Result;

            // TAGS CARD SECOND SNAPSHOT TAGS
            card = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(card, secondSnapshot, new Tag("Habitat variants")), context).Result;
            card = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(card, secondSnapshot, new Tag("Habitat variants 2")), context).Result;
            card = this._addPriceSnapshotTagPipeline.Run(new PriceCardSnapshotTagArgument(card, secondSnapshot, new Tag("common")), context).Result;

            // TAGS CARD APPROVAl COMPONENT
            card.Snapshots.ForEach(s =>
            {
                s.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            });
            this._persistEntityPipeline.Run(new PersistEntityArgument(card), context).Wait();
        }
    }
}