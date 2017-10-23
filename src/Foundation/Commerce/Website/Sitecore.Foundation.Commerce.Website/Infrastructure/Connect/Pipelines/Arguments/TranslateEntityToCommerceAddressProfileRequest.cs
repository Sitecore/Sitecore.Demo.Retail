//-----------------------------------------------------------------------
// <copyright file="TranslateEntityToCommerceAddressProfileRequest.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Request class for the pipeline responsible for translating a Party to a Commerce Server address.</summary>
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
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Arguments
{
    public class TranslateEntityToCommerceAddressProfileRequest : CommerceRequest
    {
        public TranslateEntityToCommerceAddressProfileRequest(CommerceParty sourceParty, Profile destinationProfile)
        {
            Assert.ArgumentNotNull(destinationProfile, nameof(destinationProfile));
            Assert.ArgumentNotNull(sourceParty, nameof(sourceParty));

            this.DestinationProfile = destinationProfile;
            this.SourceParty = sourceParty;
        }

        public Profile DestinationProfile { get; set; }

        public CommerceParty SourceParty { get; set; }
    }
}