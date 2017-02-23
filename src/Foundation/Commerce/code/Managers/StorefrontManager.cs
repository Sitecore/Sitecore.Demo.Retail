//-----------------------------------------------------------------------
// <copyright file="StorefrontManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StorefrontManager class.</summary>
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
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;

namespace Sitecore.Foundation.Commerce.Managers
{
    public static class StorefrontManager
    {
        public static CommerceStorefront CurrentStorefront
        {
            get
            {
                var siteContext = DependencyResolver.Current.GetService<SiteContextRepository>().GetCurrent();
                var path = Context.Site.RootPath + Context.Site.StartItem;
                if (siteContext.CurrentContext.Items.Contains(path))
                {
                    return siteContext.CurrentContext.Items[path] as CommerceStorefront;
                }

                var storefront = CommerceTypeLoader.CreateInstance<CommerceStorefront>(Context.Database.GetItem(path));
                siteContext.CurrentContext.Items[path] = storefront;
                return (CommerceStorefront) siteContext.CurrentContext.Items[path];
            }
        }

        public static Item CommerceItem => Context.Database.GetItem("/sitecore/Commerce");

        public static Item StorefrontConfigurationItem => Context.Database.GetItem("/sitecore/Commerce/Storefront Configuration");

        public static HtmlString GetHtmlSystemMessage(string messageKey)
        {
            return new HtmlString(GetSystemMessage(messageKey));
        }

        public static string GetSystemMessage(string messageKey, bool insertBracketsWhenNotFound = true)
        {
            Item lookupItem = null;

            return Lookup(StorefrontConstants.KnowItemNames.SystemMessages, messageKey, out lookupItem, insertBracketsWhenNotFound);
        }

        public static string GetProductStockStatusName(StockStatus status)
        {
            if (status == null)
            {
                return string.Empty;
            }

            Item lookupItem = null;

            return Lookup(StorefrontConstants.KnowItemNames.InventoryStatuses, status.Name, out lookupItem, true);
        }

        public static string GetRelationshipName(string name, out Item lookupItem)
        {
            return Lookup(StorefrontConstants.KnowItemNames.Relationships, name, out lookupItem, true);
        }

        public static string GetOrderStatusName(string status)
        {
            if (status == null)
            {
                return string.Empty;
            }

            Item lookupItem = null;

            return Lookup(StorefrontConstants.KnowItemNames.OrderStatuses, status, out lookupItem, true);
        }

        public static string GetPaymentName(string payment)
        {
            if (payment == null)
            {
                return string.Empty;
            }

            Item lookupItem = null;

            return Lookup(StorefrontConstants.KnowItemNames.Payments, payment, out lookupItem, true);
        }

        public static string GetShippingName(string shipping)
        {
            if (shipping == null)
            {
                return string.Empty;
            }

            Item lookupItem = null;

            return Lookup(StorefrontConstants.KnowItemNames.Shipping, shipping, out lookupItem, true);
        }

        public static CurrencyInformationModel GetCurrencyInformation(string currency)
        {
            var displayKey = $"{currency}_{Context.Language.Name}";
            var item = StorefrontConfigurationItem.Axes.GetItem(string.Concat(StorefrontConstants.KnowItemNames.CurrencyDisplay, "/", displayKey));
            if (item != null)
            {
                return new CurrencyInformationModel(item);
            }

            item = StorefrontConfigurationItem.Axes.GetItem(string.Concat(StorefrontConstants.KnowItemNames.Currencies, "/", currency));
            if (item != null)
            {
                return new CurrencyInformationModel(item);
            }

            return null;
        }

        public static string Lookup(string tableName, string itemName, out Item lookupItem, bool insertBracketsWhenNotFound)
        {
            Assert.ArgumentNotNullOrEmpty(tableName, nameof(tableName));

            lookupItem = null;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                return string.Empty;
            }

            var item = CurrentStorefront.GlobalItem.Axes.GetItem(string.Concat(StorefrontConstants.KnowItemNames.Lookups, "/", tableName, "/", itemName));
            if (item == null)
            {
                return insertBracketsWhenNotFound ? $"[{itemName}]" : itemName;
            }

            lookupItem = item;
            return item[StorefrontConstants.KnownFieldNames.Value];
        }
    }
}