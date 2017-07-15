namespace Sitecore.Demo.Retail.Feature.Payments.Engine
{
    public static class PaymentsConstants
    {
        public static class Pipelines
        {
            /// <summary>
            /// The name of the payment pipelines blocks.
            /// </summary>
            public static class Blocks
            {
                public const string GetClientTokenBlock = "Payments.block.getclienttoken";
                public const string UpdateFederatedPaymentBlock = "Payments.block.updatefederatedpayment";
                public const string UpdateFederatedPaymentAfterSettlementBlock = "Payments.block.updatefederatedpaymentaftersettlement";
                public const string CreateFederatedPaymentBlock = "Payments.block.createfederatedpayment";
                public const string EnsureSettlePaymentRequestedBlock = "Payments.block.ensuresettlepaymentrequested";
                public const string VoidFederatedPaymentBlock = "Payments.block.voidfederatedpayment";
                public const string VoidCancelOrderFederatedPaymentBlock = "Payments.block.voidcancelorderfederatedpayment";
                public const string RefundFederatedPaymentBlock = "Payments.block.refundfederatedpayment";
                public const string VoidOnHoldOrderFederatedPaymentBlock = "Payments.block.voidonholdorderfederatedpayment";
                public const string ValidateSettlementBlock = "Payments.block.validatesettlement";
            }
        }
    }
}
