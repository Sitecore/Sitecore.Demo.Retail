//-----------------------------------------------------------------------
// <copyright file="CategorySearchResults.cs" company="Sitecore Corporation">
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

using System.Collections.Generic;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data.Items;

namespace Sitecore.Foundation.Commerce.Website.Models.Search
{
    public class CategorySearchResults
    {
        public CategorySearchResults(List<Item> categoryItems, int totalCategoryCount, int totalPageCount, int currentPageNumber, List<FacetCategory> facets)
        {
            this.CategoryItems = categoryItems;
            this.TotalPageCount = totalPageCount;
            this.TotalCategoryCount = totalCategoryCount;
            this.Facets = facets;
            this.CurrentPageNumber = currentPageNumber;
        }

        public List<Item> CategoryItems { get; private set; }
        public int TotalCategoryCount { get; private set; }
        public int TotalPageCount { get; private set; }
        public List<FacetCategory> Facets { get; private set; }
        public int CurrentPageNumber { get; private set; }
    }
}