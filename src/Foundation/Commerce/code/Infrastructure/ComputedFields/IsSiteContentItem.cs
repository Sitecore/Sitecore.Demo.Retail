// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteContentItem.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>
//   Contains the SiteContentItem computed field class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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

using Sitecore.Commerce.Search.ComputedFields;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Infrastructure.ComputedFields
{
    public class IsSiteContentItem : BaseComputedField
    {
        public override object ComputeValue(IIndexable itemToIndex)
        {
            Assert.ArgumentNotNull(itemToIndex, nameof(itemToIndex));

            var item = itemToIndex as SitecoreIndexableItem;
            if (item?.Item == null)
            {
                return false;
            }

            return item.Item.HasLayout() && item.Item[StorefrontConstants.ItemFields.DisplayInSearchResults] == "1";
        }
    }
}