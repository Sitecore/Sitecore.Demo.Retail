using System;
using Sitecore;
using Sitecore.Sites;

namespace Foundation.Commerce.Website.Extensions
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