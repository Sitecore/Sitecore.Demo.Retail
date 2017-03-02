// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefundFederatedPaymentBlock.cs" company="Sitecore Corporation">
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
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a refind federated paymentBlock.
    /// </summary>
    /// <seealso>
    /// <cref>
    ///    Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Payments.OrderPaymentsArgument, 
    ///    Sitecore.Commerce.Plugin.Payments.OrderPaymentsArgument, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    /// </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.Pipelines.Blocks.RefundFederatedPaymentBlock)]
    public class RefundFederatedPaymentBlock : PipelineBlock<OrderPaymentsArgument, OrderPaymentsArgument, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundFederatedPaymentBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        public RefundFederatedPaymentBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            this._persistPipeline = persistEntityPipeline;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// An OrderPaymentsArgument with order and Federated payment info
        /// </returns>
        public async override Task<OrderPaymentsArgument> Run(OrderPaymentsArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull("The arg can not be null");
            Condition.Requires(arg.Order).IsNotNull("The order can not be null");          

            var order = arg.Order;          

            if (!order.Status.Equals(context.GetPolicy<KnownOrderStatusPolicy>().Completed, StringComparison.OrdinalIgnoreCase))
            {
                var invalidOrderStateMessage = $"{this.Name}: Expected order in '{context.GetPolicy<KnownOrderStatusPolicy>().Completed}' status but order was in '{order.Status}' status";
                context.Abort(context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().ValidationError,
                    "InvalidOrderState",
                    new object[] { context.GetPolicy<KnownOrderStatusPolicy>().OnHold, order.Status },
                    invalidOrderStateMessage), context);
            }

            var components = order.Components.OfType<FederatedPaymentComponent>().ToList();
            if (!components.Any())
            {
                return arg;
            }

            var braintreeClientPolicy = context.GetPolicy<BraintreeClientPolicy>();
            if (string.IsNullOrEmpty(braintreeClientPolicy?.Environment) || string.IsNullOrEmpty(braintreeClientPolicy?.MerchantId)
                || string.IsNullOrEmpty(braintreeClientPolicy?.PublicKey) || string.IsNullOrEmpty(braintreeClientPolicy?.PrivateKey))
            {
                context.CommerceContext.AddMessage(
                   context.GetPolicy<KnownResultCodes>().Error,
                   "InvalidClientPolicy",
                   new object[] { "BraintreeClientPolicy" },
                    $"{this.Name}. Invalid BraintreeClientPolicy");
                return arg;
            }

            try
            {
                var payment = arg.Payments.Where(p => !string.IsNullOrEmpty(p?.Id)).FirstOrDefault();
                if (payment == null)
                {
                    context.CommerceContext.AddMessage(
                           context.GetPolicy<KnownResultCodes>().Error,
                           "InvalidOrMissingPropertyValue",
                           new object[] { "Payment" },
                           "Invalid Payment value for refund.");
                    return arg;
                }

                var existingPayment = order.GetComponent<FederatedPaymentComponent>();
                if (existingPayment == null || !existingPayment.Id.Equals(payment.Id, StringComparison.OrdinalIgnoreCase))
                {
                    return arg;
                }
                var salesActivityPayment = order.Clone<Order>().GetComponent<FederatedPaymentComponent>();

                var gateway = new BraintreeGateway(braintreeClientPolicy?.Environment, braintreeClientPolicy?.MerchantId, braintreeClientPolicy?.PublicKey, braintreeClientPolicy?.PrivateKey);

                if (existingPayment.Amount.Amount < payment.Amount.Amount)
                {
                    context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        "IllegalRefundOperation",
                        new object[] { order.Id, existingPayment.Id },
                        "Order Federated Payment amount is less than refund amount");
                    return arg;
                }

                Result<Transaction> result = gateway.Transaction.Refund(existingPayment.TransactionId, payment.Amount.Amount);
                if (result.IsSuccess())
                {
                    context.Logger.LogInformation($"{this.Name} - Refund Payment succeeded:{payment.Id}");
                    existingPayment.TransactionStatus = result.Target.Status.ToString();
                    existingPayment.TransactionId = result.Target.Id;
                }
                else
                {
                    var errorMessages = result.Errors.DeepAll().Aggregate(string.Empty, (current, error) => current + ("Error: " + (int)error.Code + " - " + error.Message + "\n"));

                    context.Abort(context.CommerceContext.AddMessage(
                       context.GetPolicy<KnownResultCodes>().Error,
                       "PaymentRefundFailed",
                       new object[] { existingPayment.TransactionId },
                       $"{this.Name}. Payment refund failed for transaction { existingPayment.TransactionId }: { errorMessages }"), context);

                    return arg;
                }

                if (existingPayment.Amount.Amount == payment.Amount.Amount)
                {
                    order.Components.Remove(existingPayment);
                }
                else 
                {
                    //Reduce existing payment by the amount being refunded
                    existingPayment.Amount.Amount -= payment.Amount.Amount;                
                }

                salesActivityPayment.Amount.Amount = payment.Amount.Amount * -1;
                await this.GenerateSalesActivity(order, salesActivityPayment, context);

                await _persistPipeline.Run(new PersistEntityArgument(order), context.CommerceContext.GetPipelineContextOptions());
            }
            catch (BraintreeException ex)
            {
                context.CommerceContext.AddMessage(
                   context.GetPolicy<KnownResultCodes>().Error,
                   "PaymentRefundFailed",
                   new object[] { order.Id },
                    $"{this.Name}. Payment refund failed: { ex.Message }");
                return arg;
            }

            return arg;
        }

        /// <summary>
        /// Generates the sales activity.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="payment">The payment.</param>
        /// <param name="context">The context.</param>
        protected async virtual Task GenerateSalesActivity(Order order, PaymentComponent payment, CommercePipelineExecutionContext context)
        {
            var salesActivity = new SalesActivity
            {
                Id = CommerceEntity.IdPrefix<SalesActivity>() + Guid.NewGuid().ToString("N"),
                ActivityAmount = new Money(payment.Amount.CurrencyCode, payment.Amount.Amount),
                Customer = new EntityReference
                {
                    EntityTarget = order.Components.OfType<ContactComponent>().FirstOrDefault()?.CustomerId
                },
                Order = new EntityReference
                {
                    EntityTarget = order.Id
                },
                Name = "Refund the Federated Payment",
                PaymentStatus = "Completed"
            };

            salesActivity.SetComponent(new ListMembershipsComponent
            {
                Memberships = new List<string>
                    {
                        CommerceEntity.ListName<SalesActivity>(),
                        context.GetPolicy<KnownOrderListsPolicy>().SalesCredits,
                        string.Format(context.GetPolicy<KnownOrderListsPolicy>().OrderSalesActivities, order.FriendlyId)
                    }
            });

            salesActivity.SetComponent(payment);

            var salesActivities = order.SalesActivity.ToList();
            salesActivities.Add(new EntityReference { EntityTarget = salesActivity.Id });
            order.SalesActivity = salesActivities;

            await this._persistPipeline.Run(new PersistEntityArgument(salesActivity), context);
        }
    }
}
