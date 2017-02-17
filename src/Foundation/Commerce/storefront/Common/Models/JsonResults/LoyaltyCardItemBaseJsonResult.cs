//-----------------------------------------------------------------------
// <copyright file="LoyaltyCardItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LoyaltyCardItemBaseJsonResult class.</summary>
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

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class LoyaltyCardItemBaseJsonResult : BaseJsonResult
    {
        public LoyaltyCardItemBaseJsonResult()
        {
            RewardPoints = new List<LoyaltyRewardPointItemBaseJsonResult>();
        }

        public LoyaltyCardItemBaseJsonResult(ServiceProviderResult result)
            : base(result)
        {
            RewardPoints = new List<LoyaltyRewardPointItemBaseJsonResult>();
        }

        public string CardNumber { get; set; }

        public List<LoyaltyRewardPointItemBaseJsonResult> RewardPoints { get; protected set; }

        public virtual void Initialize(LoyaltyCard loyaltyCard)
        {
            Assert.ArgumentNotNull(loyaltyCard, nameof(loyaltyCard));

            CardNumber = loyaltyCard.CardNumber;

            foreach (var point in loyaltyCard.RewardPoints)
            {
                var result = new LoyaltyRewardPointItemBaseJsonResult();
                result.Initialize(point);
                RewardPoints.Add(result);
            }
        }
    }
}