//-----------------------------------------------------------------------
// <copyright file="ShippingMethodsModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the ShippingMethodsModel class.</summary>
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
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Commerce.Services.Shipping;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
    public class ShippingMethodsApiModel : BaseApiModel
    {
        public ShippingMethodsApiModel()
        {
        }

        public ShippingMethodsApiModel(GetShippingMethodsResult result)
            : base(result)
        {
        }

        public IEnumerable<ShippingMethodPerItemApiModel> LineShippingMethods { get; set; }
        public IEnumerable<ShippingMethodApiModel> ShippingMethods { get; set; }


        private void Initialize(IEnumerable<ShippingMethod> shippingMethods)
        {
            if (shippingMethods == null)
            {
                return;
            }

            var shippingMethodList = new List<ShippingMethodApiModel>();

            foreach (var shippingMethod in shippingMethods)
            {
                var jsonResult = new ShippingMethodApiModel();

                jsonResult.Initialize(shippingMethod);
                shippingMethodList.Add(jsonResult);
            }

            ShippingMethods = shippingMethodList;
        }

        public void Initialize(IEnumerable<ShippingMethod> shippingMethods, IEnumerable<ShippingMethodPerItem> shippingMethodsPerItem)
        {
            Initialize(shippingMethods);

            if (shippingMethods == null || shippingMethodsPerItem == null)
                return;

            var shippingMethodPerItemArray = shippingMethodsPerItem as ShippingMethodPerItem[] ?? shippingMethodsPerItem.ToArray();
            if (!shippingMethodPerItemArray.Any())
            {
                return;
            }

            var lineShippingMethodList = new List<ShippingMethodPerItemApiModel>();

            foreach (var shippingMethodPerItem in shippingMethodPerItemArray)
            {
                var jsonResult = new ShippingMethodPerItemApiModel();

                jsonResult.Initialize(shippingMethodPerItem);

                lineShippingMethodList.Add(jsonResult);
            }

            LineShippingMethods = lineShippingMethodList;
        }
    }
}