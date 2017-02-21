//-----------------------------------------------------------------------
// <copyright file="BaseController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the BaseController class.</summary>
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
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Contacts;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Alerts;
using Sitecore.Foundation.Alerts.Extensions;
using Sitecore.Foundation.Alerts.Models;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class BaseController : SitecoreController
    {
        private ICommerceSearchManager _currentSearchManager;
        private VisitorContext _currentVisitorContext;
        private ISiteContext _siteContext;

        public BaseController([NotNull] AccountManager accountManager, [NotNull] ContactFactory contactFactory)
        {
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));
            Assert.ArgumentNotNull(accountManager, nameof(accountManager));

            ContactFactory = contactFactory;
            AccountManager = accountManager;
        }

        public AccountManager AccountManager { get; set; }

        public ContactFactory ContactFactory { get; }

        public ISiteContext CurrentSiteContext => _siteContext ?? (_siteContext = CommerceTypeLoader.CreateInstance<ISiteContext>());

        public virtual VisitorContext CurrentVisitorContext
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

        public virtual void ValidateModel(BaseJsonResult result)
        {
            if (ModelState.IsValid)
                return;

            var errors = ModelState.Values.Where(modelValue => modelValue.Errors.Any()).SelectMany(modelValue => modelValue.Errors, (modelValue, error) => error.ErrorMessage).ToList();
            result.SetErrors(errors);
        }
    }
}