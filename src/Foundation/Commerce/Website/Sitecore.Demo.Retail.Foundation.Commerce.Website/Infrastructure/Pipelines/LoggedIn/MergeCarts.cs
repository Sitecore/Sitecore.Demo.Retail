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

using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Accounts.Pipelines;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Pipelines.LoggedIn
{
    [Service]
    public class MergeCarts
    {
        public MergeCarts(CommerceUserContext commerceUserContext, CartManager cartManager, StorefrontContext storefrontContext)
        {
            CommerceUserContext = commerceUserContext;
            CartManager = cartManager;
            StorefrontContext = storefrontContext;
        }

        public void Process(LoggedInPipelineArgs args)
        {
            if (this.StorefrontContext.Current == null)
                return;

            if (args.PreviousContactId == null)
                return;
            var previousContactId = args.PreviousContactId.ToString();
            if (CommerceUserContext.Current.UserId == previousContactId)
                return;
            var previousCart = CartManager.GetCart(previousContactId).Result;
            if (previousCart == null)
                return;
            this.CartManager.MergeCarts(CommerceUserContext.Current.UserId, previousContactId, previousCart);
        }

        public StorefrontContext StorefrontContext { get; }

        public CartManager CartManager { get; }

        public CommerceUserContext CommerceUserContext { get; }
    }
}