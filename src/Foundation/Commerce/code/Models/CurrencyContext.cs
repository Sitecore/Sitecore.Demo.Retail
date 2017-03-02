using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Models
{
    public class CurrencyContext
    {
        private Item _contextItem;

        public string CurrencyCode
        {
            get
            {
                var currencyItem = ContextItem?.TargetItem(Templates.CurrencyContext.Fields.DefaultCurrency);
                if (currencyItem == null)
                {
                    throw new ConfigurationErrorsException("Default currency not set on the store");
                }

                return currencyItem.Name;
            }
        }

        private Item ContextItem
        {
            get
            {
                if (_contextItem == null)
                {
                    _contextItem = Sitecore.Context.Item?.GetAncestorOrSelfOfTemplate(Templates.CurrencyContext.ID);
                    if (_contextItem == null)
                    {
                        _contextItem = Sitecore.Context.Site?.GetStartItem()?.GetAncestorOrSelfOfTemplate(Templates.CurrencyContext.ID);
                    }
                }

                if (_contextItem == null)
                    throw new ConfigurationErrorsException("Cannot determine the CurrencyContext for the commerce storefront");
                return _contextItem;
            }
        }
    }
}