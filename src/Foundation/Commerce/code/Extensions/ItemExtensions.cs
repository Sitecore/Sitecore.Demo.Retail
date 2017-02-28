using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.Commerce.Extensions
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