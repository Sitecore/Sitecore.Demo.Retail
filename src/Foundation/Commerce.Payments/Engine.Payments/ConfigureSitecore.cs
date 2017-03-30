using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Foundation.Commerce.Engine.Plugin.Payments.Pipelines.Blocks;

namespace Sitecore.Foundation.Commerce.Engine.Plugin.Payments
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IGetClientTokenPipeline>(builder => builder
                    .Add<Pipelines.Blocks.GetClientTokenBlock>().After<Sitecore.Commerce.Plugin.Payments.GetClientTokenBlock>()
                )

                .ConfigurePipeline<ICreateOrderPipeline>(builder => builder
                    .Add<CreateFederatedPaymentBlock>().Before<CreateOrderBlock>()
                )

                .ConfigurePipeline<IReleaseOnHoldOrderPipeline>(d =>
                {
                    d.Add<UpdateFederatedPaymentBlock>().After<ValidateOnHoldOrderBlock>();
                })

                .ConfigurePipeline<ISettleSalesActivityPipeline>(d =>
                {
                    d.Add<EnsureSettlePaymentRequestedBlock>().After<ValidateSalesActivityBlock>()
                     .Add<ValidateSettlementBlock>().After<EnsureSettlePaymentRequestedBlock>()
                     .Add<UpdateFederatedPaymentAfterSettlementBlock>().Before<PersistSalesActivityBlock>();
                })

                .ConfigurePipeline<IRefundPaymentsPipeline>(d =>
                {
                    d.Add<RefundFederatedPaymentBlock>().After<RefundCreditCardPaymentBlock>();
                })

                .ConfigurePipeline<ICancelOrderPipeline>(d =>
                {
                    d.Add<VoidCancelOrderFederatedPaymentBlock>().After<GetPendingOrderBlock>();
                })
            );
        }
    }
}
