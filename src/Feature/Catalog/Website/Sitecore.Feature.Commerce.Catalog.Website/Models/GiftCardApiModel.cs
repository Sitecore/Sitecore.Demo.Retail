//-----------------------------------------------------------------------
// <copyright file="GiftCardBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the GiftCardBaseJsonResult class.</summary>
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
using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Catalog.Website.Models
{
    public class GiftCardApiModel : BaseApiModel
    {
        public GiftCardApiModel()
        {
        }

        public GiftCardApiModel(ServiceProviderResult result)
            : base(result)
        {
        }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string CustomerId { get; set; }

        public string ShopName { get; set; }

        public string CurrencyCode { get; set; }

        public decimal Balance { get; set; }

        public string FormattedBalance { get; set; }

        public string OriginalAmount { get; set; }

        public string Description { get; set; }

        public void Initialize(GiftCard giftCard)
        {
            Assert.ArgumentNotNull(giftCard, nameof(giftCard));

            ExternalId = giftCard.ExternalId;
            Name = giftCard.Name;
            CustomerId = giftCard.CustomerId;
            ShopName = giftCard.ShopName;
            CurrencyCode = giftCard.CurrencyCode;
            Balance = giftCard.Balance;
            FormattedBalance = giftCard.Balance.ToCurrency();
            OriginalAmount = giftCard.OriginalAmount.ToCurrency();
            Description = giftCard.Description;
        }
    }
}