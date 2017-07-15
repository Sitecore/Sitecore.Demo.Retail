//-----------------------------------------------------------------------
// <copyright file="LineShippingOptionModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Line shipping options JSON result.</summary>
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
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Models
{
    public class LineShippingOptionApiModel : BaseApiModel
    {
        public string LineId { get; set; }

        public IEnumerable<ShippingOptionApiModel> ShippingOptions { get; set; }

        public void Initialize(LineShippingOption lineShippingOption)
        {
            if (lineShippingOption == null)
            {
                return;
            }

            LineId = lineShippingOption.LineId;

            var shippingOptionList = new List<ShippingOptionApiModel>();

            if (lineShippingOption.ShippingOptions != null)
            {
                foreach (var shippingOption in lineShippingOption.ShippingOptions)
                {
                    var jsonResult = new ShippingOptionApiModel();

                    jsonResult.Initialize(shippingOption);
                    shippingOptionList.Add(jsonResult);
                }
            }

            ShippingOptions = shippingOptionList;
        }
    }
}