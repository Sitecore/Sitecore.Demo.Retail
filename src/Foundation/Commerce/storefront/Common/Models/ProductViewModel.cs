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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Links;
using Sitecore.Mvc;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Reference.Storefront.Models
{
    public class ProductViewModel : RenderingModel, ICatalogProduct, IInventoryProduct
    {
        private readonly Item _item;
        private List<MediaItem> _images;

        public ProductViewModel()
        {
        }

        public ProductViewModel(Item item)
        {
            _item = item;
        }

        public override Item Item => _item ?? base.Item;

        public string ProductName { get; set; }

        public string ParentCategoryId { get; set; }

        public string ParentCategoryName { get; set; }

        public string Description { get; set; }

        public HtmlString DescriptionRender => PageContext.Current.HtmlHelper.Sitecore().Field("Description", Item);

        public List<MediaItem> Images
        {
            get
            {
                if (_images != null)
                {
                    return _images;
                }

                _images = new List<MediaItem>();

                MultilistField field = Item.Fields["Images"];

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

        public string DisplayName
        {
            get
            {
                var displayName = Item[FieldIDs.DisplayName.ToString()];
                return string.IsNullOrEmpty(displayName) ? string.Empty : displayName;
            }
        }

        public HtmlString DisplayNameRender => PageContext.Current.HtmlHelper.Sitecore().Field(FieldIDs.DisplayName.ToString(), Item);

        public string ListPriceWithCurrency => ListPrice.HasValue ? ListPrice.ToCurrency(StorefrontManager.GetCustomerCurrency()) : string.Empty;

        public decimal CustomerAverageRating { get; set; }

        public string RatingStarImage
        {
            get
            {
                var starsImage = "stars_sm_0";
                var rating = CustomerAverageRating;
                if (rating > 0 && rating < 1)
                {
                    starsImage = "stars_sm_1";
                }
                else if (rating > 1 && rating < 2)
                {
                    starsImage = "stars_sm_1";
                }
                else if (rating > 2 && rating < 3)
                {
                    starsImage = "stars_sm_2";
                }
                else if (rating > 3 && rating < 4)
                {
                    starsImage = "stars_sm_3";
                }
                else if (rating > 4 && rating < 5)
                {
                    starsImage = "stars_sm_4";
                }
                else
                {
                    starsImage = "stars_sm_5";
                }

                return starsImage;
            }
        }

        public string AdjustedPriceWithCurrency => AdjustedPrice.HasValue ? AdjustedPrice.ToCurrency(StorefrontManager.GetCustomerCurrency()) : string.Empty;

        public decimal SavingsPercentage => CalculateSavingsPercentage(AdjustedPrice, ListPrice);

        public string LowestPricedVariantAdjustedPriceWithCurrency => LowestPricedVariantAdjustedPrice.HasValue ? LowestPricedVariantAdjustedPrice.ToCurrency(StorefrontManager.GetCustomerCurrency()) : string.Empty;

        public string LowestPricedVariantListPriceWithCurrency => LowestPricedVariantListPrice.HasValue ? LowestPricedVariantListPrice.ToCurrency(StorefrontManager.GetCustomerCurrency()) : string.Empty;

        public decimal VariantSavingsPercentage => CalculateSavingsPercentage(LowestPricedVariantAdjustedPrice, LowestPricedVariantListPrice);

        public bool IsOnSale => AdjustedPrice.HasValue && ListPrice.HasValue && AdjustedPrice < ListPrice;

        public bool IsProduct
        {
            get
            {
                var val = Item["IsProduct"];

                return !string.IsNullOrEmpty(val) && val != "0";
            }
        }

        public decimal? Quantity { get; set; }

        [Required]
        [Display(Name = "Gift Card Amount")]
        public decimal? GiftCardAmount { get; set; }

        public List<KeyValuePair<string, decimal?>> GiftCardAmountOptions { get; set; }

        public double StockCount { get; set; }

        public string StockAvailabilityDate { get; set; }

        public IEnumerable<VariantViewModel> Variants { get; protected set; }

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

        public string ProductUrl => ProductId.Equals(StorefrontManager.CurrentStorefront.GiftCardProductId, StringComparison.OrdinalIgnoreCase) ? StorefrontManager.StorefrontUri("/buygiftcard") : LinkManager.GetDynamicUrl(Item);

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

        public void Initialize(Rendering rendering, List<VariantViewModel> variants)
        {
            base.Initialize(rendering);
            Variants = variants;
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
            return ProductId.Equals(StorefrontManager.CurrentStorefront.GiftCardProductId, StringComparison.OrdinalIgnoreCase) ? StorefrontManager.StorefrontUri("/buygiftcard") : LinkManager.GetDynamicUrl(Item);
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
    }
}