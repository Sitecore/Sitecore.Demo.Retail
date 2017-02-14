//-----------------------------------------------------------------------
// <copyright file="TranslateCommerceAddressProfileToEntity.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline processor used to translate a Commerce Server address to a Party.</summary>
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
using CommerceServer.Core.Runtime.Profiles;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Pipelines;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Connect.Pipelines.Arguments;

namespace Sitecore.Foundation.Commerce.Connect.Pipelines.Customers
{
    public class TranslateCommerceAddressProfileToEntity : CommerceTranslateProcessor
    {
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentNotNull(args.Request, nameof(args.Request));
            Assert.ArgumentNotNull(args.Result, nameof(args.Result));
            Assert.ArgumentCondition(args.Request is TranslateCommerceAddressProfileToEntityRequest, nameof(args.Request), "args.Request is TranslateCommerceProfileToEntityRequest");

            var request = (TranslateCommerceAddressProfileToEntityRequest) args.Request;
            Assert.ArgumentNotNull(request.SourceProfile, nameof(request.SourceProfile));
            Assert.ArgumentNotNull(request.DestinationParty, nameof(request.DestinationParty));

            if (request.DestinationParty != null)
                TranslateToCommerceParty(request.SourceProfile, request.DestinationParty);
            else
                TranslateToCustomParty(request.SourceProfile, request.DestinationParty);
        }

        protected virtual void TranslateToCommerceParty(Profile profile, CommerceParty party)
        {
            party.ExternalId = Get<string>(profile, "GeneralInfo.address_id");
            party.FirstName = Get<string>(profile, "GeneralInfo.first_name");
            party.LastName = Get<string>(profile, "GeneralInfo.last_name");
            party.Name = Get<string>(profile, "GeneralInfo.address_name");
            party.Address1 = Get<string>(profile, "GeneralInfo.address_line1");
            party.Address2 = Get<string>(profile, "GeneralInfo.address_line2");
            party.City = Get<string>(profile, "GeneralInfo.city");
            party.RegionCode = Get<string>(profile, "GeneralInfo.region_code");
            party.RegionName = Get<string>(profile, "GeneralInfo.region_name");
            party.ZipPostalCode = Get<string>(profile, "GeneralInfo.postal_code");
            party.CountryCode = Get<string>(profile, "GeneralInfo.country_code");
            party.Country = Get<string>(profile, "GeneralInfo.country_name");
            party.PhoneNumber = Get<string>(profile, "GeneralInfo.tel_number");
            party.State = Get<string>(profile, "GeneralInfo.region_code");

            TranslateToCommercePartyCustomProperties(profile, party);
        }

        protected virtual void TranslateToCommercePartyCustomProperties(Profile profile, CommerceParty party)
        {
        }

        protected virtual void TranslateToCustomParty(Profile profile, Party party)
        {
        }

        protected T Get<T>(Profile profile, string propertyName)
        {
            if (profile[propertyName].Value == DBNull.Value)
                return default(T);

            return (T) profile[propertyName].Value;
        }
    }
}