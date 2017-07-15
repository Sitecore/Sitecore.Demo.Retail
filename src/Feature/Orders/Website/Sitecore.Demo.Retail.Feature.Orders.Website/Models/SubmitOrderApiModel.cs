//-----------------------------------------------------------------------
// <copyright file="SubmitOrderModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Emits the Json result of a Submit Order request.</summary>
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

using Sitecore.Commerce.Services;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Models
{
    public class SubmitOrderApiModel : BaseApiModel
    {
        public SubmitOrderApiModel()
        {
        }

        public SubmitOrderApiModel(ServiceProviderResult result)
            : base(result)
        {
        }

        public string ConfirmUrl { get; set; }

        public void Initialize(string confirmUrl)
        {
            Assert.ArgumentNotNullOrEmpty(confirmUrl, nameof(confirmUrl));

            ConfirmUrl = confirmUrl;
        }
    }
}