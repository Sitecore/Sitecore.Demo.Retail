//-----------------------------------------------------------------------
// <copyright file="WishListHeaderItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the WishListHeaderItemBaseJsonResult class.</summary>
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

using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Foundation.Commerce.Managers;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class WishListHeaderItemBaseJsonResult
    {
        public WishListHeaderItemBaseJsonResult(WishListHeader header)
        {
            ExternalId = header.ExternalId;
            Name = header.Name;
            IsFavorite = header.IsFavorite;
            DetailsUrl = string.Concat(StorefrontManager.StorefrontUri("/accountmanagement/mywishlist"), "?id=", header.ExternalId);
        }

        public string ExternalId { get; protected set; }

        public string Name { get; protected set; }

        public bool IsFavorite { get; protected set; }

        public string DetailsUrl { get; protected set; }
    }
}