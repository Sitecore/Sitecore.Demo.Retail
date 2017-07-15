//-----------------------------------------------------------------------
// <copyright file="ShippingMethodModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Shipping method JSON result.</summary>
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

using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Models
{
    public class ShippingMethodApiModel : BaseApiModel
    {
        public string ExternalId { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string ShippingOptionId { get; set; }

        public string ShopName { get; set; }

        public void Initialize(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
            {
                return;
            }

            ExternalId = shippingMethod.ExternalId;
            Description = ShippingManager.GetShippingName(shippingMethod.Description);
            Name = shippingMethod.Name;
            ShippingOptionId = shippingMethod.ShippingOptionId;
            ShopName = shippingMethod.ShopName;
        }
    }
}