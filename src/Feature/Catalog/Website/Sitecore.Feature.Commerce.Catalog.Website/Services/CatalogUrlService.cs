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
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Website.Models;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;

namespace Sitecore.Feature.Commerce.Catalog.Website.Services
{
    [Service]
    public class CatalogUrlService
    {
        private readonly string _urlTokenDelimiter = Settings.GetSetting("Storefront.UrlTokenDelimiter", "_");
        private readonly string _encodedDelimiter = Settings.GetSetting("Storefront.EncodedDelimiter", "%5f");

        public CatalogUrlService(CatalogManager catalogManager)
        {
            CatalogManager = catalogManager;
        }

        private bool IncludeLanguage
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

        public string GetProductCatalogUrl(Item productItem)
        {
            var productCatalogRootItem = CatalogManager.CatalogContext.CatalogRootItem;
            Assert.IsTrue(productCatalogRootItem != null, "CatalogManager.CatalogContext.CatalogRootItem must be set");

            var categoryDatasource = productCatalogRootItem[global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.NavigationItem.Fields.CategoryDatasource];
            Assert.IsNotNullOrEmpty(categoryDatasource, "the Catalog root has no CategoryDatasource.");

            var parentPath = productItem.Paths.FullPath;
            var path = parentPath.Replace(categoryDatasource, string.Empty);
            return GetSiteRelativeUrl(productCatalogRootItem, path);
        }

        private string GetSiteRelativeUrl(Item productCatalogRootItem, string path)
        {
            var catalogRootUrl = LinkManager.GetItemUrl(productCatalogRootItem);
            return string.Join("/", catalogRootUrl, path);
        }

        public string BuildUrl(Item item, bool includeCatalog, bool includeFriendlyName)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var catalogInfo = ExtractCatalogItemInfo(item);
            if (catalogInfo == null)
                return null;

            var url = BuildUrl(catalogInfo, includeCatalog, includeFriendlyName);
            return url;
        }

        public string BuildShopUrl(Item item, bool includeCatalog, bool includeFriendlyName)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var catalogInfo = ExtractCatalogItemInfo(item);
            if (catalogInfo == null)
                return null;

            var url = BuildShopUrl(catalogInfo, includeCatalog, includeFriendlyName);
            return url;
        }

        private string BuildUrl(CatalogItemInfo catalogInfo, bool includeCatalog, bool includeFriendlyName)
        {
            var route = new StringBuilder("/");

            if (IncludeLanguage)
            {
                route.Append(Context.Language.Name);
                route.Append("/");
            }

            if (includeCatalog)
            {
                route.Append(EncodeUrlToken(catalogInfo.CatalogName, true));
                route.Append("/");
            }

            string itemId;
            string itemName;
            switch (catalogInfo.ItemType)
            {
                case CatalogItemType.Category:
                    route.Append(RouteConfig.GetRootName(CatalogRouteRoot.Category));
                    itemId = catalogInfo.CategoryId;
                    itemName = catalogInfo.CategoryName;
                    break;
                case CatalogItemType.Product:
                case CatalogItemType.Variant:
                    route.Append(RouteConfig.GetRootName(CatalogRouteRoot.Product));
                    itemId = catalogInfo.ProductId;
                    itemName = catalogInfo.ProductName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            route.Append("/");

            if (includeFriendlyName)
            {
                route.Append(EncodeUrlToken(itemName, true));
                route.Append(_urlTokenDelimiter);
            }

            route.Append(EncodeUrlToken(itemId, false));

            var url = route.ToString();
            return url;
        }

        private string BuildShopUrl(CatalogItemInfo catalogItem, bool includeCatalog, bool includeFriendlyNames)
        {
            var route = new StringBuilder("/");

            if (IncludeLanguage)
            {
                route.Append(Context.Language.Name);
                route.Append("/");
            }

            if (includeCatalog)
            {
                route.Append(EncodeUrlToken(catalogItem.CatalogName, true));
                route.Append("/");
            }

            route.Append("shop/");

            if (includeFriendlyNames)
            {
                route.Append(EncodeUrlToken(catalogItem.CategoryName, true));
                route.Append(_urlTokenDelimiter);
            }

            route.Append(EncodeUrlToken(catalogItem.CategoryId, false));
            route.Append("/");

            if (catalogItem.ItemType == CatalogItemType.Product || catalogItem.ItemType == CatalogItemType.Variant)
            {
                if (includeFriendlyNames)
                {
                    // Replace plus (+) character with dash to prevent URL "Double Escaping" errors.
                    route.Append(EncodeUrlToken(catalogItem.ProductName.Replace("+","-"), true));
                    route.Append(_urlTokenDelimiter);
                }

                route.Append(EncodeUrlToken(catalogItem.ProductId, false));
                route.Append("/");

                if (catalogItem.ItemType == CatalogItemType.Variant)
                {
                    if (includeFriendlyNames)
                    {
                        route.Append(EncodeUrlToken(catalogItem.VariantName, true));
                        route.Append(_urlTokenDelimiter);
                    }

                    route.Append(EncodeUrlToken(catalogItem.VariantId, false));
                }
            }

            return route.ToString();
        }

        public string ExtractItemId(string folder)
        {
            var itemName = folder;
            if (folder != null && folder.Contains(_urlTokenDelimiter))
            {
                var tokens = folder.Split(new[] {_urlTokenDelimiter}, StringSplitOptions.None);
                itemName = tokens[tokens.Length - 1];
            }

            return DecodeUrlToken(itemName);
        }

        private string GetCatalogName(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            return item[global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Fields.CatalogName].ToLowerInvariant();
        }

        private CatalogItemInfo ExtractCatalogItemInfo(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            var info = new CatalogItemInfo {CatalogName = GetCatalogName(item)};

            Item variantItem = null;
            Item productItem = null;
            Item categoryItem;
            if (item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.ProductVariant.Id))
            {
                info.ItemType = CatalogItemType.Variant;
                variantItem = item;
                productItem = item.Parent;
                categoryItem = item.Parent.Parent;
            }
            else if (item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Product.Id))
            {
                info.ItemType = CatalogItemType.Product;
                productItem = item;
                categoryItem = item.Parent;
            }
            else if (item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Category.Id))
            {
                info.ItemType = CatalogItemType.Category;
                categoryItem = item;
            }
            else
            {
                return null;
            }

            if (categoryItem != null)
            {
                info.CategoryId = categoryItem.Name.ToLowerInvariant();
                info.CategoryName = categoryItem.DisplayName;
            }
            if (productItem != null)
            {
                info.ProductId = productItem.Name.ToLowerInvariant();
                info.ProductName = productItem.DisplayName;
            }
            if (variantItem != null)
            {
                info.VariantId = variantItem.Name.ToLowerInvariant();
                info.VariantName = variantItem.DisplayName;
            }

            return info;
        }

        private string EncodeUrlToken(string urlToken, bool removeInvalidPathCharacters)
        {
            if (string.IsNullOrEmpty(urlToken))
            {
                return null;
            }
            if (removeInvalidPathCharacters)
                urlToken = MainUtil.EncodeName(urlToken);

            return Uri.EscapeDataString(urlToken).Replace(_urlTokenDelimiter, _encodedDelimiter);
        }

        private string DecodeUrlToken(string urlToken)
        {
            if (string.IsNullOrEmpty(urlToken))
            {
                return null;
            }

            return Uri.UnescapeDataString(urlToken).Replace(_encodedDelimiter, _urlTokenDelimiter);
        }

        public CatalogManager CatalogManager { get; }

        private class CatalogItemInfo
        {
            public CatalogItemType ItemType { get; set; }
            public string CatalogName { get; set; }
            public string CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string VariantId { get; set; }
            public string VariantName { get; set; }
        }
    }
}