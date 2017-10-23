//---------------------------------------------------------------------
// <copyright file="VaryByCurrency.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Handles the "Vary by Currency" cacheable option.</summary>
//---------------------------------------------------------------------
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

using Sitecore.Foundation.Accounts.Pipelines;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure.Pipelines.LoggedIn
{
    [Service]
    public class SetCommerceUser
    {
        public SetCommerceUser(CommerceUserContext commerceUserContext, AccountManager accountManager, StorefrontContext storefrontContext)
        {
            CommerceUserContext = commerceUserContext;
            AccountManager = accountManager;
            StorefrontContext = storefrontContext;
        }

        public void Process(LoggedInPipelineArgs args)
        {
            if (this.StorefrontContext.Current == null)
                return;
            var user = AccountManager.ResolveCommerceUser();
            if (user == null)
                return;
            CommerceUserContext.SetUser(user);
        }

        public StorefrontContext StorefrontContext { get; }

        public AccountManager AccountManager { get; }

        public CommerceUserContext CommerceUserContext { get; }
    }
}