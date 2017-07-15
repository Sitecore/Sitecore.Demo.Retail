//-----------------------------------------------------------------------
// <copyright file="GetUser.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Pipeline the retrieves the user.</summary>
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
using Sitecore.Commerce.Data.Customers;
using Sitecore.Commerce.Entities.Customers;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Pipelines.Customers.GetUser;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Diagnostics;
using Sitecore.Security;
using Sitecore.Security.Accounts;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Connect.Pipelines.Customers
{
    public class GetUser : GetUserFromSitecore
    {
        public GetUser(IUserRepository userRepository) : base(userRepository)
        {
        }

        public override void Process(ServicePipelineArgs args)
        {
            base.Process(args);

            var result = (GetUserResult) args.Result;

            if (result.CommerceUser == null)
                return;

            // if we found a user, add some addition info
            var userProfile = GetUserProfile(result.CommerceUser.UserName);
            Assert.IsNotNull(userProfile, $"Could not load the user profile for {result.CommerceUser.UserName}");

            AssociateSitecoreUserWithCommerceUser(result.CommerceUser, userProfile);
        }

        protected void AssociateSitecoreUserWithCommerceUser(CommerceUser commerceUser, UserProfile sitecoreUser)
        {
            var externalId = sitecoreUser[Demo.Retail.Foundation.Commerce.Website.Constants.Profile.SitecoreProfile.UserId];
            Assert.IsNotNullOrEmpty(externalId, $"The external id for the user {sitecoreUser.UserName} is empty.");
            commerceUser.ExternalId = externalId;

            if (commerceUser.Customers != null && commerceUser.Customers.Count != 0)
            {
                return;
            }

            var customers = new List<string> {commerceUser.ExternalId};
            commerceUser.Customers = customers.AsReadOnly();
        }

        protected UserProfile GetUserProfile(string userName)
        {
            return User.FromName(userName, true).Profile;
        }
    }
}