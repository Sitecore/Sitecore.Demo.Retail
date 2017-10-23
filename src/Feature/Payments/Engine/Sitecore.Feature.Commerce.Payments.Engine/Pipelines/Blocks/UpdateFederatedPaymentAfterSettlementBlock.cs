using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Feature.Commerce.Payments.Engine.Pipelines.Blocks
{
    [PipelineDisplayName(PaymentsConstants.Pipelines.Blocks.UpdateFederatedPaymentAfterSettlementBlock)]
    public class UpdateFederatedPaymentAfterSettlementBlock : PipelineBlock<SalesActivity, SalesActivity, CommercePipelineExecutionContext>
    {
        private readonly GetOrderCommand _getOrderCommand;
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        public UpdateFederatedPaymentAfterSettlementBlock(GetOrderCommand getOrderCommand, IPersistEntityPipeline persistEntityPipeline)
        {
            this._getOrderCommand = getOrderCommand;
            this._persistEntityPipeline = persistEntityPipeline;
        }

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