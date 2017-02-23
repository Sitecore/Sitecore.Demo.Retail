//-----------------------------------------------------------------------
// <copyright file="LoyaltyRewardPointItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LoyaltyRewardPointItemBaseJsonResult class.</summary>
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

using System.Collections.Generic;
using Sitecore.Commerce.Entities.LoyaltyPrograms;
using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class LoyaltyRewardPointItemBaseJsonResult : BaseJsonResult
    {
        public LoyaltyRewardPointItemBaseJsonResult()
        {
            Transactions = new List<LoyaltyTransactionItemBaseJsonResult>();
        }

        public LoyaltyRewardPointItemBaseJsonResult(ServiceProviderResult result)
            : base(result)
        {
            Transactions = new List<LoyaltyTransactionItemBaseJsonResult>();
        }

        public string RewardPointId { get; set; }

        public string ActivePoints { get; set; }

        public string CurrencyCode { get; set; }

        public string Description { get; set; }

        public string ExpiredPoints { get; set; }

        public string IssuedPoints { get; set; }

        public string RewardPointType { get; set; }

        public string UsedPoints { get; set; }

        public List<LoyaltyTransactionItemBaseJsonResult> Transactions { get; protected set; }

        public void Initialize(LoyaltyRewardPoint rewardPoint)
        {
            Assert.ArgumentNotNull(rewardPoint, nameof(rewardPoint));

            ActivePoints = rewardPoint.ActivePoints.ToString(Context.Language.CultureInfo);
            CurrencyCode = rewardPoint.CurrencyCode;
            Description = rewardPoint.Description;
            ExpiredPoints = rewardPoint.ExpiredPoints.ToString(Context.Language.CultureInfo);
            IssuedPoints = rewardPoint.IssuedPoints.ToString(Context.Language.CultureInfo);
            RewardPointType = rewardPoint.RewardPointType.Name;
            UsedPoints = rewardPoint.UsedPoints.ToString(Context.Language.CultureInfo);

            var transactions = rewardPoint.GetPropertyValue("Transactions") as List<LoyaltyCardTransaction>;
            if (transactions == null || transactions.Count <= 0)
            {
                return;
            }

            foreach (var transaction in transactions)
            {
                var result = new LoyaltyTransactionItemBaseJsonResult();
                result.Initialize(transaction);
                Transactions.Add(result);
            }
        }
    }
}