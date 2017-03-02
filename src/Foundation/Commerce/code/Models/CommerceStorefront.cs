//-----------------------------------------------------------------------
// <copyright file="CommerceStorefront.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CommerceStorefront class.</summary>
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

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Models
{
    public class CommerceStorefront : SitecoreItemBase
    {
        public CommerceStorefront(Item item)
        {
            InnerItem = item;

            SetShopNameBySiteContext();
        }

        public Item HomeItem => InnerItem;

        public Item GlobalItem => InnerItem.Database.GetItem(Context.Site.RootPath + "/Global");

        public string SenderEmailAddress
        {
            get
            {
                var email = HomeItem.Fields[StorefrontConstants.KnownFieldNames.SenderEmailAddress];
                return email?.ToString() ?? string.Empty;
            }
        }

        public string ShopName { get; set; } = "storefront";

        public MediaItem OnSaleOverlayImage => string.IsNullOrWhiteSpace(InnerItem["On Sale Overlay Image"])  ? null : Context.Database.GetItem(new ID(InnerItem["On Sale Overlay Image"]));

        private void SetShopNameBySiteContext()
        {
            if (Context.Site == null)
            {
                Log.Warn($"Cannot determine the Commerce ShopName. No SiteContext found", this);
                return;
            }

            var shopName = Context.Site.Properties["commerceShopName"];
            if (string.IsNullOrWhiteSpace(shopName))
            {
                Log.Warn($"The site '{Context.Site.Name}' has no commerceShopName defined", this);
                return;
            }
            ShopName = shopName;
        }
    }
}