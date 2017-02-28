//-----------------------------------------------------------------------
// <copyright file="CartBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CartBaseJsonResult class.</summary>
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
using System.Linq;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Services;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class CartBaseJsonResult : BaseJsonResult
    {
        public CartBaseJsonResult()
        {
        }

        public CartBaseJsonResult(ServiceProviderResult result) : base(result)
        {
        }

        public bool IsPreview { get; set; }

        public List<CartLineBaseJsonResult> Lines { get; set; }

        public List<CartAdjustmentBaseJsonResult> Adjustments { get; set; }

        public string Subtotal { get; set; }

        public decimal SubtotalAmount { get; set; }

        public string TaxTotal { get; set; }

        public decimal TaxTotalAmount { get; set; }

        public string Total { get; set; }

        public decimal TotalAmount { get; set; }

        public string Discount { get; set; }

        public decimal DiscountAmount { get; set; }

        public string ShippingTotal { get; set; }

        public decimal ShippingTotalAmount { get; set; }

        public List<string> PromoCodes { get; set; }

        public virtual void Initialize(Cart cart)
        {
            Lines = new List<CartLineBaseJsonResult>();
            Adjustments = new List<CartAdjustmentBaseJsonResult>();
            PromoCodes = new List<string>();
            var currencyCode = StorefrontManager.CurrentStorefront.DefaultCurrency;

            Subtotal = 0.0M.ToCurrency(currencyCode);
            SubtotalAmount = 0.0M;
            TaxTotal = 0.0M.ToCurrency(currencyCode);
            TaxTotalAmount = 0.0M;
            Total = 0.0M.ToCurrency(currencyCode);
            TotalAmount = 0.0M;
            Discount = 0.0M.ToCurrency(currencyCode);
            DiscountAmount = 0.0M;
            ShippingTotal = 0.0M.ToCurrency(currencyCode);
            ShippingTotalAmount = 0.0M;

            if (cart == null)
            {
                return;
            }

            var catalogManager = DependencyResolver.Current.GetService<CatalogManager>();
            foreach (var line in cart.Lines ?? Enumerable.Empty<CartLine>())
            {
                var product = (CommerceCartProduct) line.Product;
                var productItem = catalogManager.GetProduct(product.ProductId, product.ProductCatalog);

                var cartLine = new CartLineBaseJsonResult(line, productItem);
                Lines.Add(cartLine);
            }

            foreach (var adjustment in cart.Adjustments ?? Enumerable.Empty<CartAdjustment>())
            {
                Adjustments.Add(new CartAdjustmentBaseJsonResult(adjustment));
            }

            var commerceTotal = (CommerceTotal) cart.Total;
            Subtotal = commerceTotal.Subtotal.ToCurrency(currencyCode);
            SubtotalAmount = commerceTotal.Subtotal;
            TaxTotal = cart.Total.TaxTotal.Amount.ToCurrency(currencyCode);
            TaxTotalAmount = cart.Total.TaxTotal.Amount;
            Total = cart.Total.Amount.ToCurrency(currencyCode);
            TotalAmount = cart.Total.Amount;
            Discount = commerceTotal.OrderLevelDiscountAmount.ToCurrency(currencyCode);
            DiscountAmount = commerceTotal.OrderLevelDiscountAmount;
            ShippingTotal = commerceTotal.ShippingTotal.ToCurrency(currencyCode);
            ShippingTotalAmount = commerceTotal.ShippingTotal;
        }
    }
}