//-----------------------------------------------------------------------
// <copyright file="StorefrontManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StorefrontManager class.</summary>
//-----------------------------------------------------------------------
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
using System.Web.Mvc;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Links;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class StorefrontManager : IManager
    {
        public CommerceStorefront Current
        {
            get
            {
                var path = Context.Site.RootPath + Context.Site.StartItem;
                if (HttpContext.Current.Items.Contains(path))
                {
                    return HttpContext.Current.Items[path] as CommerceStorefront;
                }

                var storefront = new CommerceStorefront(Context.Database.GetItem(path));
                HttpContext.Current.Items[path] = storefront;
                return (CommerceStorefront) HttpContext.Current.Items[path];
            }
        }

        public string StorefrontHome => LinkManager.GetItemUrl(Current.HomeItem);
    }
}