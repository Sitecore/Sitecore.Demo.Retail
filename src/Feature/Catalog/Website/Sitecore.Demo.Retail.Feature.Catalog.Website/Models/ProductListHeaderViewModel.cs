//-----------------------------------------------------------------------
// <copyright file="ProductListHeaderViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the ProductListHeaderViewModel class.</summary>
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
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public class ProductListHeaderViewModel : RenderingModel
    {
        private const string DefaultPageSizeClass = "changePageSize";

        public ProductListHeaderViewModel()
        {
            PageSizeClass = DefaultPageSizeClass;
        }

        public IEnumerable<QuerySortField> SortFields { get; protected set; }

        public PaginationModel Pagination { get; set; }

        public string PageSizeClass { get; set; }

        public void Initialize(Rendering rendering, SearchResults products, IEnumerable<QuerySortField> sortFields, SearchOptions searchOptions)
        {
            base.Initialize(rendering);

            if (products != null && searchOptions != null)
            {
                var itemsPerPage = searchOptions.NumberOfItemsToReturn;
                var alreadyShown = products.CurrentPageNumber * searchOptions.NumberOfItemsToReturn;
                Pagination = new PaginationModel
                {
                    PageNumber = products.CurrentPageNumber,
                    TotalResultCount = products.TotalItemCount,
                    NumberOfPages = products.TotalPageCount,
                    PageResultCount = itemsPerPage,
                    StartResultIndex = alreadyShown + 1,
                    EndResultIndex = Math.Min(products.TotalItemCount, alreadyShown + itemsPerPage)
                };
            }

            SortFields = sortFields ?? Enumerable.Empty<QuerySortField>();
        }
    }
}