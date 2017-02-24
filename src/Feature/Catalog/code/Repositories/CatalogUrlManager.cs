//---------------------------------------------------------------------
// <copyright file="CatalogUrlManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The CatalogUrlManager class</summary>
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

using System;
using System.Text;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Infrastructure.Pipelines;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Web;

namespace Sitecore.Feature.Commerce.Catalog.Repositories
{
    public static class CatalogUrlRepository
    {
        private static readonly string _urlTokenDelimiter = Settings.GetSetting("Storefront.UrlTokenDelimiter", "_");
        private static readonly string _encodedDelimiter = Settings.GetSetting("Storefront.EncodedDelimiter", "[[_]]");

        private static readonly string[] _invalidPathCharacters =
        {
            "<", ">", "*", "%", "&", ":", "\\", "?", ".", "\"",
            " "
        };

        private static readonly Lazy<ICommerceSearchManager> _searchManagerLoader = new Lazy<ICommerceSearchManager>(DependencyResolver.Current.GetService<ICommerceSearchManager>);

        private static bool IncludeLanguage
        {
            get
            {
                if (Context.Language == null)
                {
                    return false;
                }
                return Context.Site == null || !String.Equals(Context.Language.Name, Context.Site.Language, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string BuildProductCatalogLink(Item productItem)
        {
            return GetProductCatalogUrl(productItem);
        }

        public static string BuildProductLink(Item item, bool includeCatalog, bool includeFriendlyName)
        {
            var url = BuildUrl(item, includeCatalog, includeFriendlyName, ProductItemResolver.ProductUrlRoute);
            return url;
        }

        public static string BuildCategoryLink(Item item, bool includeCatalog, bool includeFriendlyName)
        {
            var url = BuildUrl(item, includeCatalog, includeFriendlyName, ProductItemResolver.CategoryUrlRoute);
            return url;
        }

        public static string BuildUrl(Item item, bool includeCatalog, bool includeFriendlyName, string root)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            string itemFriendlyName;
            string itemName;
            var catalogName = ExtractCatalogName(item, includeCatalog);

            ExtractCatalogItemInfo(item, includeFriendlyName, out itemName, out itemFriendlyName);

            var url = BuildUrl(itemName, itemFriendlyName, catalogName, root);
            return url;
        }

        public static string BuildUrl(string itemName, string itemFriendlyName, string catalogName, string root)
        {
            Assert.ArgumentNotNullOrEmpty(itemName, nameof(itemName));

            var route = new StringBuilder("/");

            if (IncludeLanguage)
            {
                route.Append(Context.Language.Name);
                route.Append("/");
            }

            if (!String.IsNullOrEmpty(catalogName))
            {
                route.Append(EncodeUrlToken(catalogName, true));
                route.Append("/");
            }

            route.Append(root);
            route.Append("/");

            if (!String.IsNullOrEmpty(itemFriendlyName))
            {
                route.Append(EncodeUrlToken(itemFriendlyName, true));
                route.Append(_urlTokenDelimiter);
            }

            route.Append(EncodeUrlToken(itemName, false));

            var url = route.ToString();
            return url;
        }

        public static string BuildVariantShopLink(Item item, bool includeCatalog, bool includeFriendlyName,
            bool includeCurrentCategory)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            string variantName;
            string variantId;
            string productName;
            string productId;
            var categoryName = String.Empty;
            var categoryId = String.Empty;
            var catalogName = ExtractCatalogName(item, includeCatalog);

            ExtractCatalogItemInfo(item, includeFriendlyName, out variantId, out variantName);

            var parentItem = item.Parent;
            ExtractCatalogItemInfo(parentItem, includeFriendlyName, out productId, out productName);

            if (includeCurrentCategory)
            {
                ExtractCategoryInfoFromCurrentShopUrl(out categoryId, out categoryName);
            }

            if (String.IsNullOrEmpty(categoryId))
            {
                var grandParentItem = parentItem.Parent;
                ExtractCatalogItemInfo(grandParentItem, includeFriendlyName, out categoryId, out categoryName);
            }

            var url = BuildShopUrl(categoryId, categoryName, productId, productName, variantId, variantName, catalogName);
            return url;
        }

        public static string BuildProductShopLink(Item item, bool includeCatalog, bool includeFriendlyName,
            bool includeCurrentCategory)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            string productName;
            string productId;
            var categoryName = String.Empty;
            var categoryId = String.Empty;
            var catalogName = ExtractCatalogName(item, includeCatalog);

            ExtractCatalogItemInfo(item, includeFriendlyName, out productId, out productName);

            if (includeCurrentCategory)
            {
                ExtractCategoryInfoFromCurrentShopUrl(out categoryId, out categoryName);
            }

            if (String.IsNullOrEmpty(categoryId))
            {
                var parentItem = item.Parent;
                ExtractCatalogItemInfo(parentItem, includeFriendlyName, out categoryId, out categoryName);
            }

            var url = BuildShopUrl(categoryId, categoryName, productId, productName, String.Empty, String.Empty,
                catalogName);
            return url;
        }

        public static string BuildCategoryShopLink(Item item, bool includeCatalog, bool includeFriendlyName)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            string categoryName;
            string categoryId;
            var catalogName = ExtractCatalogName(item, includeCatalog);

            ExtractCatalogItemInfo(item, includeFriendlyName, out categoryId, out categoryName);

            var url = BuildShopUrl(categoryId, categoryName, String.Empty, String.Empty, String.Empty, String.Empty,
                catalogName);
            return url;
        }

        public static string BuildShopUrl(string categoryId, string categoryName, string productId, string productName,
            string variantId, string variantName, string catalogName)
        {
            Assert.ArgumentNotNullOrEmpty(categoryId, nameof(categoryId));

            var route = new StringBuilder("/");

            if (IncludeLanguage)
            {
                route.Append(Context.Language.Name);
                route.Append("/");
            }

            if (!String.IsNullOrEmpty(catalogName))
            {
                route.Append(EncodeUrlToken(catalogName, true));
                route.Append("/");
            }

            route.Append(ProductItemResolver.ShopUrlRoute);
            route.Append("/");

            if (!String.IsNullOrEmpty(categoryName))
            {
                route.Append(EncodeUrlToken(categoryName, true));
                route.Append(_urlTokenDelimiter);
            }

            route.Append(EncodeUrlToken(categoryId, false));

            if (!String.IsNullOrEmpty(productId))
            {
                route.Append("/");

                if (!String.IsNullOrEmpty(productName))
                {
                    route.Append(EncodeUrlToken(productName, true));
                    route.Append(_urlTokenDelimiter);
                }

                route.Append(EncodeUrlToken(productId, false));

                if (!String.IsNullOrEmpty(variantId))
                {
                    route.Append("/");

                    if (!String.IsNullOrEmpty(variantName))
                    {
                        route.Append(EncodeUrlToken(variantName, true));
                        route.Append(_urlTokenDelimiter);
                    }

                    route.Append(EncodeUrlToken(variantId, false));
                }
            }

            var url = route.ToString();
            return url;
        }

        public static string ExtractItemIdFromCurrentUrl()
        {
            return ExtractItemId(WebUtil.GetUrlName(0));
        }

        public static string ExtractCategoryNameFromCurrentUrl()
        {
            var categoryFolder = WebUtil.GetUrlName(1);
            return String.IsNullOrEmpty(categoryFolder) ? ExtractItemIdFromCurrentUrl() : ExtractItemId(categoryFolder);
        }

        public static string ExtractItemId(string folder)
        {
            var itemName = folder;
            if (folder != null && folder.Contains(_urlTokenDelimiter))
            {
                var tokens = folder.Split(new[] {_urlTokenDelimiter}, StringSplitOptions.None);
                itemName = tokens[tokens.Length - 1];
            }

            return DecodeUrlToken(itemName);
        }

        public static string ExtractItemName(string folder)
        {
            var itemName = String.Empty;
            if (folder != null && folder.Contains(_urlTokenDelimiter))
            {
                var tokens = folder.Split(new[] {_urlTokenDelimiter}, StringSplitOptions.None);
                itemName = tokens[tokens.Length - 2];
            }

            return DecodeUrlToken(itemName);
        }

        private static void ExtractCatalogItemInfo(string folder, out string itemId, out string itemName)
        {
            itemId = ExtractItemId(folder);
            itemName = ExtractItemName(folder);
        }

        private static void ExtractCategoryInfoFromCurrentShopUrl(out string categoryId, out string categoryName)
        {
            categoryId = String.Empty;
            categoryName = String.Empty;

            if (WebUtil.GetUrlName(1).ToLowerInvariant() == ProductItemResolver.ShopUrlRoute)
            {
                ExtractCatalogItemInfo(WebUtil.GetUrlName(0), out categoryId, out categoryName);
            }

            if (WebUtil.GetUrlName(2).ToLowerInvariant() == ProductItemResolver.ShopUrlRoute)
            {
                ExtractCatalogItemInfo(WebUtil.GetUrlName(1), out categoryId, out categoryName);
            }

            if (WebUtil.GetUrlName(3).ToLowerInvariant() == ProductItemResolver.ShopUrlRoute)
            {
                ExtractCatalogItemInfo(WebUtil.GetUrlName(2), out categoryId, out categoryName);
            }
        }

        private static string ExtractCatalogName(Item item, bool includeCatalog)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            return includeCatalog ? item[CommerceConstants.KnownFieldIds.CatalogName].ToLowerInvariant() : string.Empty;
        }

        private static void ExtractCatalogItemInfo(Item item, bool includeFriendlyName, out string itemName, out string itemFriendlyName)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            if (_searchManagerLoader.Value.IsItemCatalog(item) || _searchManagerLoader.Value.IsItemVirtualCatalog(item))
            {
                itemName = ProductItemResolver.ProductUrlRoute;
                itemFriendlyName = string.Empty;
            }
            else
            {
                itemName = item.Name.ToLowerInvariant();
                itemFriendlyName = string.Empty;
                if (includeFriendlyName)
                {
                    itemFriendlyName = item.DisplayName;
                }
            }
        }

        private static string EncodeUrlToken(string urlToken, bool removeInvalidPathCharacters)
        {
            if (String.IsNullOrEmpty(urlToken))
            {
                return null;
            }

            if (removeInvalidPathCharacters)
            {
                foreach (var character in _invalidPathCharacters)
                {
                    urlToken = urlToken.Replace(character, String.Empty);
                }
            }

            return Uri.EscapeDataString(urlToken).Replace(_urlTokenDelimiter, _encodedDelimiter);
        }

        private static string DecodeUrlToken(string urlToken)
        {
            return string.IsNullOrEmpty(urlToken) ? null : Uri.UnescapeDataString(urlToken).Replace(_encodedDelimiter, _urlTokenDelimiter);
        }

        public static string GetProductCatalogUrl(Item productItem)
        {
            var productCatalogItem = GetProductCatalogRoot();
            Assert.IsTrue(productCatalogItem != null && !productCatalogItem.IsDerived(Foundation.Commerce.Templates.Commerce.NavigationItem.ID), "Product Catalog item must be a Commerce Navigation Item");
            var categoryDatasource = productCatalogItem[Foundation.Commerce.Templates.Commerce.NavigationItem.Fields.CategoryDatasource];
            Assert.IsNotNullOrEmpty(categoryDatasource, "Product Catalog item missing CategoryDatasource.");
            var parentPath = productItem.Paths.FullPath;
            var path = parentPath.Replace(categoryDatasource, string.Empty);
            return $"/{ProductItemResolver.NavigationItemName}{path}";
        }

        public static Item GetProductCatalogRoot()
        {
            return StorefrontManager.CurrentStorefront.HomeItem.Axes.GetChild(ProductItemResolver.NavigationItemName);
        }

        public static bool IsProductCategoryUrl(string url)
        {
            return url.IndexOf(ProductItemResolver.NavigationItemName, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}