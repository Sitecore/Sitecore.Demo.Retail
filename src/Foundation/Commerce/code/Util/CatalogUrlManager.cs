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
                if (Context.Language != null)
                {
                    if (Context.Site == null ||
                        !string.Equals(Context.Language.Name, Context.Site.Language, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static string BuildProductCatalogLink(Item item)
        {
            var productCatalogItem =
                StorefrontManager.CurrentStorefront.HomeItem.Axes.GetChild(ProductItemResolver.NavigationItemName);
            var categoryDatasource = productCatalogItem["CategoryDatasource"];
            Assert.IsNotNullOrEmpty(categoryDatasource, "Product Catalog item missing CategoryDatasource.");
            var parentPath = item.Paths.FullPath;
            var path = parentPath.Replace(categoryDatasource, string.Empty);
            return $"/{ProductItemResolver.NavigationItemName}{path}";
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

            var isGiftCard = itemName == StorefrontManager.CurrentStorefront.GiftCardProductId;
            if (isGiftCard)
            {
                route.Append(ProductItemResolver.LandingUrlRoute);
                route.Append("/");
                route.Append(ProductItemResolver.BuyGiftCardUrlRoute);
            }
            else
            {
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
            }

            var url = StorefrontManager.StorefrontUri(route.ToString());
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

            var isGiftCard = productId == StorefrontManager.CurrentStorefront.GiftCardProductId;
            if (isGiftCard)
            {
                route.Append(ProductItemResolver.LandingUrlRoute);
                route.Append("/");
                route.Append(ProductItemResolver.BuyGiftCardUrlRoute);
            }
            else
            {
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
            }

            var url = StorefrontManager.StorefrontUri(route.ToString());
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

        public static string ExtractCatalogNameFromCurrentUrl()
        {
#pragma warning disable 618
            var linkProvider = LinkManager.Provider as CatalogLinkProvider;
#pragma warning restore 618

            if (linkProvider == null || !linkProvider.IncludeCatalog)
            {
                return StorefrontManager.CurrentStorefront.DefaultCatalog.Name;
            }
            var catalogName = WebUtil.GetUrlName(2);
            if (!string.IsNullOrEmpty(catalogName))
            {
                return catalogName;
            }

            catalogName = WebUtil.GetUrlName(1);
            return !string.IsNullOrEmpty(catalogName) ? catalogName : StorefrontManager.CurrentStorefront.DefaultCatalog.Name;
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
    }
}