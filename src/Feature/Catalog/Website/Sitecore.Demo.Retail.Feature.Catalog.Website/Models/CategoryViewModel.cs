//-----------------------------------------------------------------------
// <copyright file="CategoryViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CategoryViewModel class.</summary>
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
using System.Web.Mvc;
using System.Xml.Serialization;
using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Feature.Catalog.Website.Factories;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Mvc.Common;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public class CategoryViewModel : CatalogItemViewModel
    {
        public CategoryViewModel(Item categoryItem, SearchResults products, IEnumerable<QuerySortField> sortFields, SearchOptions searchOptions) : base(categoryItem)
        {
            var itemsPerPage = searchOptions?.NumberOfItemsToReturn ?? 0;

            if (products != null)
            {
                ChildProducts = new List<ProductViewModel>();
                var factory = DependencyResolver.Current.GetService<ProductViewModelFactory>();
                foreach (var child in products.SearchResultItems)
                {
                    ChildProducts.Add(factory.Create(child));
                }

                ChildProductFacets = products.Facets;
                if (itemsPerPage > products.SearchResultItems.Count)
                    itemsPerPage = products.SearchResultItems.Count;

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
            else
                ChildProducts = new List<ProductViewModel>();

            SortFields = sortFields;
        }

        public IEnumerable<QueryFacet> ChildProductFacets { get; protected set; }

        public IEnumerable<QuerySortField> SortFields { get; protected set; }

        public List<ProductViewModel> ChildProducts { get; set; }

        public PaginationModel Pagination { get; set; }

        [XmlIgnore]
        protected ViewContext CurrentViewContext => ContextService.Get().GetCurrentOrDefault<ViewContext>();

        public override string ImagesFieldName => Templates.Generated.Category.Fields.Images;
        public override string DescriptionFieldName => Templates.Generated.Category.Fields.Description;
        public override string TitleFieldName => FieldIDs.DisplayName.ToString();
    }
}