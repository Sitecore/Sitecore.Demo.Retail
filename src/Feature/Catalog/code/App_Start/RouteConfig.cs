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
using Sitecore.Feature.Commerce.Catalog.Infrastructure.Pipelines;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Feature.Commerce.Catalog
{
    public static class RouteConfig
    {
        private static readonly List<ApiControllerMapping> _apiInfoList = new List<ApiControllerMapping>
        {
            new ApiControllerMapping("catalog-facetapplied", "Catalog", "FacetApplied"),
            new ApiControllerMapping("catalog-getproductstockinfo", "Catalog", "GetCurrentProductStockInfo"),
            new ApiControllerMapping("catalog-checkgiftcardbalance", "Catalog", "CheckGiftCardBalance"),
            new ApiControllerMapping("catalog-signupforbackinstocknotification", "Catalog", "SignUpForBackInStockNotification"),
            new ApiControllerMapping("catalog-sortorderapplied", "Catalog", "SortOrderApplied"),
            new ApiControllerMapping("catalog-switchcurrency", "Catalog", "SwitchCurrency")
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

            routes.MapRoute(
                "shop-category",
                ProductItemResolver.ShopUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = "category"});

            routes.MapRoute(
                "shop-product",
                ProductItemResolver.ShopUrlRoute + "/{category}/{id}",
                new {id = UrlParameter.Optional, itemType = "product"});

            routes.MapRoute(
                "shop-category-catalog",
                "{catalog}/" + ProductItemResolver.ShopUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = "category"});

            routes.MapRoute(
                "shop-product-catalog",
                "{catalog}/" + ProductItemResolver.ShopUrlRoute + "/{category}/{id}",
                new {id = UrlParameter.Optional, itemType = "product"});

            routes.MapRoute(
                "category",
                ProductItemResolver.CategoryUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = "category"});

            routes.MapRoute(
                "product",
                ProductItemResolver.ProductUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = "product"});

            routes.MapRoute(
                "ProductAction",
                ProductItemResolver.ProductUrlRoute + "/{action}/{id}",
                new {controller = "Catalog", id = UrlParameter.Optional, itemType = "product"});

            routes.MapRoute(
                "category-catalog",
                "{catalog}/" + ProductItemResolver.CategoryUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = "category"});

            routes.MapRoute(
                "product-catalog",
                "{catalog}/" + ProductItemResolver.ProductUrlRoute + "/{id}",
                new {id = UrlParameter.Optional, itemType = "product"});

            routes.MapRoute(
                "catalogitem-all",
                ProductItemResolver.NavigationItemName + "/{*pathElements}",
                new {itemType = "catalogitem"});
        }
    }
}