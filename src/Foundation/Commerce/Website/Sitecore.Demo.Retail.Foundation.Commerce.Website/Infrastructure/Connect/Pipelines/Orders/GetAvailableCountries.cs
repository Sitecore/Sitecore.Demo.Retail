//-----------------------------------------------------------------------
// <copyright file="GetAvailableCountries.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline responsible for returning the available countries.</summary>
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

using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Orders;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Repositories;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Orders
{
    public class GetAvailableCountries : CommerceOrderPipelineProcessor
    {
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentCondition(args.Request is GetAvailableCountriesRequest, nameof(args.Request), "args.Request is GetAvailableCountriesRequest");
            Assert.ArgumentCondition(args.Result is GetAvailableCountriesResult, nameof(args.Result), "args.Result is GetAvailableCountriesResult");

            var result = (GetAvailableCountriesResult) args.Result;

            var repository = new CountryRepository();
            result.AvailableCountries = repository.GetCountriesAsDictionary();
        }
    }
}