//-----------------------------------------------------------------------
// <copyright file="StorefrontContext.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StorefrontContext class.</summary>
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

using System.Linq;
using System.Web;
using Sitecore.Commerce.Multishop;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Website.Managers
{
    public class CurrencyManager : IManager
    {
        public CurrencyContext CurrencyContext
        {
            get
            {
                var context = (CurrencyContext) HttpContext.Current.Items["_currencyContext"];
                if (context != null)
                {
                    return context;
                }
                context = new CurrencyContext();
                HttpContext.Current.Items["_currencyContext"] = context;
                return context;
            }
        }

        public static CurrencyInformationModel GetCurrencyInformation(string currency)
        {
            var currencyConfigurations = ConnectStorefrontContext.Current?.StorefrontConfiguration?.Children.FirstOrDefault(i => i.IsDerived(Sitecore.Commerce.Constants.Templates.CurrenciesDisplayAdjustments.ID));
            var currencyConfiguration = currencyConfigurations?.Children[currency];
            return currencyConfiguration != null ? new CurrencyInformationModel(currencyConfiguration) : null;
        }
    }
}