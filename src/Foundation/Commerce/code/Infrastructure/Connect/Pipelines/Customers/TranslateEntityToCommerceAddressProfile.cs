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
using Sitecore.Foundation.Commerce.Connect.Pipelines.Arguments;

namespace Sitecore.Foundation.Commerce.Infrastructure.Connect.Pipelines.Customers
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

        protected virtual void TranslateCommerceCustomerParty(CommerceParty party, Profile profile)
        {
            profile["GeneralInfo.first_name"].Value = party.FirstName;
            profile["GeneralInfo.last_name"].Value = party.LastName;
            profile["GeneralInfo.address_name"].Value = party.Name;
            profile["GeneralInfo.address_line1"].Value = party.Address1;
            profile["GeneralInfo.address_line2"].Value = party.Address2;
            profile["GeneralInfo.city"].Value = party.City;
            profile["GeneralInfo.region_code"].Value = party.RegionCode;
            profile["GeneralInfo.region_name"].Value = party.RegionName;
            profile["GeneralInfo.postal_code"].Value = party.ZipPostalCode;
            profile["GeneralInfo.country_code"].Value = party.CountryCode;
            profile["GeneralInfo.country_name"].Value = party.Country;
            profile["GeneralInfo.tel_number"].Value = party.PhoneNumber;
            profile["GeneralInfo.region_code"].Value = party.State;

            TranslateCommerceCustomerPartyCustomProperties(party, profile);
        }

        protected virtual void TranslateCommerceCustomerPartyCustomProperties(CommerceParty party, Profile profile)
        {
        }

        private void TranslateCustomParty(CommerceParty party, Profile profile)
        {
        }
    }
}