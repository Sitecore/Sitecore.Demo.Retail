//-----------------------------------------------------------------------
// <copyright file="SiteContext.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the SiteContext class.</summary>
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
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    [Service]
    public class CatalogItemContext
    {
        public bool IsCategory => Current?.ItemType == CatalogItemType.Category;

        public bool IsProduct => Current?.ItemType == CatalogItemType.Product;

        public ICatalogItemContext Current
        {
            get
            {
                return (ICatalogItemContext)HttpContext.Current.Items["_catalogItemContext"];
            }

            set
            {
                HttpContext.Current.Items["_catalogItemContext"] = value;
            }
        }
    }
}