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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Links;
using Sitecore.Web;

namespace Sitecore.Foundation.Commerce.Util
{
    public static class CatalogUrlManager
    {
        private static readonly string _urlTokenDelimiter = Settings.GetSetting("Storefront.UrlTokenDelimiter", "_");
        private static readonly string _encodedDelimiter = Settings.GetSetting("Storefront.EncodedDelimiter", "[[_]]");

        private static readonly string[] _invalidPathCharacters =
        {
            "<", ">", "*", "%", "&", ":", "\\", "?", ".", "\"",
            " "
        };

        private static readonly Lazy<ICommerceSearchManager> _searchManagerLoader = new Lazy<ICommerceSearchManager>(CommerceTypeLoader.CreateInstance<ICommerceSearchManager>);

        private static bool IncludeLanguage
        {
            get
            {
                if (Context.Language == null)
                {
                    return false;
                }
                return Context.Site == null || !string.Equals(Context.Language.Name, Context.Site.Language, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string BuildProductCatalogLink(Item productItem)
        {
            return ProductItemResolver.GetProductCatalogUrl(productItem);
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

            if (!string.IsNullOrEmpty(catalogName))
            {
                route.Append(EncodeUrlToken(catalogName, true));
                route.Append("/");
            }

            route.Append(root);
            route.Append("/");

            if (!string.IsNullOrEmpty(itemFriendlyName))
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
            var categoryName = string.Empty;
            var categoryId = string.Empty;
            var catalogName = ExtractCatalogName(item, includeCatalog);

            ExtractCatalogItemInfo(item, includeFriendlyName, out variantId, out variantName);

            var parentItem = item.Parent;
            ExtractCatalogItemInfo(parentItem, includeFriendlyName, out productId, out productName);

            if (includeCurrentCategory)
            {
                ExtractCategoryInfoFromCurrentShopUrl(out categoryId, out categoryName);
            }

            if (string.IsNullOrEmpty(categoryId))
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
            var categoryName = string.Empty;
            var categoryId = string.Empty;
            var catalogName = ExtractCatalogName(item, includeCatalog);

            ExtractCatalogItemInfo(item, includeFriendlyName, out productId, out productName);

            if (includeCurrentCategory)
            {
                ExtractCategoryInfoFromCurrentShopUrl(out categoryId, out categoryName);
            }

            if (string.IsNullOrEmpty(categoryId))
            {
                var parentItem = item.Parent;
                ExtractCatalogItemInfo(parentItem, includeFriendlyName, out categoryId, out categoryName);
            }

            var url = BuildShopUrl(categoryId, categoryName, productId, productName, string.Empty, string.Empty,
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

            var url = BuildShopUrl(categoryId, categoryName, string.Empty, string.Empty, string.Empty, string.Empty,
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

            if (!string.IsNullOrEmpty(catalogName))
            {
                route.Append(EncodeUrlToken(catalogName, true));
                route.Append("/");
            }

            route.Append(ProductItemResolver.ShopUrlRoute);
            route.Append("/");

            if (!string.IsNullOrEmpty(categoryName))
            {
                route.Append(EncodeUrlToken(categoryName, true));
                route.Append(_urlTokenDelimiter);
            }

            route.Append(EncodeUrlToken(categoryId, false));

            if (!string.IsNullOrEmpty(productId))
            {
                route.Append("/");

                if (!string.IsNullOrEmpty(productName))
                {
                    route.Append(EncodeUrlToken(productName, true));
                    route.Append(_urlTokenDelimiter);
                }

                route.Append(EncodeUrlToken(productId, false));

                if (!string.IsNullOrEmpty(variantId))
                {
                    route.Append("/");

                    if (!string.IsNullOrEmpty(variantName))
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
            return string.IsNullOrEmpty(categoryFolder) ? ExtractItemIdFromCurrentUrl() : ExtractItemId(categoryFolder);
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
            var itemName = string.Empty;
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
            categoryId = string.Empty;
            categoryName = string.Empty;

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
            if (string.IsNullOrEmpty(urlToken))
            {
                return null;
            }

            if (removeInvalidPathCharacters)
            {
                foreach (var character in _invalidPathCharacters)
                {
                    urlToken = urlToken.Replace(character, string.Empty);
                }
            }

            return Uri.EscapeDataString(urlToken).Replace(_urlTokenDelimiter, _encodedDelimiter);
        }

        private static string DecodeUrlToken(string urlToken)
        {
            return string.IsNullOrEmpty(urlToken) ? null : Uri.UnescapeDataString(urlToken).Replace(_encodedDelimiter, _urlTokenDelimiter);
        }

        public static string GetAdjustedProductUrl(Item item)
        {
            if (!item.Paths.FullPath.StartsWith("/sitecore/Commerce/Catalog Management/Catalogs/"))
                return null;

            string url = null;
            var category = GetCategory(item);
            if (category != null)
                url = $"/shop/{category.Name}_{category.Name}/{item["__Display name"]}_{item.Name}";
            return url;
        }

        private static Item GetCategory(Item item)
        {
            if (item == null)
                return null;

            var origItem = item;
            var catelog = ProductItemResolver.GetProductCatalogRoot();

            // Go up the tree until you find the item whose parent is the catelog.
            // That item is the category.
            while (item.Parent.Paths.FullPath != "/sitecore")
            {
                item = item.Parent;
                if (item.Parent.ID == catelog.ID)
                    return item;
            }

            // At this point, the Sitecore.Context.Item is duplicate item found outside
            // the catelog.  Looks like we have to do things the hard / wrong way...
            foreach (Item category in catelog.Children)
                foreach (Item product in category.Children)
                    if (product.ID == origItem.ID)
                        return category;

            return null;
        }

    }
}