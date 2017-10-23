//-----------------------------------------------------------------------
// <copyright file="GetProductBulkPricesRequest.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The get product bulk prices request.</summary>
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
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Arguments
{
    public class GetProductBulkPricesRequest : Sitecore.Commerce.Services.Prices.GetProductBulkPricesRequest
    {
        public GetProductBulkPricesRequest(string catalogName, IEnumerable<string> productIds, params string[] priceTypeIds) : base(productIds)
        {
            Assert.ArgumentNotNull(catalogName, nameof(catalogName));
            ProductCatalogName = catalogName;
            PriceTypeIds = priceTypeIds;
        }
        public string ProductCatalogName { get; set; }
        public IEnumerable<string> PriceTypeIds { get; protected set; }
    }
}