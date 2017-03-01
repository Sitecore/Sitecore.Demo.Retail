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

using System;
using System.Web;
using System.Web.Mvc;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Links;

namespace Sitecore.Foundation.Commerce.Managers
{
    [Obsolete("Please refactor")]
    public static class LookupManager
    {
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

        public static string Lookup(string tableName, string itemName, out Item lookupItem, bool insertBracketsWhenNotFound)
        {
            Assert.ArgumentNotNullOrEmpty(tableName, nameof(tableName));

            lookupItem = null;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                return string.Empty;
            }

            var item = StorefrontManager.CurrentStorefront.GlobalItem.Axes.GetItem(string.Concat(StorefrontConstants.KnowItemNames.Lookups, "/", tableName, "/", itemName));
            if (item == null)
            {
                return insertBracketsWhenNotFound ? $"[{itemName}]" : itemName;
            }

            lookupItem = item;
            return item[StorefrontConstants.KnownFieldNames.Value];
        }
    }
}