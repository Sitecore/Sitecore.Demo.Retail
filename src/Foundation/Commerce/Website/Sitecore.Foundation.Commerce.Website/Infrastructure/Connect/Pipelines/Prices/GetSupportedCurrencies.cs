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
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Catalog.Pipelines;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Prices;
using Sitecore.Diagnostics;
using GetSupportedCurrenciesRequest = Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Arguments.GetSupportedCurrenciesRequest;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Prices
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
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentNotNull(args.Request, nameof(args.Request));
            Assert.ArgumentCondition(args.Request is GetSupportedCurrenciesRequest, nameof(args.Request), "args.Request is RefSFArgs.GetSupportedCurrenciesRequest");
            Assert.ArgumentCondition(args.Result is GetSupportedCurrenciesResult, nameof(args.Result), "args.Result is GetSupportedCurrenciesResult");

            var request = (GetSupportedCurrenciesRequest) args.Request;
            var result = (GetSupportedCurrenciesResult) args.Result;

            Assert.ArgumentNotNullOrEmpty(request.CatalogName, nameof(request.CatalogName));

            var catalogRepository = DependencyResolver.Current.GetService<ICatalogRepository>();

            var catalog = catalogRepository.GetCatalogReadOnly(request.CatalogName);

            var currencyList = new List<string> {catalog[CommerceConstants.KnownCatalogFieldNames.Currency].ToString()};

            if (_currenciesToInject.Count > 0)
                currencyList.AddRange(_currenciesToInject);

            result.Currencies = new ReadOnlyCollection<string>(currencyList);
        }
    }
}