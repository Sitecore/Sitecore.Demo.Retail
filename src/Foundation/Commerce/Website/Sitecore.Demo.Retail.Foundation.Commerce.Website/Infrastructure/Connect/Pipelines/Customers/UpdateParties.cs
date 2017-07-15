//-----------------------------------------------------------------------
// <copyright file="UpdateParties.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline processor used to update parties (addresses) from CS user profiles.</summary>
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
using System.Linq;
using CommerceServer.Core.Runtime.Profiles;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Connect.CommerceServer.Profiles.Models;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Arguments;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
{
    public class UpdateParties : CustomerPipelineProcessor
    {
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentCondition(args.Request is UpdatePartiesRequest, nameof(args.Request), "args.Request is UpdatePartiesRequest");
            Assert.ArgumentCondition(args.Result is CustomerResult, nameof(args.Result), "args.Result is CustomerResult");

            var request = (UpdatePartiesRequest) args.Request;
            var result = (CustomerResult) args.Result;

            Profile customerProfile = null;
            var response = GetCommerceUserProfile(request.CommerceCustomer.ExternalId, ref customerProfile);
            if (!response.Success)
            {
                result.Success = false;
                response.SystemMessages.ToList().ForEach(m => result.SystemMessages.Add(m));
                return;
            }

            var preferredAddress = customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PreferredAddress].Value as string;

            var profileValue = customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressList].Value as object[];
            if (profileValue == null)
                return;

            var e = profileValue.Select(i => i.ToString());
            var addressList = new ProfilePropertyListCollection<string>(e);

            foreach (var partyToUpdate in request.Parties)
            {
                Assert.IsTrue(partyToUpdate is CommerceParty, "partyToUpdate is CommerceParty");

                var foundId = addressList.FirstOrDefault(x => x.Equals(partyToUpdate.ExternalId, StringComparison.OrdinalIgnoreCase));
                if (foundId == null)
                    continue;
                Profile commerceAddress = null;
                response = GetCommerceAddressProfile(foundId, ref commerceAddress);
                if (!response.Success)
                {
                    result.Success = false;
                    response.SystemMessages.ToList().ForEach(m => result.SystemMessages.Add(m));
                    return;
                }

                // Check if the IsPrimary address flag has been flipped.
                if (((CommerceParty) partyToUpdate).IsPrimary)
                {
                    customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PreferredAddress].Value = partyToUpdate.ExternalId;
                    customerProfile.Update();
                }
                else if (!string.IsNullOrWhiteSpace(preferredAddress) && preferredAddress.Equals(partyToUpdate.ExternalId, StringComparison.OrdinalIgnoreCase))
                {
                    customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PreferredAddress].Value = DBNull.Value;
                    customerProfile.Update();
                }

                var translateToEntityRequest = new TranslateEntityToCommerceAddressProfileRequest((CommerceParty) partyToUpdate, commerceAddress);
                PipelineUtility.RunCommerceConnectPipeline<TranslateEntityToCommerceAddressProfileRequest, CommerceResult>(Constants.Pipelines.TranslateEntityToCommerceAddressProfile, translateToEntityRequest);

                commerceAddress.Update();
            }
        }
    }
}