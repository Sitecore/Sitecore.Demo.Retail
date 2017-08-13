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

using System.Collections.Specialized;
using System.Web.Mvc;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Website.Services;
using Sitecore.Links;

namespace Sitecore.Feature.Commerce.Catalog.Website.Infrastructure.Provider
{
    public class CatalogLinkProvider : LinkProvider
    {
        public const string IncludeCatalogsAttribute = "includeCatalog";

        public const string UseShopLinksAttribute = "useShopLinks";

        public const string IncludeFriendlyNameAttribute = "includeFriendlyName";

        private const bool IncludeCatalogsDefault = false;

        private const bool UseShopLinksDefault = true;

        private const bool IncludeFriendlyNameDefault = true;

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

            var urlService = DependencyResolver.Current.GetService<CatalogUrlService>();

            //TODO: Incorporate the url options in the catalog URL building
            if (Context.PageMode.IsExperienceEditor)
                return urlService.GetProductCatalogUrl(item);

            var url = UseShopLinks ? urlService.BuildShopUrl(item, IncludeCatalog, IncludeFriendlyName) : urlService.BuildUrl(item, IncludeCatalog, IncludeFriendlyName);

            if (string.IsNullOrEmpty(url))
            {
                url = base.GetDynamicUrl(item, options);
            }

            return url;
        }
    }
}