//-----------------------------------------------------------------------
// <copyright file="PaginationViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the PaginationViewModel class.</summary>
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
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public class PaginationViewModel : RenderingModel
    {
        public PaginationModel Pagination { get; set; }

        public string QueryStringToken { get; set; }

        public void Initialize(Rendering rendering, SearchResults products, SearchOptions searchOptions)
        {
            base.Initialize(rendering);
            QueryStringToken = Demo.Retail.Feature.Catalog.Website.Constants.QueryString.Paging;

            var itemsPerPage = searchOptions?.NumberOfItemsToReturn ?? 20;

            if (products == null)
            {
                return;
            }
            var alreadyShown = products.CurrentPageNumber * itemsPerPage;
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
    }
}