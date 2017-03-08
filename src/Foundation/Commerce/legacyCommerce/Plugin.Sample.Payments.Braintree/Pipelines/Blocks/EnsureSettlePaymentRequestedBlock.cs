// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnsureSettlePaymentRequestedBlock.cs" company="Sitecore Corporation">
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
    using global::Braintree.Exceptions;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    ///  Defines an ensure settle payment requested block.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Orders.SalesActivity,
    ///         Sitecore.Commerce.Plugin.Orders.SalesActivity, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.Pipelines.Blocks.EnsureSettlePaymentRequestedBlock)]
    public class EnsureSettlePaymentRequestedBlock : PipelineBlock<SalesActivity, SalesActivity, CommercePipelineExecutionContext>
    {
        private readonly IRemoveListEntitiesPipeline _removePipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnsureSettlePaymentRequestedBlock"/> class.
        /// </summary>
        /// <param name="removePipeline">The remove pipeline.</param>
        public EnsureSettlePaymentRequestedBlock(IRemoveListEntitiesPipeline removePipeline)
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

            var payment = salesActivity.GetComponent<FederatedPaymentComponent>();                   

            if (string.IsNullOrEmpty(payment.TransactionId))
            {
                await this.MoveToProblemList(arg, context);
                context.Abort(context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "InvalidOrMissingPropertyValue",
                    new object[] { "TransactionId" },
                    $"Invalid or missing value for property 'TransactionId'."), context);

                return arg;
            }

            var braintreeClientPolicy = context.GetPolicy<BraintreeClientPolicy>();
            if (string.IsNullOrEmpty(braintreeClientPolicy?.Environment) || string.IsNullOrEmpty(braintreeClientPolicy?.MerchantId)
                || string.IsNullOrEmpty(braintreeClientPolicy?.PublicKey) || string.IsNullOrEmpty(braintreeClientPolicy?.PrivateKey))
            {
                context.Abort(context.CommerceContext.AddMessage(
                   context.GetPolicy<KnownResultCodes>().Error,
                   "InvalidClientPolicy",
                   new object[] { "BraintreeClientPolicy" },
                    $"{this.Name}. Invalid BraintreeClientPolicy"), context);
                return arg;
            }

            try
            {
                var gateway = new BraintreeGateway(braintreeClientPolicy?.Environment, braintreeClientPolicy?.MerchantId, braintreeClientPolicy?.PublicKey, braintreeClientPolicy?.PrivateKey);

                var transaction = gateway.Transaction.Find(payment.TransactionId);
                if (transaction.Status.ToString().Equals("authorized", StringComparison.OrdinalIgnoreCase))
                {
                    arg.DateUpdated = DateTimeOffset.UtcNow;
                    var result = gateway.Transaction.SubmitForSettlement(payment.TransactionId, payment.Amount.Amount);

                    if (result.IsSuccess())
                    {
                        var settledTransaction = result.Target;
                        payment.TransactionStatus = settledTransaction.Status.ToString();

                        // Force settlement for testing
                        if (braintreeClientPolicy.Environment.Equals("sandbox", StringComparison.OrdinalIgnoreCase))
                        {
                            gateway.TestTransaction.Settle(payment.TransactionId);
                            settledTransaction = gateway.Transaction.Find(payment.TransactionId);
                            payment.TransactionStatus = settledTransaction.Status.ToString();
                        }

                        context.CommerceContext.AddUniqueObjectByType(settledTransaction);
                    }
                    else
                    {
                        var errorMessages = result.Errors.DeepAll().Aggregate(string.Empty, (current, error) => current + ("Error: " + (int)error.Code + " - " + error.Message + "\n"));
                        await this.MoveToProblemList(arg, context);

                        context.Abort(context.CommerceContext.AddMessage(
                           context.GetPolicy<KnownResultCodes>().Error,
                           "SettlePaymentFailed",
                           new object[] { payment.TransactionId },
                           $"{this.Name}. Settle payment failed for { payment.TransactionId }: { errorMessages }"), context);
                        return arg;
                    }                    
                }
                else
                {
                    context.CommerceContext.AddUniqueObjectByType(transaction);
                }               
            }
            catch (BraintreeException ex)
            {
                context.Abort(context.CommerceContext.AddMessage(
                   context.GetPolicy<KnownResultCodes>().Error,
                   "SettlePaymentFailed",
                   new object[] { payment.TransactionId },
                    $"{this.Name}. Settle payment failed for { payment.TransactionId } { ex.Message }"), context);
                return arg;
            }

            return arg; 
        }

        /// <summary>
        /// Moves to problem list.
        /// </summary>
        /// <param name="salesActivity">The sales activity.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected async virtual Task MoveToProblemList(SalesActivity salesActivity, CommercePipelineExecutionContext context)
        {
            var transientMemberships = salesActivity.GetComponent<TransientListMembershipsComponent>();
            transientMemberships.Memberships.Remove(context.GetPolicy<KnownOrderListsPolicy>().SettleSalesActivities);
            transientMemberships.Memberships.Add(context.GetPolicy<KnownOrderListsPolicy>().ProblemSalesActivities);           

            var removeArg = new ListEntitiesArgument(new List<string> { salesActivity.Id }, context.GetPolicy<KnownOrderListsPolicy>().SettleSalesActivities);
            await this._removePipeline.Run(removeArg, context);
        }
    }
}
