//-----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the RouteConfig class.</summary>
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
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Feature.Orders.Website
{
    public static class RouteConfig
    {
        private static readonly List<ApiControllerMapping> _apiInfoList = new List<ApiControllerMapping>
        {
            new ApiControllerMapping("cart-addcartline", "Cart", "AddCartLine"),
            new ApiControllerMapping("cart-applydiscount", "Cart", "ApplyDiscount"),
            new ApiControllerMapping("cart-deletelineitem", "Cart", "DeleteLineItem"),
            new ApiControllerMapping("cart-getcurrentcart", "Cart", "GetCurrentCart"),
            new ApiControllerMapping("cart-removediscount", "Cart", "RemoveDiscount"),
            new ApiControllerMapping("cart-updatelineitem", "Cart", "UpdateLineItem"),
            new ApiControllerMapping("cart-updateminicart", "Cart", "UpdateMiniCart"),
            new ApiControllerMapping("checkout-getavailableregions", "Checkout", "GetAvailableRegions"),
            new ApiControllerMapping("checkout-getcheckoutdata", "Checkout", "GetCheckoutData"),
            new ApiControllerMapping("checkout-getshippingmethods", "Checkout", "GetShippingMethods"),
            new ApiControllerMapping("checkout-setshippingmethod", "Checkout", "SetShippingMethods"),
            new ApiControllerMapping("checkout-setpaymentmethod", "Checkout", "SetPaymentMethods"),
            new ApiControllerMapping("checkout-submitorder", "Checkout", "SubmitOrder"),
            new ApiControllerMapping("orders-recentorders", "Orders", "RecentOrders"),
            new ApiControllerMapping("orders-reorder", "Orders", "Reorder"),
            new ApiControllerMapping("orders-cancelorder", "Orders", "CancelOrder"),
            new ApiControllerMapping("checkout-updatemodel", "Checkout", "UpdateModel")
        };

        public static void RegisterRoutes(RouteCollection routes)
        {
            foreach (var apiInfo in _apiInfoList)
            {
                routes.MapRoute(
                    apiInfo.Name,
                    apiInfo.Url,
                    new {controller = apiInfo.Controller, action = apiInfo.Action, id = UrlParameter.Optional});
            }
        }
    }
}