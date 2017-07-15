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
using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Feature.Catalog.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;
using Sitecore.Mvc;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public class ProductViewModel : CatalogItemViewModel, ICatalogProduct, IInventoryProduct
    {
        public ProductViewModel(Item item, List<VariantViewModel> variants = null) : base(item)
        {
            Assert.IsTrue(item.IsDerived(Foundation.Commerce.Website.Templates.Commerce.Product.Id), "Item must be a Product");
            Variants = variants;
        }

        public string ProductName { get; set; }

        public string ParentCategoryId { get; set; }

        public string ParentCategoryName { get; set; }

        public HtmlString DescriptionRender => PageContext.Current.HtmlHelper.Sitecore().Field(Foundation.Commerce.Website.Templates.Commerce.Product.FieldNames.Description, Item);

        public MediaItem OverlayImage { get; set; }

        public string ListPriceWithCurrency => ListPrice.HasValue ? ListPrice.ToCurrency() : string.Empty;

        public decimal CustomerAverageRating { get; set; }

        public string AdjustedPriceWithCurrency => AdjustedPrice.HasValue ? AdjustedPrice.ToCurrency() : string.Empty;

        public decimal SavingsPercentage => CalculateSavingsPercentage(AdjustedPrice, ListPrice);

        public string LowestPricedVariantAdjustedPriceWithCurrency => LowestPricedVariantAdjustedPrice.HasValue ? LowestPricedVariantAdjustedPrice.ToCurrency() : string.Empty;

        public string LowestPricedVariantListPriceWithCurrency => LowestPricedVariantListPrice.HasValue ? LowestPricedVariantListPrice.ToCurrency() : string.Empty;

        public decimal VariantSavingsPercentage => CalculateSavingsPercentage(LowestPricedVariantAdjustedPrice, LowestPricedVariantListPrice);

        public bool IsOnSale => Item.IsDerived(Foundation.Commerce.Website.Templates.Commerce.Product.Id) && Item.Fields[Foundation.Commerce.Website.Templates.Commerce.Product.FieldNames.OnSale].IsChecked();

        public IEnumerable<string> Tags
        {
            get
            {
                var tagsString = Item[Templates.Generated.Product.Fields.Tags];
                return string.IsNullOrWhiteSpace(tagsString) ? Enumerable.Empty<string>() : tagsString.Split(new[] {',', '|', ';'}, StringSplitOptions.RemoveEmptyEntries);
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

        public HtmlString RenderFeatures()
        {
            var fieldValue = PageContext.Current.HtmlHelper.Sitecore().Field("Features", Item);
            if (fieldValue.ToString().Equals("Default", StringComparison.OrdinalIgnoreCase) || fieldValue.ToString().Equals(string.Empty, StringComparison.OrdinalIgnoreCase)
                && Item.HasChildren
                && Item.Children[0] != null)
            {
                fieldValue = PageContext.Current.HtmlHelper.Sitecore().Field("VariantFeatures", Item.Children[0]);
            }

            return fieldValue;
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
            return Item.IsDerived(Foundation.Commerce.Website.Templates.Commerce.Category.Id);
        }

        public override string ImagesFieldName => Templates.Generated.Product.Fields.Images;
        public override string DescriptionFieldName => Templates.Generated.Product.Fields.Description;
        public override string TitleFieldName => FieldIDs.DisplayName.ToString();
    }
}