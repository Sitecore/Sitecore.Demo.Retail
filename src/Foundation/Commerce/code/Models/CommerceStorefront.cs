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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce.Managers;

namespace Sitecore.Foundation.Commerce.Models
{
    public class CommerceStorefront : SitecoreItemBase
    {
        private string _shopName = "website";

        public CommerceStorefront()
        {
        }

        public CommerceStorefront(Item item)
        {
            InnerItem = item;

            if (!string.IsNullOrWhiteSpace(Context.Site.Name))
            {
                _shopName = Context.Site.Name;
            }
        }

        public virtual Item RootItem => Context.Database.GetItem(Context.Site.RootPath);

        public virtual Item HomeItem => InnerItem;

        public virtual Item GlobalItem => Context.Database.GetItem(Context.Site.RootPath + "/Global");

        public virtual string SenderEmailAddress
        {
            get
            {
                var email = HomeItem.Fields[StorefrontConstants.KnownFieldNames.SenderEmailAddress];
                return email?.ToString() ?? string.Empty;
            }
        }

        public virtual bool UseIndexFileForProductStatusInLists => MainUtil.GetBool(HomeItem[StorefrontConstants.KnownFieldNames.UseIndexFileForProductStatusInLists],
            false);

        public virtual bool FormsAuthentication => MainUtil.GetBool(HomeItem[StorefrontConstants.KnownFieldNames.FormsAuthentication], false);

        public virtual bool IsAXSite => !string.IsNullOrEmpty(HomeItem[StorefrontConstants.KnownFieldNames.OperatingUnitNumber]);

        public virtual string ShopName
        {
            get { return _shopName; }
            set { _shopName = value; }
        }

        public virtual string DefaultCartName { get; set; } = CommerceConstants.CartSettings.DefaultCartName;

        public virtual Catalog DefaultCatalog
        {
            get
            {
                //If this is not in a storefront then return the default catalog
                if (InnerItem == null)
                {
                    var defaultCatalogContainer =
                        Context.Database.GetItem(CommerceConstants.KnownItemIds.DefaultCatalog);
                    var catalogPath = defaultCatalogContainer.Fields["Catalog"].Source + "/" +
                                      defaultCatalogContainer.Fields["Catalog"].Value;
                    var defaultCatalog = Context.Database.GetItem(catalogPath);
                    return new Catalog(defaultCatalog);
                }

                //Get list of all related catalogs
                var catalogs = InnerItem.Fields["Catalogs"].Value;
                var catalogArray = catalogs.Split("|".ToCharArray());
                if (catalogArray.Length == 0)
                {
                    return null;
                }

                //Return first catalog
                var catalogItem = Context.Database.GetItem(catalogArray[0]);
                return new Catalog(catalogItem);
            }
        }

        public virtual string GiftCardProductId
        {
            get { return InnerItem == null ? "22565422120" : InnerItem["GiftCardProductId"]; }
        }

        public virtual string GiftCardAmountOptions
        {
            get
            {
                return InnerItem == null ? "10|20|25|50|100" : InnerItem["GiftCardAmountOptions"];
            }
        }

        public virtual string DefaultProductId => InnerItem == null ? "22565422120" : InnerItem["DefaultProductId"];

        public virtual bool SupportsWishLists => true;

        public virtual bool SupportsLoyaltyPrograms => true;

        public virtual bool SupportsGiftCardPayment => true;

        public virtual int MaxNumberOfAddresses
        {
            get { return MainUtil.GetInt(HomeItem[StorefrontConstants.KnownFieldNames.MaxNumberOfAddresses], 10); }
        }

        public virtual int MaxNumberOfWishLists => MainUtil.GetInt(HomeItem[StorefrontConstants.KnownFieldNames.MaxNumberOfWishLists], 10);

        public virtual int MaxNumberOfWishListItems => MainUtil.GetInt(HomeItem[StorefrontConstants.KnownFieldNames.MaxNumberOfWishListItems], 10);

        public virtual string DefaultCurrency
        {
            get
            {
                var linkedCurrency = HomeItem[StorefrontConstants.KnownFieldNames.DefaultCurrency];
                if (!string.IsNullOrWhiteSpace(linkedCurrency))
                {
                    return Context.Database.GetItem(ID.Parse(linkedCurrency)).Name;
                }

                throw new ConfigurationErrorsException(StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.DefaultCurrencyNotSetException));
            }
        }

        public virtual List<string> SupportedCurrencies
        {
            get
            {
                var returnValues = new List<string>();

                MultilistField supportedCurrenciesList =
                    HomeItem.Fields[StorefrontConstants.KnownFieldNames.SupportedCurrencies];
                if (supportedCurrenciesList != null)
                {
                    returnValues.AddRange(
                        supportedCurrenciesList.TargetIDs.Select(id => HomeItem.Database.GetItem(id).Name));
                }

                return returnValues;
            }
        }

        public virtual bool IsSupportedCurrency(string currency)
        {
            return SupportedCurrencies.Exists(x => x.Equals(currency, StringComparison.OrdinalIgnoreCase));
        }

        public virtual string Title()
        {
            return InnerItem == null ? "default" : InnerItem[StorefrontConstants.ItemFields.Title];
        }

        public virtual string NameTitle()
        {
            return InnerItem == null ? "default" : InnerItem["Name Title"];
        }

        public virtual string GetMapKey()
        {
            return HomeItem[StorefrontConstants.KnownFieldNames.MapKey];
        }
    }
}