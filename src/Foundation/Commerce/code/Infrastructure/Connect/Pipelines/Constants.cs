using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Foundation.Commerce.Connect.Pipelines
{
    public class Constants {
        public static class Pipelines
        {
            public const string GetAvailableRegions = "commerce.orders.getAvailableRegions";
            public const string GetAvailableCountries = "commerce.orders.getAvailableCountries";
            public const string TranslateEntityToCommerceAddressProfile = "translate.entityToCommerceAddressProfile";
            public const string TranslateCommerceAddressProfileToEntity = "translate.commerceAddressProfileToEntity";
        }
    }
}