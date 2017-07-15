using Sitecore.Commerce.Core;

namespace Sitecore.Demo.Retail.Feature.Payments.Engine.Policies
{
    public class BraintreeClientPolicy : Policy
    {
        public BraintreeClientPolicy()
        {
            this.Environment = string.Empty;
            this.MerchantId = string.Empty;
            this.PublicKey = string.Empty;
            this.PrivateKey = string.Empty;
        }

        public string Environment { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
