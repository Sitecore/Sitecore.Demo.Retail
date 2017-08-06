//-----------------------------------------------------------------------
// <copyright file="CartLineModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CartLineModel class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.Commerce.Website.Util;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;

namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
    public class CartLineApiModel : BaseApiModel
    {
        public CartLineApiModel(CartLine line, Item productItem)
        {
            var product = (CommerceCartProduct) line.Product;

            Title = product.DisplayName;
            Color = product.Properties["Color"] as string;
            var total = (CommerceTotal) line.Total;
            LineDiscount = total.LineItemDiscountAmount.ToCurrency(total.CurrencyCode);
            Quantity = line.Quantity.ToString(Context.Language.CultureInfo);
            LinePrice = product.Price.Amount.ToCurrency(product.Price.CurrencyCode);
            LineTotal = line.Total.Amount.ToCurrency(line.Total.CurrencyCode);
            ExternalCartLineId = StringUtility.RemoveCurlyBrackets(line.ExternalCartLineId);
            ProductUrl = LinkManager.GetDynamicUrl(productItem);
            Image = (line as CommerceCartLineWithImages)?.DefaultImage?.ImageUrl(150, 150) ?? string.Empty;

            DiscountOfferNames = line.Adjustments.Select(a => a.Description).ToList();
        }

        public string Image { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string LineDiscount { get; set; }
        public List<string> DiscountOfferNames { get; set; }
        public string Quantity { get; set; }
        public string LinePrice { get; set; }
        public string LineTotal { get; set; }
        public string ExternalCartLineId { get; set; }
        public string ProductUrl { get; set; }
        public IEnumerable<ShippingOptionApiModel> ShippingOptions { get; set; }

        public void SetShippingOptions(IEnumerable<ShippingOption> shippingOptions)
        {
            if (shippingOptions == null)
            {
                return;
            }

            var shippingOptionList = new List<ShippingOptionApiModel>();

            foreach (var shippingOption in shippingOptions)
            {
                var jsonResult = new ShippingOptionApiModel();

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