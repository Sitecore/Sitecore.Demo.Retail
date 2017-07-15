//-----------------------------------------------------------------------
// <copyright file="StorefrontContext.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StorefrontContext class.</summary>
//-----------------------------------------------------------------------
// Copyright 2016 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the 
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// either express or implied. See the License for the specific language governing permissions 
// and limitations under the License.
// -------------------------------------------------------------------------------------------

using System.Web;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Diagnostics;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Sites;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website
{
    [Service]
    public class StorefrontContext
    {
        public CommerceStorefront Current
        {
            get
            {
                var site = GetStorefrontSiteContext();
                if (site == null)
                {
                    Log.Debug($"The site '{Context.Site.Name}' has no commerceShopName defined", this);
                    return null;
                }

                var cacheKey = $"Storefront_{Context.Site.Name}";
                if (HttpContext.Current.Items.Contains(cacheKey))
                {
                    return HttpContext.Current.Items[cacheKey] as CommerceStorefront;
                }

                var storefront = new CommerceStorefront(site);
                HttpContext.Current.Items[cacheKey] = storefront;
                return storefront;
            }
        }

        private SiteContext GetStorefrontSiteContext()
        {
            if (Context.Site == null)
            {
                Log.Warn($"Cannot determine the Commerce ShopName. No SiteContext found", this);
                return null;
            }

            var shopName = Context.Site.CommerceShopName();
            return string.IsNullOrWhiteSpace(shopName) ? null : Context.Site;
        }
    }
}