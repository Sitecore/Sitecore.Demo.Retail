using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine.Plugin.Entitlements
{
    public static class EntitlementsConstants
    {
        public static class Pipelines
        {
            public static class Blocks
            {
                public const string ProvisionWarrantyEntitlementsBlock = "Entitlements.block.ProvisionWarrantyEntitlements";
                public const string ProvisionInstallationEntitlementsBlock = "Entitlements.block.ProvisionInstallationEntitlements";
                public const string ProvisionDigitalProductEntitlementsBlock = "Entitlements.block.ProvisionDigitalProductEntitlements";
                public const string GetOrderWarrantyEntitlementDetailsViewBlock = "Entitlements.block.GetOrderWarrantyEntitlementDetailsView";
                public const string GetCustomerWarrantyEntitlementDetailsViewBlock = "Entitlements.block.GetCustomerWarrantyEntitlementDetailsView";
                public const string GetOrderInstallationEntitlementDetailsViewBlock = "Entitlements.block.GetOrderInstallationEntitlementDetailsView";
                public const string GetCustomerInstallationEntitlementDetailsViewBlock = "Entitlements.block.GetCustomerInstallationEntitlementDetailsView";
                public const string GetOrderDigitalProductEntitlementDetailsViewBlock = "Entitlements.block.GetOrderDigitalProductEntitlementDetailsView";
                public const string GetCustomerDigitalProductEntitlementDetailsViewBlock = "Entitlements.block.GetCustomerDigitalProductEntitlementDetailsView";
            }
        }
    }
}
