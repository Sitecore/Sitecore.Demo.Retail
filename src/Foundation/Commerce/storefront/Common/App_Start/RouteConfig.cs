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
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines;
using Sitecore.Foundation.Commerce.Util;

namespace Sitecore.Reference.Storefront
{
    public static class RouteConfig
    {
        private static readonly List<ApiInfo> _apiInfoList = new List<ApiInfo>
        {
            new ApiInfo("account-getcurrentuser", "Account", "GetCurrentUser"),
            new ApiInfo("account-register", "Account", "Register"),
            new ApiInfo("account-addresslist", "Account", "AddressList"),
            new ApiInfo("account-recentorders", "Account", "RecentOrders"),
            new ApiInfo("account-reorder", "Account", "Reorder"),
            new ApiInfo("account-cancelorder", "Account", "CancelOrder"),
            new ApiInfo("account-addressdelete", "Account", "AddressDelete"),
            new ApiInfo("account-addressmodify", "Account", "AddressModify"),
            new ApiInfo("account-updateprofile", "Account", "UpdateProfile"),
            new ApiInfo("account-changepassword", "Account", "ChangePassword"),
            new ApiInfo("cart-addcartline", "Cart", "AddCartLine"),
            new ApiInfo("cart-applydiscount", "Cart", "ApplyDiscount"),
            new ApiInfo("cart-deletelineitem", "Cart", "DeleteLineItem"),
            new ApiInfo("cart-getcurrentcart", "Cart", "GetCurrentCart"),
            new ApiInfo("cart-removediscount", "Cart", "RemoveDiscount"),
            new ApiInfo("cart-updatelineitem", "Cart", "UpdateLineItem"),
            new ApiInfo("cart-updateminicart", "Cart", "UpdateMiniCart"),
            new ApiInfo("catalog-facetapplied", "Catalog", "FacetApplied"),
            new ApiInfo("catalog-getproductstockinfo", "Catalog", "GetCurrentProductStockInfo"),
            new ApiInfo("catalog-checkgiftcardbalance", "Catalog", "CheckGiftCardBalance"),
            new ApiInfo("catalog-signupforbackinstocknotification", "Catalog", "SignUpForBackInStockNotification"),
            new ApiInfo("catalog-sortorderapplied", "Catalog", "SortOrderApplied"),
            new ApiInfo("catalog-switchcurrency", "Catalog", "SwitchCurrency"),
            new ApiInfo("checkout-getavailablestates", "Checkout", "GetAvailableStates"),
            new ApiInfo("checkout-getcheckoutdata", "Checkout", "GetCheckoutData"),
            new ApiInfo("checkout-getshippingmethods", "Checkout", "GetShippingMethods"),
            new ApiInfo("checkout-setshippingmethod", "Checkout", "SetShippingMethods"),
            new ApiInfo("checkout-setpaymentmethod", "Checkout", "SetPaymentMethods"),
            new ApiInfo("checkout-submitorder", "Checkout", "SubmitOrder"),
            new ApiInfo("global-culturechosen", "Shared", "CultureChosen")
        };

        public static void RegisterRoutes(RouteCollection routes)
        {
            foreach (var apiInfo in _apiInfoList)
                routes.MapRoute(
                    apiInfo.Name,
                    apiInfo.Url,
                    new {controller = apiInfo.Controller, action = apiInfo.Action, id = UrlParameter.Optional});

            routes.MapRoute(
                ProductItemResolver.ShopCategoryRouteName,
                ProductItemResolver.ShopUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.CategoryItemType});

            routes.MapRoute(
                ProductItemResolver.ShopProductRouteName,
                ProductItemResolver.ShopUrlRoute + "/{category}/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.ProductItemType});

            routes.MapRoute(
                ProductItemResolver.ShopCategoryWithCatalogRouteName,
                "{catalog}/" + ProductItemResolver.ShopUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.CategoryItemType});

            routes.MapRoute(
                ProductItemResolver.ShopProductWithCatalogRouteName,
                "{catalog}/" + ProductItemResolver.ShopUrlRoute + "/{category}/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.ProductItemType});

            routes.MapRoute(
                ProductItemResolver.CategoryRouteName,
                ProductItemResolver.CategoryUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.CategoryItemType});

            routes.MapRoute(
                ProductItemResolver.ProductRouteName,
                ProductItemResolver.ProductUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.ProductItemType});

            routes.MapRoute(
                "ProductAction",
                ProductItemResolver.ProductUrlRoute + "/{action}/{id}",
                new {controller = "Catalog", id = UrlParameter.Optional, itemType = ProductItemResolver.ProductItemType});

            routes.MapRoute(
                ProductItemResolver.CategoryWithCatalogRouteName,
                "{catalog}/" + ProductItemResolver.CategoryUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.CategoryItemType});

            routes.MapRoute(
                ProductItemResolver.ProductWithCatalogRouteName,
                "{catalog}/" + ProductItemResolver.ProductUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = ProductItemResolver.ProductItemType});

            routes.MapRoute(
                "catalogitem-all",
                ProductItemResolver.NavigationItemName + "/{*pathElements}",
                new {itemType = ProductItemResolver.CatalogItemType});

            routes.MapRoute(
                "logoff",
                "logoff",
                new {controller = "Account", action = "LogOff", storefront = UrlParameter.Optional}
            );
        }
        private class ApiInfo
        {
            public ApiInfo(string name, string controller, string action)
            {
                this.Name = name;
                this.Controller = controller;
                this.Action = action;
            }

            public string Name { get; private set; }

            public string Controller { get; private set; }

            public string Action { get; private set; }

            public string Url => "api/storefront/" + (this.Controller.ToLower(CultureInfo.InvariantCulture) + "/" + this.Action.ToLower(CultureInfo.InvariantCulture));
        }

    }
}