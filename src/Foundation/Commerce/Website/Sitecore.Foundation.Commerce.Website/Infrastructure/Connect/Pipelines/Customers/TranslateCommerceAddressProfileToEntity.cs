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
using Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Arguments;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
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

        private void TranslateToCommerceParty(Profile profile, CommerceParty party)
        {
            party.ExternalId = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressId);
            party.FirstName = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.FirstName);
            party.LastName = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.LastName);
            party.Name = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressName);
            party.Address1 = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressLine1);
            party.Address2 = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressLine2);
            party.City = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.City);
            party.RegionCode = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.RegionCode);
            party.RegionName = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.RegionName);
            party.ZipPostalCode = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PostalCode);
            party.CountryCode = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.CountryCode);
            party.Country = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.CountryName);
            party.PhoneNumber = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.TelNumber);
            party.State = Get<string>(profile, global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.RegionCode);

            TranslateToCommercePartyCustomProperties(profile, party);
        }

        private void TranslateToCommercePartyCustomProperties(Profile profile, CommerceParty party)
        {
        }

        private void TranslateToCustomParty(Profile profile, Party party)
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