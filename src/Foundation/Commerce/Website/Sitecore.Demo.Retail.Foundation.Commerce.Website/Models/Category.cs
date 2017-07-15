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
using Sitecore.Data.Items;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public class Category : SitecoreItemBase
    {
        private int _itemsPerPage;

        public Category(Item item)
        {
            InnerItem = item;
        }

        public string Name => InnerItem.Name;
        public string Title => InnerItem.DisplayName;
        public List<QueryFacet> RequiredFacets { get; set; }
        public List<QuerySortField> SortFields { get; set; }

        public int ItemsPerPage
        {
            get { return _itemsPerPage <= 0 ? 12 : _itemsPerPage; }

            set { _itemsPerPage = value; }
        }
    }
}