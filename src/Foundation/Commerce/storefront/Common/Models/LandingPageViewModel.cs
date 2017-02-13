//-----------------------------------------------------------------------
// <copyright file="LandingPageViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LandingPageViewModel class.</summary>
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
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.Search;
using Sitecore.Mvc;
using Sitecore.Mvc.Common;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Reference.Storefront.Models
{
    public class LandingPageViewModel : RenderingModel
    {
        private readonly Item _item;
        private List<MediaItem> _images;

        public LandingPageViewModel()
        {
        }

        public LandingPageViewModel(Item item)
        {
            _item = item;
            ChildProducts = new List<Item>();
            ChildCategories = new List<Item>();
        }

        public override Item Item
        {
            get
            {
                if (_item == null)
                {
                    return base.Item;
                }

                return _item;
            }
        }

        public string DisplayName
        {
            get
            {
                if (Item != null && Item["Heading"] != null)
                {
                    return Item["Heading"];
                }

                return string.Empty;
            }
        }

        public string Description
        {
            get { return Item["Description"]; }

            set { Item["Description"] = value; }
        }

        public HtmlString DescriptionRender
        {
            get { return PageContext.Current.HtmlHelper.Sitecore().Field("Description", Item); }
        }

        public string AssociatedProductIds
        {
            get { return Item["AssociatedProducts"]; }
        }

        public List<MediaItem> HeroImages
        {
            get
            {
                if (_images != null)
                {
                    return _images;
                }

                _images = new List<MediaItem>();

                MultilistField field = Item.Fields["HeroImages"];

                if (field != null)
                {
                    foreach (var id in field.TargetIDs)
                    {
                        MediaItem mediaItem = Item.Database.GetItem(id);
                        _images.Add(mediaItem);
                    }
                }

                return _images;
            }
        }

        public IEnumerable<CommerceQueryFacet> ChildProductFacets { get; protected set; }

        public IEnumerable<CommerceQuerySort> SortFields { get; protected set; }

        public List<Item> ChildProducts { get; protected set; }

        public List<Item> ChildCategories { get; protected set; }

        public PaginationModel Pagination { get; set; }

        [XmlIgnore]
        protected ViewContext CurrentViewContext
        {
            get { return ContextService.Get().GetCurrentOrDefault<ViewContext>(); }
        }

        public void Initialize(Rendering rendering, SearchResults products, CategorySearchResults childCategories, IEnumerable<CommerceQuerySort> sortFields, CommerceSearchOptions searchOptions)
        {
            base.Initialize(rendering);

            ChildProducts = products == null ? new List<Item>() : products.SearchResultItems;
        }
    }
}