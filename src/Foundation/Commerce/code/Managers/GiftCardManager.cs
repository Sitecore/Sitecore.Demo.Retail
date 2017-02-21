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

using Sitecore.Commerce.Entities.GiftCards;
using Sitecore.Commerce.Services.GiftCards;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class GiftCardManager : BaseManager
    {
        public GiftCardManager([NotNull] GiftCardServiceProvider giftCardServiceProvider)
        {
            Assert.ArgumentNotNull(giftCardServiceProvider, nameof(giftCardServiceProvider));

            GiftCardServiceProvider = giftCardServiceProvider;
        }

        private GiftCardServiceProvider GiftCardServiceProvider { get; set; }

        public ManagerResponse<GetGiftCardResult, decimal> GetGiftCardBalance([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] string giftCardId)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(giftCardId, nameof(giftCardId));

            var result = GetGiftCard(giftCardId, storefront.ShopName).ServiceProviderResult;

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetGiftCardResult, decimal>(result, result.Success && result.GiftCard != null ? result.GiftCard.Balance : -1);
        }

        private ManagerResponse<GetGiftCardResult, GiftCard> GetGiftCard(string giftCardId, string shopName)
        {
            Assert.ArgumentNotNullOrEmpty(giftCardId, nameof(giftCardId));

            var request = new GetGiftCardRequest(giftCardId, shopName);
            var result = GiftCardServiceProvider.GetGiftCard(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetGiftCardResult, GiftCard>(result, result.GiftCard);
        }
    }
}