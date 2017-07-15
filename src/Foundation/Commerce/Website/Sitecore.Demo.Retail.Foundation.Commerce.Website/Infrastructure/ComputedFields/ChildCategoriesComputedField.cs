//-----------------------------------------------------------------------
// <copyright file="ChildCategoriesComputedField.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Sitecore index computed field to save product child categories in the
// sequence defined in Commerce Server.</summary>
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

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommerceServer.Core.Catalog;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Search.ComputedFields;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.ComputedFields
{
    public class ChildCategoriesComputedField : BaseCommerceComputedField
    {
        protected override IEnumerable<ID> ValidTemplates { get; } = new[] { CommerceConstants.KnownTemplateIds.CommerceCategoryTemplate };

        public override object ComputeValue(IIndexable indexable)
        {
            Assert.ArgumentNotNull(indexable, nameof(indexable));
            var validatedItem = GetValidatedItem(indexable);

            if (validatedItem == null)
            {
                return string.Empty;
            }

            var category = GetCategoryReadOnly(validatedItem.ID, validatedItem.Language.Name);
            if (category?.ChildCategories == null || category.ChildCategories.Count <= 0)
            {
                return new List<string>();
            }
            return category.ChildCategories.Select(childCategory => childCategory.ExternalId.ToString()).ToList();
        }

        private Category GetCategoryReadOnly(ID itemId, string language)
        {
            var catalogRepository = DependencyResolver.Current.GetService<ICatalogRepository>();
            var externalInfo = catalogRepository.GetExternalIdInformation(itemId.Guid);

            if (externalInfo == null || externalInfo.CommerceItemType != CommerceItemType.Category)
            {
                return null;
            }
            var culture = CommerceUtility.ConvertLanguageToCulture(language);
            return catalogRepository.GetCategoryReadOnly(externalInfo.CatalogName, externalInfo.CategoryName, culture);
        }
    }
}