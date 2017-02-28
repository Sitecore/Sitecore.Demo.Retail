//-----------------------------------------------------------------------
// <copyright file="WishListBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the WishListsBaseJsonResult class.</summary>
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
using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Commerce.Services;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class WishListBaseJsonResult : BaseJsonResult
    {
        public WishListBaseJsonResult()
        {
        }

        public WishListBaseJsonResult(ServiceProviderResult result)
            : base(result)
        {
        }

        public string Name { get; set; }

        public bool IsFavorite { get; set; }

        public string ExternalId { get; set; }

        public List<WishListItemBaseJsonResult> Lines { get; } = new List<WishListItemBaseJsonResult>();

        public void Initialize(WishList wishList)
        {
            if (wishList == null)
            {
                return;
            }

            Name = wishList.Name;
            IsFavorite = wishList.IsFavorite;
            ExternalId = wishList.ExternalId;

            foreach (var line in wishList.Lines)
            {
                Lines.Add(new WishListItemBaseJsonResult(line, wishList.ExternalId));
            }
        }
    }
}