//---------------------------------------------------------------------
// <copyright file="SimpleTypesExtensions.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Extensions for dealing with Commerce Server entities.</summary>
//---------------------------------------------------------------------
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
using System.Globalization;
using System.Web.Mvc;
using Sitecore.Foundation.Commerce.Website.Managers;

namespace Sitecore.Foundation.Commerce.Website.Extensions
{
    public static class SimpleTypesExtensions
    {
        public static string ToCurrency(this decimal? currency, string currencyCode)
        {
            return currency.HasValue ? currency.Value.ToCurrency(currencyCode) : 0M.ToCurrency(currencyCode);
        }

        public static string ToCurrency(this decimal currency, string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                return currency.ToCurrency();

            var currencyInfo = CurrencyManager.GetCurrencyInformation(currencyCode);

            NumberFormatInfo info;
            if (currencyInfo != null)
            {
                info = (NumberFormatInfo) CultureInfo.GetCultureInfo(currencyInfo.CurrencyNumberFormatCulture).NumberFormat.Clone();
                info.CurrencySymbol = currencyInfo.Symbol;
                info.CurrencyPositivePattern = currencyInfo.SymbolPosition;
            }
            else
            {
                info = NumberFormatInfo.CurrentInfo;
            }
            return currency.ToString("C", info);
        }

        public static string ToDisplayedDate(this DateTime date)
        {
            return date.ToString("d", Context.Language.CultureInfo);
        }

        public static string ToCurrency(this decimal currency)
        {
            var currencyManager = DependencyResolver.Current.GetService<CurrencyManager>();
            return currency.ToCurrency(currencyManager.CurrencyContext.CurrencyCode);
        }
        public static string ToCurrency(this decimal? currency)
        {
            return currency.HasValue ? currency.Value.ToCurrency() : 0M.ToCurrency();
        }
    }
}