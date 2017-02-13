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
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.ContentSearch.Linq;

namespace Sitecore.Foundation.Commerce.Extensions
{
    public static class SearchExtensions
    {
        public static void Clean(this CommerceQueryFacet facet)
        {
            if (facet.FoundValues != null)
            {
                var items = facet.FoundValues.Where(v => string.IsNullOrEmpty(v.Name) || v.AggregateCount == 0);
                items.ToList().ForEach(v => facet.FoundValues.Remove(v));
            }
        }

        public static bool IsValid(this CommerceQueryFacet facet)
        {
            facet.Clean();

            return facet.FoundValues != null && facet.FoundValues.Count > 0;
        }

        public static bool IsValid(this FacetValue value)
        {
            return value.AggregateCount > 0;
        }
    }
}