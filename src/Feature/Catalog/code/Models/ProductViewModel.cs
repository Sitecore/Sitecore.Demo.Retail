//-----------------------------------------------------------------------
// <copyright file="ProductViewModel.cs" company="Sitecore Corporation">
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
using System.Web;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;
using Sitecore.Mvc;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    public class ProductViewModel : ICatalogProduct, IInventoryProduct
    {
        private List<MediaItem> _images;

        public ProductViewModel(Item item, List<VariantViewModel> variants = null)
        {
            Assert.IsTrue(item.IsDerived(Foundation.Commerce.Templates.Commerce.Product.ID), "Item must be a Product");
            Item = item;
            Variants = variants;
        }

        public Item Item { get; set; }

        public string ProductName { get; set; }

        public string ParentCategoryId { get; set; }

        public string ParentCategoryName { get; set; }

        public string Description { get; set; }

        public HtmlString DescriptionRender => PageContext.Current.HtmlHelper.Sitecore().Field(Foundation.Commerce.Templates.Commerce.Product.Fields.Description, Item);

        public List<MediaItem> Images
        {
            get
            {
                if (_images != null)
                {
                    return _images;
                }

                MultilistField field = Item.Fields["Images"];
                if (field == null)
                {
                    return new List<MediaItem>();
                }

                _images = new List<MediaItem>();
                foreach (var id in field.TargetIDs)
                {
                    MediaItem mediaItem = Item.Database.GetItem(id);
                    _images.Add(mediaItem);
                }
                return _images;
            }
        }

        public MediaItem OnSaleOverlayImage => StorefrontManager.CurrentStorefront.OnSaleOverlayImage;

        public string DisplayName
        {
            get
            {
                var displayName = Item[FieldIDs.DisplayName.ToString()];
                return string.IsNullOrEmpty(displayName) ? string.Empty : displayName;
            }
        }

        public HtmlString DisplayNameRender => PageContext.Current.HtmlHelper.Sitecore().Field(FieldIDs.DisplayName.ToString(), Item);

        public string ListPriceWithCurrency => ListPrice.HasValue ? ListPrice.ToCurrency(StorefrontManager.CurrentStorefront.DefaultCurrency) : string.Empty;

        public decimal CustomerAverageRating { get; set; }

        public string AdjustedPriceWithCurrency => AdjustedPrice.HasValue ? AdjustedPrice.ToCurrency(StorefrontManager.CurrentStorefront.DefaultCurrency) : string.Empty;

        public decimal SavingsPercentage => CalculateSavingsPercentage(AdjustedPrice, ListPrice);

        public string LowestPricedVariantAdjustedPriceWithCurrency => LowestPricedVariantAdjustedPrice.HasValue ? LowestPricedVariantAdjustedPrice.ToCurrency(StorefrontManager.CurrentStorefront.DefaultCurrency) : string.Empty;

        public string LowestPricedVariantListPriceWithCurrency => LowestPricedVariantListPrice.HasValue ? LowestPricedVariantListPrice.ToCurrency(StorefrontManager.CurrentStorefront.DefaultCurrency) : string.Empty;

        public decimal VariantSavingsPercentage => CalculateSavingsPercentage(LowestPricedVariantAdjustedPrice, LowestPricedVariantListPrice);

        public bool IsOnSale => Item.IsDerived(Foundation.Commerce.Templates.Commerce.Product.ID) && Item.Fields[Foundation.Commerce.Templates.Commerce.Product.Fields.OnSale].IsChecked();

        public bool IsProduct
        {
            get
            {
                var val = Item["IsProduct"];

                return !string.IsNullOrEmpty(val) && val != "0";
            }
        }

        public decimal? Quantity { get; set; }

        public double StockCount { get; set; }

        public string StockAvailabilityDate { get; set; }

        public IEnumerable<VariantViewModel> Variants { get; }

        public List<string> VariantProductColor
        {
            get { return Variants.GroupBy(v => v.ProductColor).Select(grp => grp.First().ProductColor).ToList(); }
        }

        public List<string> VariantSizes
        {
            get
            {
                var groups = Variants.GroupBy(v => v.Size);
                var sizes = groups.Select(grp => grp.First().Size).ToList();
                return sizes;
            }
        }

        public Item ProductListTexts
        {
            get
            {
                var home = Context.Database.GetItem(Context.Site.RootPath + Context.Site.StartItem);
                var textsItemPath = home["Product List Texts"];
                if (string.IsNullOrEmpty(textsItemPath))
                {
                    return null;
                }

                return Context.Database.GetItem(textsItemPath);
            }
        }

        public string AddToCartLinkText
        {
            get
            {
                var productListTexts = ProductListTexts;
                if (productListTexts != null)
                {
                    return productListTexts["Add To Cart Link Text"];
                }

                return string.Empty;
            }
        }

        public string ProductDetailsLinkText
        {
            get
            {
                var productListTexts = ProductListTexts;
                if (productListTexts != null)
                {
                    return productListTexts["Product Page Link Text"];
                }

                return string.Empty;
            }
        }

        public string ProductUrl => LinkManager.GetDynamicUrl(Item);

        public string ProductId => Item.Name;

        public decimal? ListPrice { get; set; }

        public decimal? AdjustedPrice { get; set; }

        public decimal? LowestPricedVariantAdjustedPrice { get; set; }

        public decimal? LowestPricedVariantListPrice { get; set; }

        public decimal? HighestPricedVariantAdjustedPrice { get; set; }

        public string CatalogName => Item["CatalogName"];
        IEnumerable<ICatalogProductVariant> ICatalogProduct.Variants => Variants;

        public StockStatus StockStatus { get; set; }

        public string StockStatusName { get; set; }
        IEnumerable<IProductVariant> IInventoryProduct.Variants => Variants;

        public List<VariantViewModel> DistinctColourVariants
        {
            get
            {
                return Variants.Where(variant => !string.IsNullOrWhiteSpace(variant.ProductColor)).Distinct(new VariantPropertiesEqualityComparer(VariantPropertiesComparisonProperty.ProductColor)).ToList();
                
            }
        }

        public HtmlString RenderField(string fieldName)
        {
            var fieldValue = PageContext.Current.HtmlHelper.Sitecore().Field(fieldName, Item);
            if (fieldName.Equals("Features", StringComparison.OrdinalIgnoreCase)
                && (fieldValue.ToString().Equals("Default", StringComparison.OrdinalIgnoreCase) || fieldValue.ToString().Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                && Item.HasChildren
                && Item.Children[0] != null)
            {
                fieldValue = PageContext.Current.HtmlHelper.Sitecore().Field("VariantFeatures", Item.Children[0]);
            }

            return fieldValue;
        }

        public string GetLink()
        {
            return LinkManager.GetDynamicUrl(Item);
        }

        public decimal CalculateSavingsPercentage(decimal? adjustedPrice, decimal? listPrice)
        {
            if (!adjustedPrice.HasValue || !listPrice.HasValue || listPrice.Value <= adjustedPrice.Value)
            {
                return 0;
            }

            var percentage = decimal.Floor(100 * (listPrice.Value - adjustedPrice.Value) / listPrice.Value);
            var integerPart = (int) percentage;
            return integerPart == 0 ? 1M : integerPart;
        }

        public bool IsCategory()
        {
            return Item.IsDerived(Foundation.Commerce.Templates.Commerce.Category.ID);
        }
    }
}