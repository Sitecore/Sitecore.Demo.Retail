//-----------------------------------------------------------------------
// <copyright file="UrlExtensionMethods.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the ProfileExtensions class.</summary>
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

using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Foundation.Commerce.Website.Util;

namespace Sitecore.Feature.Commerce.Catalog.Website.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string AddToFacets(this UrlHelper helper, string facetName, string facetValue)
        {
            var currentUrl = UrlBuilder.CurrentUrl;
            var facetQuery = currentUrl.QueryList[global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets];
            var facetQueryString = GetFacetQueryString(facetQuery, facetName, facetValue);

            if (!string.IsNullOrEmpty(facetQueryString))
            {
                currentUrl.QueryList.AddOrSet(global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets, HttpUtility.UrlDecode(facetQueryString).Remove(0, 1));
            }
            else
            {
                currentUrl.QueryList.Remove(global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets);
            }

            currentUrl.QueryList.Remove(global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Paging);

            return currentUrl.ToString(true);
        }

        private static string GetFacetQueryString(string facetQuery, string facetName, string facetValue)
        {
            var facetCollection = new QueryStringCollection();

            if (!string.IsNullOrEmpty(facetQuery))
            {
                facetCollection.Parse(HttpUtility.UrlDecode(facetQuery));
            }

            if (facetCollection.Contains(facetName))
            {
                var facetQueryValues = facetCollection[facetName];

                if (facetQueryValues.Contains(facetValue))
                {
                    var facetValues = facetQueryValues.Split(global::Sitecore.Feature.Commerce.Catalog.Website.Constants.FacetsSeparator).Where(p => !string.Equals(p, facetValue, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (facetValues.Any())
                    {
                        facetCollection.Set(facetName, string.Join(global::Sitecore.Feature.Commerce.Catalog.Website.Constants.FacetsSeparator.ToString(), facetValues));
                    }
                    else
                    {
                        facetCollection.Remove(facetName);
                    }
                }
                else
                {
                    facetCollection.Set(facetName, facetQueryValues + global::Sitecore.Feature.Commerce.Catalog.Website.Constants.FacetsSeparator + facetValue);
                }
            }
            else
            {
                facetCollection.Add(facetName, facetValue);
            }

            return facetCollection.ToString();
        }
    }
}