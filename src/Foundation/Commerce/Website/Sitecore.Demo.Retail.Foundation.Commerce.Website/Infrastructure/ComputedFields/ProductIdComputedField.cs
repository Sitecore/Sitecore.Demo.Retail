//-----------------------------------------------------------------------
// <copyright file="ProductIdComputedField.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Sitecore index computed field to lower case the product id.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search.ComputedFields;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.ComputedFields
{
    public class ProductIdComputedField : BaseCommerceComputedField
    {
        protected override IEnumerable<ID> ValidTemplates { get; } = new List<ID>
        {
            CommerceConstants.KnownTemplateIds.CommerceProductTemplate
        };

        public override object ComputeValue(IIndexable indexable)
        {
            Assert.ArgumentNotNull(indexable, nameof(indexable));
            var validatedItem = GetValidatedItem(indexable);

            return string.IsNullOrWhiteSpace(validatedItem?.Name) ? string.Empty : validatedItem.Name.ToLower(CultureInfo.InvariantCulture);
        }
    }
}