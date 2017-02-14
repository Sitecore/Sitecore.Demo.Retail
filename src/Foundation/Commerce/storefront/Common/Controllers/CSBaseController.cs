//-----------------------------------------------------------------------
// <copyright file="CSBaseController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the Commerce Server base controller.</summary>
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

using Sitecore.Commerce.Contacts;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class CSBaseController : BaseController
    {
        public CSBaseController([NotNull] AccountManager accountManager, [NotNull] ContactFactory contactFactory) : base(contactFactory)
        {
            Assert.ArgumentNotNull(accountManager, nameof(accountManager));

            AccountManager = accountManager;
        }

        public AccountManager AccountManager { get; set; }

        public override VisitorContext CurrentVisitorContext
        {
            get
            {
                // Setup the visitor context only once per HttpRequest.
                var siteContext = CurrentSiteContext;
                var visitorContext = siteContext.Items["__visitorContext"] as VisitorContext;
                if (visitorContext != null)
                {
                    return visitorContext;
                }
                visitorContext = new VisitorContext(ContactFactory);
                if (Context.User.IsAuthenticated && !Context.User.Profile.IsAdministrator)
                {
                    visitorContext.SetCommerceUser(AccountManager.ResolveCommerceUser().Result);
                }

                siteContext.Items["__visitorContext"] = visitorContext;

                return visitorContext;
            }
        }
    }
}