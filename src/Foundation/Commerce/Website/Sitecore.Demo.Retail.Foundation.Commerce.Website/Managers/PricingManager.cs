//-----------------------------------------------------------------------
// <copyright file="PricingManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the pricing business logic for the site.</summary>
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
using System.Collections.Generic;
using Sitecore.Commerce.Entities.Prices;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Prices;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Diagnostics;
using Sitecore.Web;
using GetProductBulkPricesRequest = Sitecore.Commerce.Engine.Connect.Services.Prices.GetProductBulkPricesRequest;
using GetProductPricesRequest = Sitecore.Commerce.Engine.Connect.Services.Prices.GetProductPricesRequest;
using GetSupportedCurrenciesRequest = Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Arguments.GetSupportedCurrenciesRequest;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
    public class PricingManager : IManager
    {
        private static readonly string[] _defaultPriceTypeIds = {PriceTypes.List, PriceTypes.Adjusted, PriceTypes.LowestPricedVariant, PriceTypes.LowestPricedVariantListPrice, PriceTypes.HighestPricedVariant};

        public PricingManager(PricingServiceProvider pricingServiceProvider, CurrencyManager currencyManager, StorefrontContext storefrontContext)
        {
            Assert.ArgumentNotNull(pricingServiceProvider, nameof(pricingServiceProvider));

            PricingServiceProvider = pricingServiceProvider;
            CurrencyManager = currencyManager;
            StorefrontContext = storefrontContext;
        }

        private PricingServiceProvider PricingServiceProvider { get; }
        private CurrencyManager CurrencyManager { get; }
        private StorefrontContext StorefrontContext { get; }


        public ManagerResponse<GetProductPricesResult, IDictionary<string, Price>> GetProductPrices(string catalogName, string productId, bool includeVariants, string userId, params string[] priceTypeIds)
        {
            if (priceTypeIds == null)
            {
                priceTypeIds = _defaultPriceTypeIds;
            }

            var request = new GetProductPricesRequest(catalogName, productId, priceTypeIds)
            {
                DateTime = GetCurrentDate()
            };

            request.UserId = userId;
            request.IncludeVariantPrices = includeVariants;
            request.CurrencyCode = CurrencyManager.CurrencyContext.CurrencyCode;
            var result = PricingServiceProvider.GetProductPrices(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetProductPricesResult, IDictionary<string, Price>>(result, result.Prices ?? new Dictionary<string, Price>());
        }

        public ManagerResponse<GetProductBulkPricesResult, IDictionary<string, Price>> GetProductBulkPrices(string catalogName, IEnumerable<string> productIds, params string[] priceTypeIds)
        {
            Assert.ArgumentNotNull(catalogName, nameof(catalogName));
            Assert.ArgumentNotNull(productIds, nameof(productIds));

            if (priceTypeIds == null)
            {
                priceTypeIds = _defaultPriceTypeIds;
            }

            var request = new GetProductBulkPricesRequest(catalogName, productIds, priceTypeIds)
            {
                CurrencyCode = CurrencyManager.CurrencyContext.CurrencyCode,
                DateTime = GetCurrentDate()
            };

            var result = PricingServiceProvider.GetProductBulkPrices(request);

            // Currently, both Categories and Products are passed in and are waiting for a fix to filter the categories out. Until then, this code is commented
            // out as it generates an unecessary Error event indicating the product cannot be found.
            //result.WriteToSitecoreLog();
            return new ManagerResponse<GetProductBulkPricesResult, IDictionary<string, Price>>(result, result.Prices ?? new Dictionary<string, Price>());
        }

        public ManagerResponse<GetSupportedCurrenciesResult, IReadOnlyCollection<string>> GetSupportedCurrencies(string catalogName)
        {
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new GetSupportedCurrenciesRequest(StorefrontContext.Current.ShopName, catalogName);
            var result = PricingServiceProvider.GetSupportedCurrencies(request);

            return new ManagerResponse<GetSupportedCurrenciesResult, IReadOnlyCollection<string>>(result, result.Currencies);
        }

        public ManagerResponse<ServiceProviderResult, bool> CurrencyChosenPageEvent(string currency)
        {
            Assert.ArgumentNotNullOrEmpty(currency, nameof(currency));

            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new CurrencyChosenRequest(StorefrontContext.Current.ShopName, currency);
            var result = PricingServiceProvider.CurrencyChosen(request);

            return new ManagerResponse<ServiceProviderResult, bool>(result, result.Success);
        }

        private DateTime GetCurrentDate()
        {
            var dateCookieValue = WebUtil.GetCookieValue(Context.Site.GetCookieKey(Sitecore.Constants.PreviewDateCookieName));
            return !string.IsNullOrEmpty(dateCookieValue) ? DateUtil.ToUniversalTime(DateUtil.IsoDateToDateTime(dateCookieValue)) : DateTime.UtcNow;
        }
    }
}