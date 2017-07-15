//-----------------------------------------------------------------------
// <copyright file="CurrencyInformationModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Model used to return currency information.</summary>
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
using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public class CurrencyInformationModel
    {
        public CurrencyInformationModel(Item item)
        {
            if (!item.IsDerived(Sitecore.Commerce.Constants.Templates.Currency.ID))
                throw new ArgumentException("Invalid items type, must be a CurrenciesDisplayAdjustments item", nameof(item));
            this.Name = item.Name;
            this.Description = item[Sitecore.Commerce.Constants.Templates.Currency.Fields.CurrencyDescription];
            this.Symbol = item[Sitecore.Commerce.Constants.Templates.Currency.Fields.CurrencySymbol];
            this.SymbolPosition = item.GetInteger(Sitecore.Commerce.Constants.Templates.Currency.Fields.CurrencySymbolPosition) ?? 3;
            this.CurrencyNumberFormatCulture = item[Sitecore.Commerce.Constants.Templates.Currency.Fields.CurrencyNumberFormat];
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Symbol { get; set; }

        public int SymbolPosition { get; set; }

        public string CurrencyNumberFormatCulture { get; set; }
    }
}
