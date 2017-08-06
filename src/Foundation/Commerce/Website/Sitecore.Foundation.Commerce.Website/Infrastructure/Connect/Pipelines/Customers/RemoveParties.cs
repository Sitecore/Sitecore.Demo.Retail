//-----------------------------------------------------------------------
// <copyright file="RemoveParties.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline processor used to remove parties (addresses) from CS user profiles.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer.Profiles.Models;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
{
    public class RemoveParties : CustomerPipelineProcessor
    {
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentCondition(args.Request is RemovePartiesRequest, nameof(args.Request), "args.Request is RemovePartiesRequest");
            Assert.ArgumentCondition(args.Result is CustomerResult, nameof(args.Result), "args.Result is CustomerResult");

            var request = (RemovePartiesRequest) args.Request;
            var result = (CustomerResult) args.Result;

            Profile customerProfile = null;
            var response = GetCommerceUserProfile(request.CommerceCustomer.ExternalId, ref customerProfile);
            if (!response.Success)
            {
                result.Success = false;
                response.SystemMessages.ToList().ForEach(m => result.SystemMessages.Add(m));
                return;
            }

            var preferredAddress = customerProfile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PreferredAddress].Value as string;

            var profileValue = customerProfile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressList].Value as object[];
            if (profileValue == null)
                return;
            var e = profileValue.Select(i => i.ToString());
            var addressList = new ProfilePropertyListCollection<string>(e);

            foreach (var partyToRemove in request.Parties)
            {
                var foundId = addressList.FirstOrDefault(x => x.Equals(partyToRemove.ExternalId, StringComparison.OrdinalIgnoreCase));
                if (foundId == null)
                    continue;

                response = DeleteAddressCommerceProfile(foundId);
                if (!response.Success)
                {
                    result.Success = false;
                    response.SystemMessages.ToList().ForEach(m => result.SystemMessages.Add(m));
                    return;
                }

                addressList.Remove(foundId);

                if (!addressList.Any())
                    customerProfile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressList].Value = DBNull.Value;
                else
                    customerProfile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressList].Value = addressList.Cast<object>().ToArray();

                // Prefered address check. If the address being deleted was the preferred address we must clear it from the customer profile.
                if (!string.IsNullOrWhiteSpace(preferredAddress) && preferredAddress.Equals(partyToRemove.ExternalId, StringComparison.OrdinalIgnoreCase))
                    customerProfile[global::Sitecore.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PreferredAddress].Value = DBNull.Value;

                customerProfile.Update();
            }
        }
    }
}