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

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Demo.Retail.Feature.Catalog.Website.Models;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website
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
                GetRootName(CatalogRouteRoot.Shop) + "/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Category) });

            routes.MapRoute(
                "shop-product",
                GetRootName(CatalogRouteRoot.Shop) + "/{category}/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Product) });

            routes.MapRoute(
                "shop-category-catalog",
                "{catalog}/" + GetRootName(CatalogRouteRoot.Shop) + "/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Category) });

            routes.MapRoute(
                "shop-product-catalog",
                "{catalog}/" + GetRootName(CatalogRouteRoot.Shop) + "/{category}/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Product) });

            routes.MapRoute(
                "category",
                GetRootName(CatalogRouteRoot.Category) + "/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Category) });

            routes.MapRoute(
                "product",
                GetRootName(CatalogRouteRoot.Product) + "/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Product) });

            routes.MapRoute(
                "ProductAction",
                GetRootName(CatalogRouteRoot.Product) + "/{action}/{id}",
                new {controller = "Catalog", id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Product) });

            routes.MapRoute(
                "category-catalog",
                "{catalog}/" + GetRootName(CatalogRouteRoot.Category) + "/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Category) });

            routes.MapRoute(
                "product-catalog",
                "{catalog}/" + GetRootName(CatalogRouteRoot.Product) + "/{id}",
                new {id = UrlParameter.Optional, itemType = TranslateItemType(RouteItemType.Product) });

            routes.MapRoute(
                "catalogitem-all",
                GetRootName(CatalogRouteRoot.Catalog) + "/{*catalogPath}",
                new {itemType = TranslateItemType(RouteItemType.CatalogItem)});
        }

        public static string GetRootName(CatalogRouteRoot root)
        {
            switch (root)
            {
                case CatalogRouteRoot.Catalog:
                    return "product catalog";
                case CatalogRouteRoot.Shop:
                    return "shop";
                case CatalogRouteRoot.Category:
                    return "category";
                case CatalogRouteRoot.Product:
                    return "product";
                default:
                    throw new ArgumentOutOfRangeException(nameof(root), root, null);
            }
        }

        public static string TranslateItemType(RouteItemType itemType)
        {
            return itemType.ToString("G").ToLower();
        }

        public static RouteItemType? GetItemType(RouteData route)
        {
            if (!route.Values.ContainsKey("itemType"))
                return null;
            var itemType = route.Values["itemType"].ToString();
            RouteItemType value;
            if (!Enum.TryParse(itemType, true, out value))
                return null;
            return value;
        }
    }
}