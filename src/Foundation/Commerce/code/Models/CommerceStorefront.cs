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

using System.Configuration;
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

        public bool UseIndexFileForProductStatusInLists => MainUtil.GetBool(HomeItem[StorefrontConstants.KnownFieldNames.UseIndexFileForProductStatusInLists], false);

        public string ShopName { get; set; } = "storefront";

        public string DefaultProductId => InnerItem == null ? "22565422120" : InnerItem["DefaultProductId"];

        public bool SupportsWishLists => MainUtil.GetBool(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.SupportsWishLists], false);

        public bool SupportsLoyaltyPrograms => MainUtil.GetBool(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.SupportsLoyaltyProgram], false);

        public bool SupportsGiftCardPayment => MainUtil.GetBool(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.SupportsGirstCardPayment], false);

        public int MaxNumberOfAddresses => MainUtil.GetInt(HomeItem[StorefrontConstants.KnownFieldNames.MaxNumberOfAddresses], 10);

        public int MaxNumberOfWishLists => MainUtil.GetInt(HomeItem[StorefrontConstants.KnownFieldNames.MaxNumberOfWishLists], 10);

        public int MaxNumberOfWishListItems => MainUtil.GetInt(HomeItem[StorefrontConstants.KnownFieldNames.MaxNumberOfWishListItems], 10);

        public string DefaultCurrency
        {
            get
            {
                var currencyItem = CurrencyContextItem?.TargetItem(Templates.CurrencyContext.Fields.DefaultCurrency);
                if (currencyItem == null)
                {
                    throw new ConfigurationErrorsException("Default currency not set on the store");
                }

                return currencyItem.Name;
            }
        }

        private Item CurrencyContextItem => InnerItem.GetAncestorOrSelfOfTemplate(Templates.CurrencyContext.ID);

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

        public string Title()
        {
            return InnerItem == null ? "default" : InnerItem[StorefrontConstants.ItemFields.Title];
        }

        public string GetMapKey()
        {
            return HomeItem[StorefrontConstants.KnownFieldNames.MapKey];
        }
    }
}