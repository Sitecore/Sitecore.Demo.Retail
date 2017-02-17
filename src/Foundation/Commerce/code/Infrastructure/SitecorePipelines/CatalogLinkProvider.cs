//---------------------------------------------------------------------
// <copyright file="CatalogLinkProvider.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The catelog link provider</summary>
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
using System.Collections.Specialized;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;
using Sitecore.Web;

namespace Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines
{
    public class CatalogLinkProvider : LinkProvider
    {
        public const string IncludeCatalogsAttribute = "includeCatalog";

        public const string UseShopLinksAttribute = "useShopLinks";

        public const string IncludeFriendlyNameAttribute = "includeFriendlyName";

        public const bool IncludeCatalogsDefault = false;

        public const bool UseShopLinksDefault = true;

        public const bool IncludeFriendlyNameDefault = true;

        public bool IncludeCatalog { get; set; }

        public bool UseShopLinks { get; set; }

        public bool IncludeFriendlyName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            IncludeCatalog = MainUtil.GetBool(config[IncludeCatalogsAttribute], IncludeCatalogsDefault);
            UseShopLinks = MainUtil.GetBool(config[UseShopLinksAttribute], UseShopLinksDefault);
            IncludeFriendlyName = MainUtil.GetBool(config[IncludeFriendlyNameAttribute], IncludeFriendlyNameDefault);
        }

        public override string GetDynamicUrl(Item item, LinkUrlOptions options)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(options, nameof(options));

            var url = string.Empty;

            var productCatalogLinkRequired =
                WebUtil.GetRawUrl().IndexOf(ProductItemResolver.NavigationItemName, StringComparison.OrdinalIgnoreCase) >=
                0;
            if (productCatalogLinkRequired)
            {
                url = CatalogUrlManager.BuildProductCatalogLink(item);
            }
            else if (UseShopLinks)
            {
                if (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductTemplate))
                {
                    url = CatalogUrlManager.BuildProductShopLink(item, IncludeCatalog, IncludeFriendlyName, true);
                }
                else if (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceCategoryTemplate))
                {
                    url = CatalogUrlManager.BuildCategoryShopLink(item, IncludeCatalog, IncludeFriendlyName);
                }
                else if (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductVariantTemplate))
                {
                    url = CatalogUrlManager.BuildVariantShopLink(item, IncludeCatalog, IncludeFriendlyName, true);
                }
            }
            else
            {
                if (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductTemplate))
                {
                    url = CatalogUrlManager.BuildProductLink(item, IncludeCatalog, IncludeFriendlyName);
                }
                else if (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceCategoryTemplate))
                {
                    url = CatalogUrlManager.BuildCategoryLink(item, IncludeCatalog, IncludeFriendlyName);
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                url = base.GetDynamicUrl(item, options);
            }

            return url;
        }
    }
}