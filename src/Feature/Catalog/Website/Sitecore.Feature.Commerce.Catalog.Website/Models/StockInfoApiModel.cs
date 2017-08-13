//-----------------------------------------------------------------------
// <copyright file="StockInfoBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StockInfoBaseJsonResult class.</summary>
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

using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Catalog.Website.Models
{
    public class StockInfoApiModel : BaseApiModel
    {
        public StockInfoApiModel()
        {
        }

        public StockInfoApiModel(ServiceProviderResult result)
            : base(result)
        {
        }

        public string ProductId { get; set; }

        public string VariantId { get; set; }

        public string Status { get; set; }

        public string AvailabilityDate { get; set; }

        public double Count { get; set; }

        public bool CanShowSignupForNotification { get; set; }

        public void Initialize(StockInformation stockInfo)
        {
            Assert.ArgumentNotNull(stockInfo, nameof(stockInfo));

            if (stockInfo == null || stockInfo.Status == null)
            {
                return;
            }

            ProductId = stockInfo.Product.ProductId;
            VariantId = string.IsNullOrEmpty(((CommerceInventoryProduct) stockInfo.Product).VariantId) ? string.Empty : ((CommerceInventoryProduct) stockInfo.Product).VariantId;
            Status = InventoryManager.GetStockStatusName(stockInfo.Status);
            Count = stockInfo.Count < 0 ? 0 : stockInfo.Count;
            CanShowSignupForNotification = Context.User.IsAuthenticated;
            if ((stockInfo.AvailabilityDate != null) & stockInfo.AvailabilityDate.HasValue)
            {
                AvailabilityDate = stockInfo.AvailabilityDate.Value.ToDisplayedDate();
            }
        }
    }
}