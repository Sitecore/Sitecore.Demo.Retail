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

using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Models
{
    public class CurrencyInformationModel
    {
        public CurrencyInformationModel(Item item)
        {
            this.Name = item.Name;
            this.Description = item[Templates.Currency.Fields.Description];
            this.Symbol = item[Templates.Currency.Fields.Symbol];
            this.SymbolPosition = item.GetInteger(Templates.Currency.Fields.SymbolPosition) ?? 3;
            this.CurrencyNumberFormatCulture = item[Templates.Currency.Fields.NumberFormatCulture];
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Symbol { get; set; }

        public int SymbolPosition { get; set; }

        public string CurrencyNumberFormatCulture { get; set; }
    }
}
