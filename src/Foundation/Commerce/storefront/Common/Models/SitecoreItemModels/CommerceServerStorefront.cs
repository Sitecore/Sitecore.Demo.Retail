//-----------------------------------------------------------------------
// <copyright file="CommerceServerStorefront.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
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

using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Reference.Storefront.Models.SitecoreItemModels
{
    public class CommerceServerStorefront : CommerceStorefront
    {
        private Item _countryAndRegionItem;

        public CommerceServerStorefront()
        {
        }

        public CommerceServerStorefront(Item item)
            : base(item)
        {
        }

        public Item CountryAndRegionItem
        {
            get
            {
                if (_countryAndRegionItem == null)
                {
                    _countryAndRegionItem = HomeItem.Database.GetItem(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.CountryLocationPath]);
                }

                return _countryAndRegionItem;
            }
        }

        public override bool SupportsWishLists
        {
            get { return MainUtil.GetBool(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.SupportsWishLists], false); }
        }

        public override bool SupportsLoyaltyPrograms
        {
            get { return MainUtil.GetBool(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.SupportsLoyaltyProgram], false); }
        }

        public override bool SupportsGiftCardPayment
        {
            get { return MainUtil.GetBool(HomeItem[CommerceServerStorefrontConstants.KnownFieldNames.SupportsGirstCardPayment], false); }
        }
    }
}