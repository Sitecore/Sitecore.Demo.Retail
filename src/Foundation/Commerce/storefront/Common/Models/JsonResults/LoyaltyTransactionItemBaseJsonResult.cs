//-----------------------------------------------------------------------
// <copyright file="LoyaltyTransactionItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LoyaltyTransactionItemBaseJsonResult class.</summary>
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

using Sitecore.Commerce.Entities.LoyaltyPrograms;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class LoyaltyTransactionItemBaseJsonResult : BaseJsonResult
    {
        public string ExternalId { get; set; }

        public string EntryTime { get; set; }

        public string EntryDate { get; set; }

        public string EntryType { get; set; }

        public string ExpirationDate { get; set; }

        public string Points { get; set; }

        public string Store { get; set; }

        public virtual void Initialize(LoyaltyCardTransaction transaction)
        {
            Assert.ArgumentNotNull(transaction, "transaction");

            ExternalId = transaction.ExternalId;
            EntryTime = transaction.EntryDateTime.ToShortTimeString();
            EntryDate = transaction.EntryDateTime.ToDisplayedDate();
            EntryType = transaction.EntryType.Name;
            ExpirationDate = transaction.ExpirationDate.ToDisplayedDate();
            Points = transaction.RewardPointAmount.ToString(Context.Language.CultureInfo);
            Store = transaction.ShopName;
        }
    }
}