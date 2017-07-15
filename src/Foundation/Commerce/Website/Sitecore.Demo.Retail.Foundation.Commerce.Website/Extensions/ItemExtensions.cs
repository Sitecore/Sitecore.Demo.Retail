using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsWildcardItem(this Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            return item.Name == "*";
        }
    }
}