//-----------------------------------------------------------------------
// <copyright file="CartModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CartModel class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Services;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
    public class CartApiModel : BaseApiModel
    {
        public CartApiModel()
        {
        }

        public CartApiModel(ServiceProviderResult result) : base(result)
        {
        }

        public bool IsPreview { get; set; }

        public List<CartLineApiModel> Lines { get; set; }

        public List<CartAdjustmentApiModel> Adjustments { get; set; }

        public string Subtotal { get; set; }

        public decimal SubtotalAmount { get; set; }

        public string TaxTotal { get; set; }

        public decimal TaxTotalAmount { get; set; }

        public string Total { get; set; }

        public decimal TotalAmount { get; set; }

        public string Discount { get; set; }

        public decimal DiscountAmount { get; set; }

        public string TotalDiscount { get; set; }

        public decimal TotalDiscountAmount { get; set; }

        public string ShippingTotal { get; set; }

        public decimal ShippingTotalAmount { get; set; }

        public bool HasShipping { get; set; }
        public bool HasTaxes { get; set; }

        public List<string> PromoCodes { get; set; }

        public virtual void Initialize(Cart cart)
        {
            Lines = new List<CartLineApiModel>();
            Adjustments = new List<CartAdjustmentApiModel>();
            PromoCodes = new List<string>();

            Subtotal = 0.0M.ToCurrency();
            SubtotalAmount = 0.0M;
            TaxTotal = 0.0M.ToCurrency();
            TaxTotalAmount = 0.0M;
            Total = 0.0M.ToCurrency();
            TotalAmount = 0.0M;
            Discount = 0.0M.ToCurrency();
            DiscountAmount = 0.0M;
            ShippingTotal = 0.0M.ToCurrency();
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

                var cartLine = new CartLineApiModel(line, productItem);
                Lines.Add(cartLine);
            }

            foreach (var adjustment in cart.Adjustments ?? Enumerable.Empty<CartAdjustment>())
            {
                Adjustments.Add(new CartAdjustmentApiModel(adjustment));
            }

            var commerceTotal = (CommerceTotal) cart.Total;
            Subtotal = commerceTotal.Subtotal.ToCurrency();
            SubtotalAmount = commerceTotal.Subtotal;
            TaxTotal = cart.Total.TaxTotal.Amount.ToCurrency();
            TaxTotalAmount = cart.Total.TaxTotal.Amount;
            Total = cart.Total.Amount.ToCurrency();
            TotalAmount = cart.Total.Amount;
            Discount = commerceTotal.OrderLevelDiscountAmount.ToCurrency();
            DiscountAmount = commerceTotal.OrderLevelDiscountAmount;
            ShippingTotal = commerceTotal.ShippingTotal.ToCurrency();
            ShippingTotalAmount = commerceTotal.ShippingTotal;
            HasShipping = cart.Shipping != null && cart.Shipping.Any();
            HasTaxes = cart.Total.TaxTotal.TaxSubtotals?.Any() ?? false;
            var totalSavings = cart.Lines?.Sum(lineitem => ((CommerceTotal)lineitem.Total).LineItemDiscountAmount) ?? 0;
            totalSavings += ((CommerceTotal)cart.Total).OrderLevelDiscountAmount;
            TotalDiscountAmount = totalSavings;
            TotalDiscount = totalSavings.ToCurrency();

            var commerceCart = cart as CommerceCart;
            if (commerceCart?.OrderForms.Count > 0)
            {
                foreach (var promoCode in commerceCart.OrderForms[0].PromoCodes ?? Enumerable.Empty<string>())
                {
                    PromoCodes.Add(promoCode);
                }
            }

        }
    }
}