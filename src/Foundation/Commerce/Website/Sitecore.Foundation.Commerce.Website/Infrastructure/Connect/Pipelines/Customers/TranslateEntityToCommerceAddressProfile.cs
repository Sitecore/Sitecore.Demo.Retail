//-----------------------------------------------------------------------
// <copyright file="TranslateEntityToCommerceAddressProfile.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline processor used to translate a Party to a Commerce Server address .</summary>
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

using CommerceServer.Core.Runtime.Profiles;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Pipelines;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Arguments;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
{
    public class TranslateEntityToCommerceAddressProfile : CommerceTranslateProcessor
    {
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentNotNull(args.Request, nameof(args.Request));
            Assert.ArgumentNotNull(args.Result, nameof(args.Result));
            Assert.ArgumentCondition(args.Request is TranslateEntityToCommerceAddressProfileRequest, nameof(args.Request), "args.Request is TranslateEntityToCommerceAddressProfileRequest");

            var request = (TranslateEntityToCommerceAddressProfileRequest) args.Request;
            Assert.ArgumentNotNull(request.SourceParty, nameof(request.SourceParty));
            Assert.ArgumentNotNull(request.DestinationProfile, nameof(request.DestinationProfile));

            if (request.SourceParty != null)
            {
                TranslateCommerceCustomerParty(request.SourceParty, request.DestinationProfile);
            }
            else
            {
                TranslateCustomParty(request.SourceParty, request.DestinationProfile);
            }
        }

        private void TranslateCommerceCustomerParty(CommerceParty party, Profile profile)
        {
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.FirstName].Value = party.FirstName;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.LastName].Value = party.LastName;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressName].Value = party.Name;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressLine1].Value = party.Address1;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressLine2].Value = party.Address2;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.City].Value = party.City;
            if (!string.IsNullOrEmpty(party.State))
            {
                profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.RegionCode].Value = party.State;
            }
            else if (!string.IsNullOrEmpty(party.RegionCode))
            {
                profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.RegionCode].Value = party.RegionCode;
            }
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.RegionName].Value = party.RegionName;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PostalCode].Value = party.ZipPostalCode;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.CountryCode].Value = party.CountryCode;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.CountryName].Value = party.Country;
            profile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.TelNumber].Value = party.PhoneNumber;

            TranslateCommerceCustomerPartyCustomProperties(party, profile);
        }

        private void TranslateCommerceCustomerPartyCustomProperties(CommerceParty party, Profile profile)
        {
        }

        private void TranslateCustomParty(CommerceParty party, Profile profile)
        {
        }
    }
}