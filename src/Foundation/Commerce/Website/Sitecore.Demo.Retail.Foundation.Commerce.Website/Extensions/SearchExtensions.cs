//-----------------------------------------------------------------------
// <copyright file="SearchExtensions.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the SearchExtensions class.</summary>
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

using System.Linq;
using System.Web.Helpers;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.ContentSearch.Linq;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions
{
    public static class SearchExtensions
    {
        public static QueryFacet ToQueryFacet(this CommerceQueryFacet facet)
        {
            return new QueryFacet
            {
                Name = facet.Name,
                DisplayName = facet.DisplayName,
                Values = facet.Values,
                FoundValues = facet.FoundValues,
            };
        }

        public static CommerceSearchOptions ToCommerceSearchOptions(this SearchOptions searchOptions)
        {
            return new CommerceSearchOptions
            {
                NumberOfItemsToReturn = searchOptions.NumberOfItemsToReturn,
                StartPageIndex = searchOptions.StartPageIndex,
                FacetFields = searchOptions.FacetFields.Select(f => f.ToCommerceQueryFacet()).ToList(),
                SortDirection = searchOptions.SortDirection == SortDirection.Ascending ? CommerceConstants.SortDirection.Asc : CommerceConstants.SortDirection.Desc,
                SortField = searchOptions.SortField
            };
        }

        public static SearchOptions ToSearchOptions(this CommerceSearchOptions commerceSearchOptions)
        {
            return new SearchOptions
            {
                NumberOfItemsToReturn = commerceSearchOptions.NumberOfItemsToReturn,
                StartPageIndex = commerceSearchOptions.StartPageIndex,
                FacetFields = commerceSearchOptions.FacetFields.Select(f => f.ToQueryFacet()).ToList(),
                SortDirection = commerceSearchOptions.SortDirection == CommerceConstants.SortDirection.Asc ? SortDirection.Ascending : SortDirection.Descending,
                SortField = commerceSearchOptions.SortField
            };
        }

        public static CommerceQueryFacet ToCommerceQueryFacet(this QueryFacet queryFacet)
        {
            return new CommerceQueryFacet
            {
                DisplayName = queryFacet.DisplayName,
                Name = queryFacet.Name,
                Values = queryFacet.Values,
                FoundValues = queryFacet.FoundValues
            };
        }

        public static QuerySortField ToQuerySortField(this CommerceQuerySort querySortField)
        {
            return new QuerySortField
            {
                Name = querySortField.Name,
                DisplayName = querySortField.DisplayName
            };
        }

        public static bool IsValid(this FacetValue value)
        {
            return value != null && value.AggregateCount > 0;
        }
    }
}