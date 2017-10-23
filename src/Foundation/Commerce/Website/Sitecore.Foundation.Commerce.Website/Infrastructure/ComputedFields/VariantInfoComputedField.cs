//-----------------------------------------------------------------------
// <copyright file="VariantInfoComputedField.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Sitecore index computed field to save product variant information stored in the 
// "VariantInfo" index document property.</summary>
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
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using CommerceServer.Core.Catalog;
using Newtonsoft.Json;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Search.ComputedFields;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.ComputedFields
{
    public class VariantInfoComputedField : BaseCommerceVariants<string>
    {
        public override object ComputeValue(IIndexable indexable)
        {
            Assert.ArgumentNotNull(indexable, nameof(indexable));
            var validatedItem = GetValidatedItem(indexable);

            if (validatedItem == null)
            {
                return string.Empty;
            }

            var variantInfoList = new List<VariantIndexInfo>();

            foreach (var productVariant in GetChildVariantsReadOnly(validatedItem.ID, validatedItem.Language.Name))
            {
                var variantInfo = new VariantIndexInfo
                {
                    VariantId = productVariant.VariantId,
                    BasePrice = GetVariantFieldValue<decimal>(productVariant, "BasePriceVariant"),
                    ListPrice = productVariant.ListPrice
                };


                variantInfoList.Add(variantInfo);
            }

            return JsonConvert.SerializeObject(variantInfoList);
        }

        private T GetVariantFieldValue<T>(Variant variant, string fieldName)
        {
            if (!variant.DataRow.Table.Columns.Contains(fieldName))
            {
                return default(T);
            }

            var variantValue = variant[fieldName];
            if (variantValue == null)
            {
                return default(T);
            }

            if (variantValue is T)
            {
                return (T) variantValue;
            }

            return (T) System.Convert.ChangeType(variantValue, typeof(T), CultureInfo.InvariantCulture);
        }

        private IEnumerable<Variant> GetChildVariantsReadOnly(ID itemId, string language)
        {
            var catalogRepository = DependencyResolver.Current.GetService<ICatalogRepository>();
            var externalInfo = catalogRepository.GetExternalIdInformation(itemId.Guid);
            if (externalInfo == null || externalInfo.CommerceItemType != CommerceItemType.ProductFamily)
            {
                return Enumerable.Empty<Variant>();
            }

            var culture = CommerceUtility.ConvertLanguageToCulture(language);
            var productFamily = catalogRepository.GetProductReadOnly(externalInfo.CatalogName, externalInfo.ProductId, culture) as ProductFamily;
            if (productFamily != null && productFamily.Variants.Count > 0)
            {
                return productFamily.Variants;
            }

            return Enumerable.Empty<Variant>();
        }
    }
}