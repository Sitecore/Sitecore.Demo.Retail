using System;
using Sitecore.Sites;

namespace Sitecore.Foundation.Commerce.Website.Extensions
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