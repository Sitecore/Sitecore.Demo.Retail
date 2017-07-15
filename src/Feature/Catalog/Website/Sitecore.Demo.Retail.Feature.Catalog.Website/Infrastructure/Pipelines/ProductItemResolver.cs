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
using Sitecore.Demo.Retail.Feature.Catalog.Website.Factories;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Pipelines.HttpRequest;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Infrastructure.Pipelines
{
    [Service]
    public class ProductItemResolver : HttpRequestProcessor
    {
        public ProductItemResolver(CatalogItemContext catalogItemContext, CatalogItemContextFactory catalogContextFactory)
        {
            CatalogItemContext = catalogItemContext;
            CatalogContextFactory = catalogContextFactory;
        }

        private CatalogItemContext CatalogItemContext { get; }

        private CatalogItemContextFactory CatalogContextFactory { get; }

        private ICatalogItemContext GetCatalogContextFromIncomingRequest()
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var routeData = RouteTable.Routes.GetRouteData(httpContext);

            return routeData == null ? null : CatalogContextFactory.Create(routeData, Context.Database);
        }

        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item != null)
            {
                return;
            }

            var catalogContext = GetCatalogContextFromIncomingRequest();
            if (catalogContext?.Item == null)
            {
                return;
            }

            CatalogItemContext.Current = catalogContext;
            Context.Item = catalogContext.Item;
        }
    }
}