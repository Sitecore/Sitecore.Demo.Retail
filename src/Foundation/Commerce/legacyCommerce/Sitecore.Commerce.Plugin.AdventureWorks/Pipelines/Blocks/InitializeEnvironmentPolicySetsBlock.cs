// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentPolicySetsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.AdventureWorks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Framework.Pipelines;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Defines a block which bootstraps policy sets for the AdventureWorks sample environment.
    /// </summary>
    [PipelineDisplayName(AwConstants.Pipelines.Blocks.InitializeEnvironmentPolicySetsBlock)]
    public class InitializeEnvironmentPolicySetsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentPolicySetsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">
        /// The find entity pipeline.
        /// </param>
        public InitializeEnvironmentPolicySetsBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
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
        public override Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.AdventureWorks.PolicySets-1.0";

            //Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>().InitialArtifactSets.Contains(artifactSet))
            {
                return Task.FromResult(arg);
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");

            //var globalCartPolicies = new PolicySet
            //{
            //    Id = $"{CommerceEntity.IdPrefix<PolicySet>()}GlobalCartPolicies",
            //    Name = "GlobalCartPolicies",
            //    Policies = new List<Policy>
            //    {
            //        new GlobalCartPolicy
            //        {
            //            PolicyId = typeof(GlobalCartPolicy).Name,
            //             MaximumLineItems = 10,
            //              MaximumCartValue = new MultiCurrency
            //              {
            //                   Values = new List<Money>
            //                   {
            //                       new Money { CurrencyCode = "USD", Amount = 1000M },
            //                       new Money { CurrencyCode = "CAD", Amount = 500M }
            //                   }
            //              },
            //              ShowCartAfterAdd = true
            //        },
            //        new CartCouponsPolicy
            //        {
            //            PolicyId = typeof(CartCouponsPolicy).Name,
            //             MaxCouponsInCart = 2
            //        }
            //    }
            //};
            //await this._persistEntityPipeline.Run(new PersistEntityArgument(globalCartPolicies), context);

            //var globalSellableItemPolicies = new PolicySet
            //{
            //    Id = $"{CommerceEntity.IdPrefix<PolicySet>()}GlobalSellableItemPolicies",
            //    Name = "GlobalSellableItemPolicies",
            //    Policies = new List<Policy>()
            //};
            //await this._persistEntityPipeline.Run(new PersistEntityArgument(globalSellableItemPolicies), context);

            return Task.FromResult(arg);
        }
    }
}
