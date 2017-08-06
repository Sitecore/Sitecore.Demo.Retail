using System.Configuration;
using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Website.Models
{
    public class CurrencyContext
    {
        private Item _contextItem;

        public string CurrencyCode
        {
            get
            {
                var currency = ContextItem[Templates.CurrencyContext.Fields.DefaultCurrency];
                if (string.IsNullOrWhiteSpace(currency))
                {
                    throw new ConfigurationErrorsException("Default currency not set on the store");
                }

                return currency;
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
