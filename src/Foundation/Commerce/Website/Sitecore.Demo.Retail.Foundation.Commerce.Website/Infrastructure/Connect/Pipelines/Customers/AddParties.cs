//-----------------------------------------------------------------------
// <copyright file="AddParties.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline processor responsible for adding parties (addresses) to a CS user profile.</summary>
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

using System.Linq;
using CommerceServer.Core.Runtime.Profiles;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Connect.CommerceServer.Profiles.Models;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Arguments;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
{
    public class AddParties : CustomerPipelineProcessor
    {
        public AddParties(IEntityFactory entityFactory)
        {
            Assert.ArgumentNotNull(entityFactory, nameof(entityFactory));

            EntityFactory = entityFactory;
        }

        public IEntityFactory EntityFactory { get; set; }

        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentCondition(args.Request is AddPartiesRequest, nameof(args.Request), "args.Request is AddPartiesRequest");
            Assert.ArgumentCondition(args.Result is AddPartiesResult, nameof(args.Result), "args.Result is AddPartiesResult");

            var request = (AddPartiesRequest) args.Request;
            var result = (AddPartiesResult) args.Result;
            Assert.ArgumentNotNull(request.Parties, nameof(request.Parties));
            Assert.ArgumentNotNull(request.CommerceCustomer, nameof(request.CommerceCustomer));

            Profile customerProfile = null;
            var response = GetCommerceUserProfile(request.CommerceCustomer.ExternalId, ref customerProfile);
            if (!response.Success)
            {
                result.Success = false;
                response.SystemMessages.ToList().ForEach(m => result.SystemMessages.Add(m));
                return;
            }

            foreach (var party in request.Parties)
            {
                if (party == null)
                    continue;

                if (party is CommerceParty)
                    ProcessCommerceParty(result, customerProfile, party as CommerceParty);
                else
                    ProcessCustomParty(result, customerProfile, party);
            }
        }

        private Party ProcessCommerceParty(AddPartiesResult result, Profile customerProfile, CommerceParty partyToAdd)
        {
            Assert.ArgumentNotNull(partyToAdd, nameof(partyToAdd));
            Assert.ArgumentNotNull(partyToAdd.Name, nameof(partyToAdd.Name));
            Assert.ArgumentNotNull(partyToAdd.ExternalId, nameof(partyToAdd.ExternalId));

            Profile addressProfile = null;
            var response = CreateAddressProfile(partyToAdd.ExternalId, ref addressProfile);
            if (!response.Success)
            {
                result.Success = false;
                response.SystemMessages.ToList().ForEach(m => result.SystemMessages.Add(m));
                return null;
            }

            var requestToCommerceProfile = new TranslateEntityToCommerceAddressProfileRequest(partyToAdd, addressProfile);
            PipelineUtility.RunCommerceConnectPipeline<TranslateEntityToCommerceAddressProfileRequest, CommerceResult>(Constants.Pipelines.TranslateEntityToCommerceAddressProfile, requestToCommerceProfile);

            addressProfile.Update();

            ProfilePropertyListCollection<string> addressList;
            var profileValue = customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressList].Value as object[];
            if (profileValue != null)
            {
                var e = profileValue.Select(i => i.ToString());
                addressList = new ProfilePropertyListCollection<string>(e);
            }
            else
            {
                addressList = new ProfilePropertyListCollection<string>();
            }

            addressList.Add(partyToAdd.ExternalId);
            customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.AddressList].Value = addressList.Cast<object>().ToArray();

            if (partyToAdd.IsPrimary)
                customerProfile[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.GeneralInfo.PreferredAddress].Value = partyToAdd.ExternalId;

            customerProfile.Update();

            var newParty = EntityFactory.Create<CommerceParty>("Party");
            var requestToEntity = new TranslateCommerceAddressProfileToEntityRequest(addressProfile, newParty);
            PipelineUtility.RunCommerceConnectPipeline<TranslateCommerceAddressProfileToEntityRequest, CommerceResult>(Constants.Pipelines.TranslateCommerceAddressProfileToEntity, requestToEntity);

            return requestToEntity.DestinationParty;
        }

        private Party ProcessCustomParty(AddPartiesResult result, Profile customerProfile, Party party)
        {
            return null;
        }
    }
}