//-----------------------------------------------------------------------
// <copyright file="Category.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the Category class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Data.Items;

namespace Sitecore.Foundation.Commerce.Models
{
    public class Category : SitecoreItemBase
    {
        private int _itemsPerPage;
        public Category(Item item)
        {
            this.InnerItem = item;
        }
        public string Name => this.InnerItem.Name;
        public string DisplayName => this.InnerItem.DisplayName;
        public List<CommerceQueryFacet> RequiredFacets { get; set; }
        public List<CommerceQuerySort> SortFields { get; set; }

        public int ItemsPerPage
        {
            get
            {
                return (_itemsPerPage == 0) ? StorefrontConstants.Settings.DefaultItemsPerPage : _itemsPerPage;
            }

            set 
            { 
                _itemsPerPage = value; 
            }
        }

        public string NameTitle()
        {
            return this.InnerItem["Name Title"];
        }

        public string Title()
        {
            return this.InnerItem[StorefrontConstants.ItemFields.Title];
        }
    }
}