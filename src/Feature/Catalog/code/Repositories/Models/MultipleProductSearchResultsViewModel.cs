//-----------------------------------------------------------------------
// <copyright file="MultipleProductSearchResults.cs" company="Sitecore Corporation">
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

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Foundation.Commerce.Models.Search;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    public class MultipleProductSearchResultsViewModel : RenderingModel
    {
        public MultipleProductSearchResultsViewModel(MultipleProductSearchResults searchResults)
        {
            SearchResults = searchResults;
        }

        public MultipleProductSearchResults SearchResults { get; }

        public List<ProductSearchResultViewModel> ProductSearchResults { get; set; }
        public string DisplayName { get; set; }

        public override void Initialize(Rendering rendering)
        {
            base.Initialize(rendering);

            ProductSearchResults = new List<ProductSearchResultViewModel>();
            if (SearchResults.SearchResults == null || !SearchResults.SearchResults.Any())
            {
                return;
            }

            foreach (var results in SearchResults.SearchResults)
            {
                var productSearchResultModel = new ProductSearchResultViewModel();
                productSearchResultModel.Initialize(Rendering, results);
                ProductSearchResults.Add(productSearchResultModel);
            }
        }
    }
}