//-----------------------------------------------------------------------
// <copyright file="CustomerPipelineProcessor.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Base class of all Customer related pipeline processor.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Connect.CommerceServer.Profiles.Pipelines;
using Sitecore.Commerce.Services;
using Sitecore.Pipelines;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
{
    public class CustomerPipelineProcessor : CommercePipelineProcessor
    {
        protected ServiceProviderResult GetCommerceAddressProfile(string id, ref Profile profile)
        {
            return GetCommerceProfile(id, "Address", ref profile);
        }

        protected ServiceProviderResult GetCommerceUserProfile(string id, ref Profile profile)
        {
            return GetCommerceProfile(id, "UserObject", ref profile);
        }

        protected ServiceProviderResult CreateAddressProfile(string id, ref Profile profile)
        {
            var result = new ServiceProviderResult {Success = true};

            try
            {
                var createArgs = new CreateProfileArgs();
                createArgs.InputParameters.Name = "Address";
                createArgs.InputParameters.Id = id;

                CorePipeline.Run(CommerceConstants.PipelineNames.CreateProfile, createArgs);
                profile = createArgs.OutputParameters.CommerceProfile;
            }
            catch (Exception e)
            {
                result = new ServiceProviderResult {Success = false};
                result.SystemMessages.Add(new SystemMessage {Message = e.Message});
            }

            return result;
        }

        protected ServiceProviderResult DeleteAddressCommerceProfile(string id)
        {
            return DeleteCommerceProfile(id, "Address");
        }

        private ServiceProviderResult GetCommerceProfile(string id, string profileName, ref Profile profile)
        {
            var result = new ServiceProviderResult {Success = true};

            try
            {
                var createArgs = new GetProfileArgs();
                createArgs.InputParameters.Name = profileName;
                createArgs.InputParameters.Id = id;

                CorePipeline.Run(CommerceConstants.PipelineNames.GetProfile, createArgs);
                profile = createArgs.OutputParameters.CommerceProfile;
            }
            catch (Exception e)
            {
                result = new ServiceProviderResult {Success = false};
                result.SystemMessages.Add(new SystemMessage {Message = e.Message});
            }

            return result;
        }

        private ServiceProviderResult DeleteCommerceProfile(string id, string profileName)
        {
            var result = new ServiceProviderResult {Success = true};

            try
            {
                var deleteArgs = new DeleteProfileArgs();
                deleteArgs.InputParameters.Name = profileName;
                deleteArgs.InputParameters.Id = id;

                CorePipeline.Run(CommerceConstants.PipelineNames.DeleteProfile, deleteArgs);
                result.Success = deleteArgs.OutputParameters.Success;
            }
            catch (Exception e)
            {
                result = new ServiceProviderResult {Success = false};
                result.SystemMessages.Add(new SystemMessage {Message = e.Message});
            }

            return result;
        }
    }
}