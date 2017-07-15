//-----------------------------------------------------------------------
// <copyright file="VariantViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
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

using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public class VariantViewModel : ICatalogProductVariant
    {
        private readonly Item _item;

        public VariantViewModel()
        {
        }

        public VariantViewModel(Item item)
        {
            _item = item;
        }

        public string Id => _item?.Name;

        public string Title => _item != null ? _item.DisplayName : string.Empty;

        public string ProductId { get; set; }

        public bool IsOnSale => AdjustedPrice.HasValue && ListPrice.HasValue && AdjustedPrice < ListPrice;

        public decimal SavingsPercentage
        {
            get
            {
                if (!ListPrice.HasValue || !AdjustedPrice.HasValue || ListPrice.Value <= AdjustedPrice.Value)
                    return 0;

                var percentage = decimal.Floor(100 * (ListPrice.Value - AdjustedPrice.Value) / ListPrice.Value);
                var integerPart = (int) percentage;
                return integerPart == 0 ? 1M : integerPart;
            }
        }

        public string ProductColor => _item != null ? _item["ProductColor"] : string.Empty;

        public string Size => _item != null ? _item["ProductSize"] : string.Empty;

        public decimal? ListPrice { get; set; }

        public decimal? AdjustedPrice { get; set; }

        public string VariantId => _item?.Name;
    }
}