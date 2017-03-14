//---------------------------------------------------------------------
// <copyright file="ProductItemRedirector.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The product item redirector</summary>
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
using System.Web;
using System.Web.Routing;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Caching;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Catalog.Factories;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Web;
using System.Web.Mvc;
using Sitecore.Feature.Commerce.Catalog.Services;

namespace Sitecore.Feature.Commerce.Catalog.Infrastructure.Pipelines
{
    public class ProductItemRedirector : HttpRequestProcessor
    {
        private const string CommercePath = "/sitecore/Commerce/";

        public override void Process(HttpRequestArgs args)
        {
            var path = args?.Context?.Request?.Url?.AbsolutePath ?? "";
            if (Context.Item == null || !path.StartsWith(CommercePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var urlService = DependencyResolver.Current.GetService<CatalogUrlService>();
            var goodUrl = urlService.BuildShopUrl(Context.Item, false, true);
            if (goodUrl != null)
            {
                args.Context.Response.Redirect(goodUrl);
            }
        }
    }
}