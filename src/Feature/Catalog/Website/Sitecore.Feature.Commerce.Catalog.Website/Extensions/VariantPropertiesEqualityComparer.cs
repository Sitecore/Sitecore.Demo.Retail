//-----------------------------------------------------------------------
// <copyright file="VariantPropertiesEqualityComparer.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
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

using System;
using System.Collections.Generic;
using Sitecore.Feature.Commerce.Catalog.Website.Models;

namespace Sitecore.Feature.Commerce.Catalog.Website.Extensions
{
    public enum VariantPropertiesComparisonProperty
    {
        ProductColor,
        Size
    }

    public class VariantPropertiesEqualityComparer : IEqualityComparer<VariantViewModel>
    {
        public VariantPropertiesEqualityComparer(VariantPropertiesComparisonProperty comparisonProperty)
        {
            ComparisonProperty = comparisonProperty;
        }

        protected VariantPropertiesComparisonProperty ComparisonProperty { get; set; }

        public bool Equals(VariantViewModel x, VariantViewModel y)
        {
            var areEqual = false;
            switch (ComparisonProperty)
            {
                case VariantPropertiesComparisonProperty.ProductColor:
                {
                    areEqual = x.ProductColor.Equals(y.ProductColor, StringComparison.OrdinalIgnoreCase);
                    break;
                }

                case VariantPropertiesComparisonProperty.Size:
                {
                    areEqual = x.Size.Equals(y.Size, StringComparison.OrdinalIgnoreCase);
                    break;
                }

                default:
                {
                    areEqual = x.Size.Equals(y.Size, StringComparison.OrdinalIgnoreCase) &&
                               x.ProductColor.Equals(y.ProductColor, StringComparison.OrdinalIgnoreCase);
                    break;
                }
            }

            return areEqual;
        }

        public int GetHashCode(VariantViewModel obj)
        {
            switch (ComparisonProperty)
            {
                case VariantPropertiesComparisonProperty.ProductColor:
                {
                    return obj.ProductColor.GetHashCode();
                }

                case VariantPropertiesComparisonProperty.Size:
                {
                    return obj.Size.GetHashCode();
                }

                default:
                {
                    return obj.GetHashCode();
                }
            }
        }
    }
}