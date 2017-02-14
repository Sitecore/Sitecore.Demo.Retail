//-----------------------------------------------------------------------
// <copyright file="VisitorContext.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the VisitorContext class.</summary>
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
using System.Configuration;
using System.Linq;
using Sitecore.Analytics;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Customers;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Util;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class VisitorContext
    {
        private const string StaticVisitorId = "{74E29FDC-8523-4C4F-B422-23BBFF0A342A}";
        private string _userId = string.Empty;

        public VisitorContext([NotNull] ContactFactory contactFactory)
        {
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            ContactFactory = contactFactory;
        }

        public ContactFactory ContactFactory { get; protected set; }

        public string UserId => string.IsNullOrEmpty(_userId) ? VisitorId : _userId;

        public string UserName => ContactFactory.GetContact();

        public CommerceUser CommerceUser { get; private set; }

        public string VisitorId
        {
            get
            {
                if (Tracker.Current != null && Tracker.Current.Contact != null &&
                    Tracker.Current.Contact.ContactId != Guid.Empty)
                {
                    return Tracker.Current.Contact.ContactId.ToString();
                }

                // Generate our own tracking id if needed for the experience editor.
                if (Context.PageMode.IsExperienceEditor)
                {
                    return GetExperienceEditorVisitorTrackingId();
                }

                throw new ConfigurationErrorsException(
                    StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.TrackingNotEnabled));
            }
        }

        public string ShoppingCartId
        {
            get
            {
                if (Context.User.IsAuthenticated)
                {
                    return Guid.NewGuid().ToString();
                }

                return CartCookieHelper.GetAnonymousCartIdFromCookie();
            }
        }

        public string GetCustomerId()
        {
            return UserId;
        }

        public void SetCommerceUser(CommerceUser user)
        {
            if (Tracker.Current == null || Tracker.Current.Contact == null ||
                Tracker.Current.Contact.ContactId == Guid.Empty)
            {
                return;
            }

            Assert.IsNotNull(ContactFactory, "this.ContactFactory should not be null.");

            CommerceUser = user;

            Assert.IsNotNull(CommerceUser.Customers, "The user '{0}' does not contain a Customers collection.",
                user.UserName);

            _userId = CommerceUser.Customers.FirstOrDefault();
        }

        private static string GetExperienceEditorVisitorTrackingId()
        {
            return StaticVisitorId;
        }
    }
}