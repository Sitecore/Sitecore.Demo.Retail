// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateSettlementBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Payments.Braintree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Braintree;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    ///  Defines a validate settlement block.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Orders.SalesActivity,
    ///         Sitecore.Commerce.Plugin.Orders.SalesActivity, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.Pipelines.Blocks.ValidateSettlementBlock)]
    public class ValidateSettlementBlock : PipelineBlock<SalesActivity, SalesActivity, CommercePipelineExecutionContext>
    {
        private readonly IRemoveListEntitiesPipeline _removePipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateSettlementBlock"/> class.
        /// </summary>
        /// <param name="removePipeline">The remove pipeline.</param>
        public ValidateSettlementBlock(IRemoveListEntitiesPipeline removePipeline)
        {
            this._removePipeline = removePipeline;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>A cart with Federated payment info</returns>
        public async override Task<SalesActivity> Run(SalesActivity arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The sales activity cannot be null.");

            var salesActivity = arg;

            if (!salesActivity.HasComponent<FederatedPaymentComponent>())
            {
                return arg;
            }

            var knownSalesActivityStatuses = context.GetPolicy<KnownSalesActivityStatusesPolicy>();
            if (salesActivity.PaymentStatus != null && salesActivity.PaymentStatus.Equals(knownSalesActivityStatuses.Settled, StringComparison.Ordinal))
            {
                return arg;
            }

            var payment = salesActivity.GetComponent<FederatedPaymentComponent>();           

            var transaction = context.CommerceContext.Objects.OfType<Transaction>().FirstOrDefault();
            if (transaction == null)
            {
                return arg;
            }

            arg.DateUpdated = DateTimeOffset.UtcNow;
            
            switch (transaction.Status.ToString())
            {
                case "settled":
                    arg.PaymentStatus = knownSalesActivityStatuses.Settled;
                    await this.MoveToList(arg, context.GetPolicy<KnownOrderListsPolicy>().SettledSalesActivities, context);
                    break;                
                case "submitted_for_settlement":
                    arg.PaymentStatus = knownSalesActivityStatuses.Pending;
                    break;
                default:
                    arg.PaymentStatus = knownSalesActivityStatuses.Problem;
                    await this.MoveToList(arg, context.GetPolicy<KnownOrderListsPolicy>().ProblemSalesActivities, context);
                    context.CommerceContext.AddMessage(
                         context.GetPolicy<KnownResultCodes>().Error,
                         "SettlePaymentFailed",
                         new object[] { payment.TransactionId },
                       $"{this.Name}. Settle payment failed for { payment.TransactionId }: { transaction.ProcessorResponseText }");
                    break;
            }

            return arg; 
        }

        /// <summary>
        /// Moves to list.
        /// </summary>
        /// <param name="salesActivity">The sales activity.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected async virtual Task MoveToList(SalesActivity salesActivity, string listName, CommercePipelineExecutionContext context)
        {
            var transientMemberships = salesActivity.GetComponent<TransientListMembershipsComponent>();
            transientMemberships.Memberships.Remove(context.GetPolicy<KnownOrderListsPolicy>().SettleSalesActivities);
            transientMemberships.Memberships.Add(listName);

            var removeArg = new ListEntitiesArgument(new List<string> { salesActivity.Id }, context.GetPolicy<KnownOrderListsPolicy>().SettleSalesActivities);
            await this._removePipeline.Run(removeArg, context);
        }
    }
}
