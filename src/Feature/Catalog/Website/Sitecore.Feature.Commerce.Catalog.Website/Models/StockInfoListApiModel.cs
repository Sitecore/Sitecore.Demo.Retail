//-----------------------------------------------------------------------
// <copyright file="StockInfoListBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StockInfoListBaseJsonResult class.</summary>
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
using System.Linq;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Catalog.Website.Models
{
    public class StockInfoListApiModel : BaseApiModel
    {
        public StockInfoListApiModel()
        {
        }

        public StockInfoListApiModel(ServiceProviderResult result)
            : base(result)
        {
        }

        public List<StockInfoApiModel> StockInformations { get; } = new List<StockInfoApiModel>();

        public void Initialize(IEnumerable<StockInformation> stockInformations)
        {
            Assert.ArgumentNotNull(stockInformations, nameof(stockInformations));

            var stockInfos = stockInformations as IList<StockInformation> ?? stockInformations.ToList();
            if (!stockInfos.Any())
            {
                return;
            }

            foreach (var info in stockInfos)
            {
                var stockInfo = new StockInfoApiModel();
                stockInfo.Initialize(info);
                StockInformations.Add(stockInfo);
            }
        }
    }
}