//-----------------------------------------------------------------------
// <copyright file="LoyaltyTierItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LoyaltyTierItemBaseJsonResult class.</summary>
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
    public class LoyaltyTierItemBaseJsonResult : BaseJsonResult
    {
        public string Description { get; set; }

        public string TierId { get; set; }

        public string TierLevel { get; set; }

        public string ValidFrom { get; set; }

        public string ValidTo { get; set; }

        public bool IsElegible { get; set; }

        public virtual void Initialize(LoyaltyTier tier, LoyaltyCardTier cardTier)
        {
            Assert.ArgumentNotNull(tier, nameof(tier));

            TierId = tier.TierId;
            Description = tier.Description;
            TierLevel = tier.TierLevel.ToString(Context.Language.CultureInfo);
            IsElegible = false;

            if (cardTier == null)
            {
                return;
            }

            ValidFrom = cardTier.ValidFrom.ToDisplayedDate();
            ValidTo = cardTier.ValidTo.ToDisplayedDate();
            IsElegible = true;
        }
    }
}