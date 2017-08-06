//---------------------------------------------------------------------
// <copyright file="VaryByCurrency.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Handles the "Vary by Currency" cacheable option.</summary>
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
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Pipelines
{
    [Service]
    public class VaryByCurrency : RenderRenderingProcessor
    {
        public VaryByCurrency(CurrencyManager currencyManager)
        {
            CurrencyManager = currencyManager;
        }

        public CurrencyManager CurrencyManager { get; }

        public override void Process(RenderRenderingArgs args)
        {
            if (args.Rendered || HttpContext.Current == null || !args.Cacheable || args.Rendering.RenderingItem == null)
            {
                return;
            }

            var rendering = args.PageContext.Database.GetItem(args.Rendering.RenderingItem.ID);

            if (rendering == null || rendering["VaryByCurrency"] != "1")
            {
                return;
            }

            // When no cache key is present, we generate a full cache key; Otherwise we append to the existing ones.
            if (string.IsNullOrWhiteSpace(args.CacheKey))
            {
                args.CacheKey = $"_#varyByCurrency_{Context.Site.Name}_{Context.Language.Name}_{HttpContext.Current.Request.Url.AbsoluteUri}_{args.Rendering.RenderingItem.ID}_{CurrencyManager.CurrencyContext.CurrencyCode}";
            }
            else
            {
                args.CacheKey = $"_#varybyCurrency_{args.CacheKey}_{args.Rendering.RenderingItem.ID}_{CurrencyManager.CurrencyContext.CurrencyCode}";
            }
        }
    }
}