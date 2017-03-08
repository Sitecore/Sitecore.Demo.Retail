// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateFederatedPaymentAfterSettlementBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Payments.Braintree
{
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which updates transaction Id for an order.
    /// </summary>   
    /// <seealso>
    ///   <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Orders.SalesActivity,
    ///         Sitecore.Commerce.Plugin.Orders.SalesActivity, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///   </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.Pipelines.Blocks.UpdateFederatedPaymentAfterSettlementBlock)]
    public class UpdateFederatedPaymentAfterSettlementBlock : PipelineBlock<SalesActivity, SalesActivity, CommercePipelineExecutionContext>
    {
        private readonly GetOrderCommand _getOrderCommand;
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFederatedPaymentBlock" /> class.
        /// </summary>
        /// <param name="getOrderCommand">The get order command.</param>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        public UpdateFederatedPaymentAfterSettlementBlock(GetOrderCommand getOrderCommand, IPersistEntityPipeline persistEntityPipeline)
        {
            this._getOrderCommand = getOrderCommand;
            this._persistEntityPipeline = persistEntityPipeline;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A cart with federate payment component
        /// </returns>
        public async override Task<SalesActivity> Run(SalesActivity arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: sales activity can not be null.");
            if (!arg.HasComponent<FederatedPaymentComponent>())
            {
                return arg;
            }

            var order = await this._getOrderCommand.Process(context.CommerceContext, arg.Order.EntityTarget);

            if (order == null || !order.HasComponent<FederatedPaymentComponent>())
            {
                return arg;
            }

            var orderPayment = order.GetComponent<FederatedPaymentComponent>();

            var payment = arg.GetComponent<FederatedPaymentComponent>();
            orderPayment.TransactionStatus = payment.TransactionStatus;

            await this._persistEntityPipeline.Run(new PersistEntityArgument(order), context);
            return arg;
        }
    }
}