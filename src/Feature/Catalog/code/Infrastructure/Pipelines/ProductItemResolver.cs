//---------------------------------------------------------------------
// <copyright file="ProductItemResolver.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The product item resolver</summary>
//---------------------------------------------------------------------
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

using System.Web;
using System.Web.Routing;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Caching;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Catalog.Repositories;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Web;

namespace Sitecore.Feature.Commerce.Catalog.Infrastructure.Pipelines
{
    public class ProductItemResolver : HttpRequestProcessor, IProductResolver
    {
        public const string ProductUrlRoute = "product";
        public const string CategoryUrlRoute = "category";
        public const string ShopUrlRoute = "shop";

        public const string NavigationItemName = "product catalog";

        public ProductItemResolver(SiteContextRepository siteContextRepository, CatalogManager catalogManager)
        {
            SiteContextRepository = siteContextRepository;
            CatalogManager = catalogManager;
            CacheProvider = CommerceTypeLoader.GetCacheProvider(CommerceConstants.KnownCacheNames.FriendlyUrlsCache);
        }

        public SiteContextRepository SiteContextRepository { get; }
        private CatalogManager CatalogManager { get; }
        private ICacheProvider CacheProvider { get; }


        public Item ResolveCatalogItem(string itemId, string catalogName, bool isProduct)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            var id = GetFromCache(itemId, catalogName);
            if (!ID.IsNullOrEmpty(id))
            {
                return Context.Database == null ? null : Context.Database.GetItem(id);
            }

            var item = isProduct ? CatalogManager.GetProduct(itemId, catalogName) : CatalogManager.GetCategory(itemId, catalogName);
            AddToCache(itemId, catalogName, item);
            return item;
        }

        private void AddToCache(string itemId, string catalogName, Item foundItem)
        {
            CacheProvider.AddData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, GetCachekey(itemId, catalogName), foundItem != null ? foundItem.ID : ID.Undefined);
        }

        private ID GetFromCache(string itemId, string catalogName)
        {
            var id = CacheProvider.GetData<ID>(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, GetCachekey(itemId, catalogName));
            return id;
        }

        private static string GetCachekey(string itemId, string catalogName)
        {
            return "FriendlyUrl-" + itemId + "-" + catalogName;
        }

        public CatalogRouteData GetRouteDataValue(RouteData routeData)
        {
            var data = new CatalogRouteData();

            if (routeData.Values.ContainsKey("itemType"))
            {
                if (routeData.Values["itemType"].ToString() == "catalogitem")
                {
                    var currentStorefront = StorefrontManager.CurrentStorefront;
                    var productCatalogItem = currentStorefront.HomeItem.Axes.GetDescendant(NavigationItemName + "/" + routeData.Values["pathElements"]);
                    if (productCatalogItem != null)
                    {
                        data.IsProduct = productCatalogItem.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductTemplate);
                        data.Id = productCatalogItem.Name;
                    }
                }
                else
                {
                    data.IsProduct = routeData.Values["itemType"].ToString() == "product";
                }
            }
            else
            {
                return null;
            }

            if (routeData.Values.ContainsKey("id"))
            {
                data.Id = CatalogUrlRepository.ExtractItemId(routeData.Values["id"].ToString());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.Id))
                {
                    return null;
                }
            }

            if (routeData.Values.ContainsKey("catalog"))
            {
                data.Catalog = routeData.Values["catalog"].ToString();
            }

            if (string.IsNullOrEmpty(data.Catalog))
            {
                var defaultCatalog = CatalogManager.CurrentCatalog;

                if (defaultCatalog != null)
                {
                    data.Catalog = defaultCatalog.Name;
                }
            }

            if (routeData.Values.ContainsKey("category"))
            {
                SiteContextRepository.GetCurrent().UrlContainsCategory = true;
            }

            return data;
        }

        public CatalogRouteData GetCatalogItemFromIncomingRequest()
        {
            var httpContext = new HttpContextWrapper(SiteContextRepository.GetCurrent().CurrentContext);
            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            if (routeData == null)
            {
                return null;
            }
            var data = GetRouteDataValue(routeData);

            return data;
        }

        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item != null)
            {
                return;
            }

            var routeData = GetCatalogItemFromIncomingRequest();

            if (routeData == null)
            {
                return;
            }
            Context.Item = ResolveCatalogItem(routeData.Id, routeData.Catalog, routeData.IsProduct);
            SiteContextRepository.GetCurrent().CurrentCatalogItem = Context.Item;

            if (Context.Item == null)
            {
                WebUtil.Redirect("~/");
            }
        }

        public Item ResolveProductItem(string productId, string productCatalog)
        {
            return ResolveCatalogItem(productId, productCatalog, true);
        }

        public Item ResolveCategoryItem(string categoryId, string productCatalog)
        {
            return ResolveCatalogItem(categoryId, productCatalog, false);
        }
    }
}