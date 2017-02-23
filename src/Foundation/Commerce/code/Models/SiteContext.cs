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

using System.Collections;
using System.Web;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Models
{
    public class SiteContext
    {
        private const string CurrentCatalogItemKey = "_CurrentCatallogItem";
        private const string IsCategoryKey = "_IsCategory";
        private const string IsProductKey = "_IsProduct";
        private const string UrlContainsCategoryKey = "_UrlContainsCategory";

        public HttpContext CurrentContext => HttpContext.Current;

        public IDictionary Items => HttpContext.Current.Items;

        public Item CurrentCatalogItem 
        { 
            get
            {
                return this.Items[CurrentCatalogItemKey] as Item;
            }

            set
            {
                this.Items[CurrentCatalogItemKey] = value;
                if (value != null)
                {
                    this.Items[IsCategoryKey] = (value.IsDerived(CommerceConstants.KnownTemplateIds.CommerceCategoryTemplate));
                    this.Items[IsProductKey] = (value.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductTemplate));
                }
                else
                {
                    this.Items[IsCategoryKey] = false;
                    this.Items[IsProductKey] = false;
                }
            }
        }

        public bool IsCategory => (bool?) Items[IsCategoryKey] ?? false;

        public bool IsProduct => (bool?) Items[IsProductKey] ?? false;

        public bool UrlContainsCategory
        {
            get
            {
                return (bool?) Items[UrlContainsCategoryKey] ?? false;
            }

            set
            {
                this.Items[UrlContainsCategoryKey] = value;
            }
        }
    }
}