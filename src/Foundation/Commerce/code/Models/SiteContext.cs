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
    public class SiteContext : ISiteContext
    {
        private const string CurrentCatalogItemKey = "_CurrentCatallogItem";
        private const string IsCategoryKey = "_IsCategory";
        private const string IsProductKey = "_IsProduct";
        private const string UrlContainsCategoryKey = "_UrlContainsCategory";

        public virtual HttpContext CurrentContext
        {
            get { return HttpContext.Current; }
        }

        public virtual IDictionary Items
        {
            get { return HttpContext.Current.Items; }
        }

        public virtual Item CurrentCatalogItem 
        { 
            get
            {
                return this.Items[CurrentCatalogItemKey] as Item;
            }

            set
            {
                Item item = value as Item;

                this.Items[CurrentCatalogItemKey] = item;
                if (item != null)
                {
                    this.Items[IsCategoryKey] = (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceCategoryTemplate));
                    this.Items[IsProductKey] = (item.IsDerived(CommerceConstants.KnownTemplateIds.CommerceProductTemplate));
                }
                else
                {
                    this.Items[IsCategoryKey] = false;
                    this.Items[IsProductKey] = false;
                }
            }
        }

        public virtual bool IsCategory
        {
            get
            {
                return (this.Items[IsCategoryKey] != null) ? (bool)this.Items[IsCategoryKey] : false;
            }
        }

        public virtual bool IsProduct
        {
            get
            {
                return (this.Items[IsProductKey] != null) ? (bool)this.Items[IsProductKey] : false;
            }
        }

        public virtual bool UrlContainsCategory
        {
            get
            {
                return (this.Items[UrlContainsCategoryKey] != null) ? (bool)this.Items[UrlContainsCategoryKey] : false;
            }

            set
            {
                this.Items[UrlContainsCategoryKey] = value;
            }
        }
    }
}