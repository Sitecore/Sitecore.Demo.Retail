//-----------------------------------------------------------------------
// <copyright file="ShippingMethodPerItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Shipping method per item JSON result.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Entities.Shipping;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class ShippingMethodPerItemBaseJsonResult : BaseJsonResult
    {
        public string LineId { get; set; }

        public IEnumerable<ShippingMethodBaseJsonResult> ShippingMethods { get; set; }

        public virtual void Initialize(ShippingMethodPerItem shippingMethodPerItem)
        {
            if (shippingMethodPerItem == null)
            {
                return;
            }

            LineId = shippingMethodPerItem.LineId;

            if (shippingMethodPerItem.ShippingMethods != null && shippingMethodPerItem.ShippingMethods.Any())
            {
                var shippingMethodList = new List<ShippingMethodBaseJsonResult>();

                foreach (var shippingMethod in shippingMethodPerItem.ShippingMethods)
                {
                    var jsonResult = CommerceTypeLoader.CreateInstance<ShippingMethodBaseJsonResult>();

                    jsonResult.Initialize(shippingMethod);
                    shippingMethodList.Add(jsonResult);
                }

                ShippingMethods = shippingMethodList;
            }
        }
    }
}