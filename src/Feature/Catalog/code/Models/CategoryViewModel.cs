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
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Links;
using Sitecore.Mvc;
using Sitecore.Mvc.Common;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    public class CategoryViewModel
    {
        private List<MediaItem> _images;

        public CategoryViewModel(Item item)
        {
            ChildProducts = new List<ProductViewModel>();
            Item = item;
        }

        public CategoryViewModel(Item categoryItem, SearchResults products, IEnumerable<CommerceQuerySort> sortFields, CommerceSearchOptions searchOptions) : this(categoryItem)
        {
            var itemsPerPage = searchOptions?.NumberOfItemsToReturn ?? 0;

            if (products != null)
            {
                ChildProducts = new List<ProductViewModel>();
                foreach (var child in products.SearchResultItems)
                {
                    var productModel = new ProductViewModel(child);
                    ChildProducts.Add(productModel);
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

            SortFields = sortFields;
        }

        public Item Item { get; set; }

        public string DisplayName => Item.DisplayName;

        public HtmlString DisplayNameRender => PageContext.Current.HtmlHelper.Sitecore().Field(FieldIDs.DisplayName.ToString(), Item);

        public string Description => Item[Templates.Generated.Category.Fields.Description];

        public string Name => Item.Name;

        public HtmlString DescriptionRender => PageContext.Current.HtmlHelper.Sitecore().Field(Templates.Generated.Category.Fields.Description, Item);

        public List<MediaItem> Images
        {
            get
            {
                if (_images != null)
                    return _images;

                _images = new List<MediaItem>();

                MultilistField field = Item.Fields[Templates.Generated.Category.Fields.Images];

                if (field == null)
                    return _images;
                foreach (var id in field.TargetIDs)
                {
                    MediaItem mediaItem = Item.Database.GetItem(id);
                    _images.Add(mediaItem);
                }

                return _images;
            }
        }

        public IEnumerable<CommerceQueryFacet> ChildProductFacets { get; protected set; }

        public IEnumerable<CommerceQuerySort> SortFields { get; protected set; }

        public List<ProductViewModel> ChildProducts { get; set; }

        public PaginationModel Pagination { get; set; }

        [XmlIgnore]
        protected ViewContext CurrentViewContext => ContextService.Get().GetCurrentOrDefault<ViewContext>();

        public string GetLink()
        {
            return LinkManager.GetDynamicUrl(Item).TrimEnd('/');
        }
    }
}