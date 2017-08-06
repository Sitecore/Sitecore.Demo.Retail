//-----------------------------------------------------------------------
// <copyright file="SearchResults.cs" company="Sitecore Corporation">
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
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.Commerce.Website.Models
{
    public class SearchResults
    {
        private List<QueryFacet> _facets;
        private List<Item> _searchResultItems;

        public SearchResults() : this(null, 0, 0, 0, null)
        {
        }

        public SearchResults(List<Item> searchResultItems, int totalItemCount, int totalPageCount, int currentPageNumber, List<QueryFacet> facets)
        {
            SearchResultItems = searchResultItems ?? new List<Item>();
            TotalPageCount = totalPageCount;
            TotalItemCount = totalItemCount;
            Facets = facets ?? new List<QueryFacet>();
            CurrentPageNumber = currentPageNumber;
        }

        public List<Item> SearchResultItems
        {
            get { return _searchResultItems; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                _searchResultItems = value;
            }
        }

        public int TotalItemCount { get; set; }

        public int TotalPageCount { get; set; }

        public int CurrentPageNumber { get; set; }
        public string Title { get; set; }

        public List<QueryFacet> Facets
        {
            get { return _facets; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                _facets = value;
            }
        }

    }
}