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
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Pipelines;
using Sitecore.Web;

namespace Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines
{
    public class ProductItemResolver
    {
        public const string ShopProductRouteName = "shop-product";
        public const string ShopProductWithCatalogRouteName = "shop-product-catalog";
        public const string ShopCategoryRouteName = "shop-category";
        public const string ShopCategoryWithCatalogRouteName = "shop-category-catalog";
        public const string ProductRouteName = "product";
        public const string ProductWithCatalogRouteName = "product-catalog";
        public const string CategoryRouteName = "category";
        public const string CategoryWithCatalogRouteName = "category-catalog";
        public const string IdField = "id";
        public const string ItemTypeField = "itemType";
        public const string CatalogField = "catalog";
        public const string PathElementsField = "pathElements";
        public const string CategoryField = "category";
        public const string ProductItemType = "product";
        public const string CategoryItemType = "category";
        public const string CatalogItemType = "catalogitem";
        public const string NavigationItemName = "product catalog";
        public const string ProductUrlRoute = "product";
        public const string CategoryUrlRoute = "category";
        public const string ShopUrlRoute = "shop";
        public const string LandingUrlRoute = "landing";
        public const string BuyGiftCardUrlRoute = "buygiftcard";
        public static Item ResolveCatalogItem(string itemId, string catalogName, bool isProduct)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            var cachekey = "FriendlyUrl-" + itemId + "-" + catalogName;
            var cacheProvider = CommerceTypeLoader.GetCacheProvider(CommerceConstants.KnownCacheNames.FriendlyUrlsCache);
            var id = cacheProvider.GetData<ID>(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, cachekey);

            Item foundItem = null;
            if (ID.IsNullOrEmpty(id) || id == ID.Undefined)
            {
                foundItem = isProduct ? CatalogManager.GetProduct(itemId, catalogName) : CatalogManager.GetCategory(itemId, catalogName);

                cacheProvider.AddData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, cachekey, foundItem != null ? foundItem.ID : ID.Undefined);
            }
            else if (id != ID.Undefined && id != ID.Null)
            {
                foundItem = Context.Database.GetItem(id);
            }

            return foundItem;
        }

        public virtual CatalogRouteData GetRouteDataValue(RouteData routeData)
        {
            var data = new CatalogRouteData();

            if (routeData.Values.ContainsKey(ItemTypeField))
            {
                if (routeData.Values[ItemTypeField].ToString() == CatalogItemType)
                {
                    var currentStorefront = StorefrontManager.CurrentStorefront;
                    var productCatalogItem = currentStorefront.HomeItem.Axes.GetDescendant(NavigationItemName + "/" + routeData.Values[PathElementsField]);
                    if (productCatalogItem != null)
                    {
                        data.IsProduct =
                            productCatalogItem.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductTemplate);
                        data.Id = productCatalogItem.Name;
                    }
                }
                else
                {
                    data.IsProduct = routeData.Values[ItemTypeField].ToString() == ProductItemType;
                }
            }
            else
            {
                return null;
            }

            if (routeData.Values.ContainsKey(IdField))
            {
                data.Id = CatalogUrlManager.ExtractItemId(routeData.Values[IdField].ToString());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.Id))
                {
                    return null;
                }
            }

            if (routeData.Values.ContainsKey(CatalogField))
            {
                data.Catalog = routeData.Values[CatalogField].ToString();
            }

            if (string.IsNullOrEmpty(data.Catalog))
            {
                var defaultCatalog = StorefrontManager.CurrentStorefront.DefaultCatalog;

                if (defaultCatalog != null)
                {
                    data.Catalog = defaultCatalog.Name;
                }
            }

            if (routeData.Values.ContainsKey(CategoryField))
            {
                var siteContext = CommerceTypeLoader.CreateInstance<ISiteContext>();

                siteContext.UrlContainsCategory = true;
            }

            return data;
        }

        public virtual CatalogRouteData GetCatalogItemFromIncomingRequest()
        {
            var siteContext = CommerceTypeLoader.CreateInstance<ISiteContext>();
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(siteContext.CurrentContext));

            if (routeData != null)
            {
                var data = GetRouteDataValue(routeData);

                return data;
            }

            return null;
        }

        public virtual void Process(PipelineArgs args)
        {
            if (Context.Item != null)
            {
                return;
            }

            var routeData = GetCatalogItemFromIncomingRequest();

            if (routeData != null)
            {
                var siteContext = CommerceTypeLoader.CreateInstance<ISiteContext>();

                Context.Item = ResolveCatalogItem(routeData.Id, routeData.Catalog, routeData.IsProduct);
                siteContext.CurrentCatalogItem = Context.Item;

                if (Context.Item == null)
                {
                    WebUtil.Redirect("~/");
                }
            }
        }
    }
}