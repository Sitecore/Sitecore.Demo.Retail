//---------------------------------------------------------------------
// <copyright file="MetadataManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Metadata manager class used to generate the metadata tags.</summary>
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
using System.Web.Mvc;
using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
#warning move to feature.catalog
    public static class MetadataManager
    {
        public static HtmlString GetTags()
        {
            var context = DependencyResolver.Current.GetService<CatalogItemContext>();

            if (context.IsCategory)
            {
                return new HtmlString(GetCategoryTags(context.Current?.Item));
            }
            if (context.IsProduct)
            {
                return new HtmlString(GetProductTags(context.Current?.Item));
            }

            return new HtmlString(string.Empty);
        }

        private static string GetCategoryTags(Item item)
        {
            var url = "/category/" + item.Name;
            return $"<link rel='canonical' href='{url}'/>";
        }

        private static string GetProductTags(Item item)
        {
            var url = "/product/" + item.Name;
            return $"<link rel='canonical' href='{url}'/>";
        }
    }
}