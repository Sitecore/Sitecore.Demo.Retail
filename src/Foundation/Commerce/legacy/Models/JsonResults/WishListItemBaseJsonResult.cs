//-----------------------------------------------------------------------
// <copyright file="WishListItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the WishListItemBaseJsonResult class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class WishListItemBaseJsonResult
    {
        public WishListItemBaseJsonResult(WishListLine line, string wishListId)
        {
            Assert.ArgumentNotNull(line, nameof(line));
            Assert.ArgumentNotNullOrEmpty(wishListId, nameof(wishListId));

            var product = (CommerceCartProduct) line.Product;
            var productItem = StorefrontManager.ProductResolver.ResolveProductItem(product.ProductId, product.ProductCatalog);

            var currencyCode = StorefrontManager.CurrentStorefront.DefaultCurrency;

            DisplayName = product.DisplayName;
            Color = product.Properties["Color"] as string;
            LineDiscount = ((CommerceTotal) line.Total).LineItemDiscountAmount.ToString(Context.Language.CultureInfo);
            Quantity = line.Quantity.ToString(Context.Language.CultureInfo);
            LineTotal = line.Total.Amount.ToCurrency(currencyCode);
            ExternalLineId = line.ExternalId;
            ProductId = product.ProductId;
            VariantId = product.ProductVariantId;
            ProductCatalog = product.ProductCatalog;
            WishListId = wishListId;
            ProductUrl = LinkManager.GetDynamicUrl(productItem);

            if (product.Price.Amount != 0M)
                LinePrice = product.Price.Amount.ToCurrency(currencyCode);

            var imageInfo = product.Properties["_product_Images"] as string;
            if (imageInfo == null)
            {
                return;
            }
            var imageId = imageInfo.Split('|')[0];
            MediaItem mediaItem = Context.Database.GetItem(imageId);
            Image = mediaItem != null ? mediaItem.ImageUrl(100, 100) : string.Empty;
        }

        public string Image { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }
        public string LineDiscount { get; set; }
        public string Quantity { get; set; }
        public string LinePrice { get; set; }
        public string LineTotal { get; set; }
        public string ExternalLineId { get; set; }
        public string ProductUrl { get; set; }
        public string ProductId { get; set; }
        public string VariantId { get; set; }
        public string ProductCatalog { get; set; }
        public string WishListId { get; set; }
    }
}