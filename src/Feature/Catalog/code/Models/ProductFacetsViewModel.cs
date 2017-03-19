//-----------------------------------------------------------------------
// <copyright file="ProductFacetsViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the ProductFacetsViewModel class.</summary>
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
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    public class ProductFacetsViewModel : RenderingModel
    {
        public ProductFacetsViewModel()
        {
            ChildProductFacets = new List<QueryFacet>();
            ActiveFacets = new List<QueryFacet>();
        }

        public IEnumerable<QueryFacet> ChildProductFacets { get; protected set; }

        public IEnumerable<QueryFacet> ActiveFacets { get; protected set; }

        public void Initialize(Rendering rendering, SearchResults products, SearchOptions searchOptions)
        {
            base.Initialize(rendering);

            if (products != null)
            {
                ChildProductFacets = products.Facets;
            }

            ActiveFacets = searchOptions.FacetFields;
        }
    }
}