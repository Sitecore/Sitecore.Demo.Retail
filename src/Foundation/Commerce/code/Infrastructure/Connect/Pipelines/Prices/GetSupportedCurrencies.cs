//-----------------------------------------------------------------------
// <copyright file="GetSupportedCurrencies.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Processor used to return the supported catalog currency.</summary>
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
using System.Collections.ObjectModel;
using System.Linq;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Catalog.Pipelines;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Prices;
using Sitecore.Diagnostics;
using GetSupportedCurrenciesRequest = Sitecore.Foundation.Commerce.Connect.Arguments.GetSupportedCurrenciesRequest;

namespace Sitecore.Foundation.Commerce.Connect.Pipelines.Prices
{
    public class GetSupportedCurrencies : PricePipelineProcessor
    {
        private readonly List<string> _currenciesToInject = new List<string>();
        private string _currencyToInjextString;

        public string InjectCurrencies
        {
            get { return _currencyToInjextString; }

            set
            {
                value.Split(',').ToList().ForEach(x => _currenciesToInject.Add(x.Trim()));
                _currencyToInjextString = value;
            }
        }

        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.Request, "args.request");
            Assert.ArgumentCondition(args.Request is GetSupportedCurrenciesRequest, "args.Request", "args.Request is RefSFArgs.GetSupportedCurrenciesRequest");
            Assert.ArgumentCondition(args.Result is GetSupportedCurrenciesResult, "args.Result", "args.Result is GetSupportedCurrenciesResult");

            var request = (GetSupportedCurrenciesRequest) args.Request;
            var result = (GetSupportedCurrenciesResult) args.Result;

            Assert.ArgumentNotNullOrEmpty(request.CatalogName, "request.CatalogName");

            var catalogRepository = CommerceTypeLoader.CreateInstance<ICatalogRepository>();

            var catalog = catalogRepository.GetCatalogReadOnly(request.CatalogName);

            var currencyList = new List<string> {catalog["Currency"].ToString()};

            if (_currenciesToInject.Count > 0)
                currencyList.AddRange(_currenciesToInject);

            result.Currencies = new ReadOnlyCollection<string>(currencyList);
        }
    }
}