using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Sites;

namespace Sitecore.Foundation.Commerce.Extensions
{
    public static class SiteContextExtensions
    {
        public static string CommerceShopName(this SiteContext siteContext)
        {
            if (siteContext == null)
            {
                throw new ArgumentNullException(nameof(siteContext));
            }
            return Context.Site.Properties["commerceShopName"];
        }
    }
}