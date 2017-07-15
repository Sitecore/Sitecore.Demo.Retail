//-----------------------------------------------------------------------
// <copyright file="GiftCardManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the gift card 
// business logic for the site.</summary>
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
using Sitecore.Commerce.Entities.GiftCards;
using Sitecore.Commerce.Services.GiftCards;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
    public class GiftCardManager : IManager
    {
        public GiftCardManager(GiftCardServiceProvider giftCardServiceProvider, StorefrontContext storefrontContext)
        {
            Assert.ArgumentNotNull(giftCardServiceProvider, nameof(giftCardServiceProvider));

            GiftCardServiceProvider = giftCardServiceProvider;
            StorefrontContext = storefrontContext;
        }

        private GiftCardServiceProvider GiftCardServiceProvider { get; }
        public StorefrontContext StorefrontContext { get; }

        public ManagerResponse<GetGiftCardResult, decimal> GetGiftCardBalance(string giftCardId)
        {
            Assert.ArgumentNotNullOrEmpty(giftCardId, nameof(giftCardId));

            var result = GetGiftCard(giftCardId).ServiceProviderResult;

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetGiftCardResult, decimal>(result, result.Success && result.GiftCard != null ? result.GiftCard.Balance : -1);
        }

        private ManagerResponse<GetGiftCardResult, GiftCard> GetGiftCard(string giftCardId)
        {
            Assert.ArgumentNotNullOrEmpty(giftCardId, nameof(giftCardId));

            if (this.StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new GetGiftCardRequest(giftCardId, StorefrontContext.Current.ShopName);
            var result = GiftCardServiceProvider.GetGiftCard(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetGiftCardResult, GiftCard>(result, result.GiftCard);
        }
    }
}