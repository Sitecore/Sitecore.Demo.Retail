﻿//-----------------------------------------------------------------------
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

namespace Sitecore.Reference.Storefront.Models
{
    using System.Globalization;

    /// <summary>
    /// View model for a Variant entity.
    /// </summary>
    public class VariantViewModel
    {
        private readonly Sitecore.Data.Items.Item _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariantViewModel"/> class.
        /// </summary>
        public VariantViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariantViewModel"/> class.
        /// </summary>
        /// <param name="item">The sitecore item for the related page</param>
        public VariantViewModel(Sitecore.Data.Items.Item item)
        {
            this._item = item;
        }

        /// <summary>
        /// Gets the Variant Id.
        /// </summary>
        public string Id
        {
            get
            {
                if (this._item != null)
                {
                    return this._item.Name;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the Variant DisplayName.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (this._item != null)
                {
                    return this._item.DisplayName;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the related Product Id.
        /// </summary>
        public string ProductId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Variant ListPrice.
        /// </summary>
        public decimal? ListPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the adjusted price.
        /// </summary>        
        public decimal? AdjustedPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on sale.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is on sale; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnSale
        {
            get
            {
                return (this.AdjustedPrice.HasValue && this.ListPrice.HasValue && this.AdjustedPrice < this.ListPrice);
            }
        }

        /// <summary>
        /// Gets the percentage savings for the product.
        /// </summary>
        public decimal SavingsPercentage
        {
            get
            {
                if (!this.ListPrice.HasValue || !this.AdjustedPrice.HasValue || this.ListPrice.Value <= this.AdjustedPrice.Value)
                {
                    return 0;
                }

                var percentage = decimal.Floor(100 * (this.ListPrice.Value - this.AdjustedPrice.Value) / this.ListPrice.Value);
                int integerPart = (int)percentage;
                return integerPart == 0 ? 1M : (decimal)integerPart;
            }
        }

        /// <summary>
        /// Gets the Variant Color.
        /// </summary>
        public string ProductColor
        {
            get
            {
                if (this._item != null)
                {
                    return this._item["ProductColor"];
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the Variant Size.
        /// </summary>
        public string Size
        {
            get
            {
                if (this._item != null)
                {
                    return this._item["ProductSize"];
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the VariantId.
        /// </summary>
        public string VariantId
        {
            get
            {
                if (this._item != null)
                {
                    return this._item.Name;
                }

                return null;
            }
        }
    }
}
