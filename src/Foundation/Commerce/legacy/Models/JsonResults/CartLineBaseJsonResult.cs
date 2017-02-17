//-----------------------------------------------------------------------
// <copyright file="CartLineBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CartLineBaseJsonResult class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class CartLineBaseJsonResult : BaseJsonResult
    {
        public CartLineBaseJsonResult(CommerceCartLineWithImages line)
        {
            var product = (CommerceCartProduct) line.Product;
            var productItem = ProductItemResolver.ResolveCatalogItem(product.ProductId, product.ProductCatalog, true);

            if (line.Images.Count > 0)
            {
                Image = line.Images[0] != null ? line.Images[0].ImageUrl(100, 100) : string.Empty;
            }

            var userCurrency = StorefrontManager.GetCustomerCurrency();

            DisplayName = product.DisplayName;
            Color = product.Properties["Color"] as string;
            LineDiscount = ((CommerceTotal) line.Total).LineItemDiscountAmount.ToCurrency(GetCurrencyCode(userCurrency, ((CommerceTotal) line.Total).CurrencyCode));
            Quantity = line.Quantity.ToString(Context.Language.CultureInfo);
            LinePrice = product.Price.Amount.ToCurrency(GetCurrencyCode(userCurrency, product.Price.CurrencyCode));
            LineTotal = line.Total.Amount.ToCurrency(GetCurrencyCode(userCurrency, line.Total.CurrencyCode));
            ExternalCartLineId = StringUtility.RemoveCurlyBrackets(line.ExternalCartLineId);
            ProductUrl = product.ProductId.Equals(StorefrontManager.CurrentStorefront.GiftCardProductId, StringComparison.OrdinalIgnoreCase) ? StorefrontManager.StorefrontUri("/buygiftcard") : LinkManager.GetDynamicUrl(productItem);

            DiscountOfferNames = line.Adjustments.Select(a => a.Description).ToList();
        }

        public string Image { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }
        public string LineDiscount { get; set; }
        public List<string> DiscountOfferNames { get; set; }
        public string Quantity { get; set; }
        public string LinePrice { get; set; }
        public string LineTotal { get; set; }
        public string ExternalCartLineId { get; set; }
        public string ProductUrl { get; set; }
        public IEnumerable<ShippingOptionBaseJsonResult> ShippingOptions { get; set; }

        public virtual void SetShippingOptions(IEnumerable<ShippingOption> shippingOptions)
        {
            if (shippingOptions == null)
            {
                return;
            }

            var shippingOptionList = new List<ShippingOptionBaseJsonResult>();

            foreach (var shippingOption in shippingOptions)
            {
                var jsonResult = CommerceTypeLoader.CreateInstance<ShippingOptionBaseJsonResult>();

                jsonResult.Initialize(shippingOption);
                shippingOptionList.Add(jsonResult);
            }

            ShippingOptions = shippingOptionList;
        }

        protected string GetCurrencyCode(string userCurrency, string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                return userCurrency;
            }

            return currency;
        }
    }
}