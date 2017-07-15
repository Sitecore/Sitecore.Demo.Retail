//-----------------------------------------------------------------------
// <copyright file="StorefrontUser.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the StorefrontUser class.</summary>
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
using Sitecore.Analytics;
using Sitecore.Commerce.Contacts;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Dictionary.Repositories;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public class CommerceUser : IUser
    {
        private const string StaticContactId = "{74E29FDC-8523-4C4F-B422-23BBFF0A342A}";
        private string _userId = string.Empty;

        public CommerceUser(ContactFactory contactFactory)
        {
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            ContactFactory = contactFactory;
        }

        private ContactFactory ContactFactory { get; }

        public string UserId
        {
            get { return string.IsNullOrEmpty(_userId) ? ContactId : _userId; }
            set { _userId = value; }
        }

        public string UserName => ContactFactory.GetContact();

        public Sitecore.Commerce.Entities.Customers.CommerceUser User { get; set; }

        private string ContactId
        {
            get
            {
                if (Tracker.Current != null && Tracker.Current.Contact != null && Tracker.Current.Contact.ContactId != Guid.Empty)
                {
                    return Tracker.Current.Contact.ContactId.ToString();
                }

                // Generate our own tracking id if needed for the experience editor.
                if (Context.PageMode.IsExperienceEditor)
                {
                    return GetExperienceEditorVisitorContactId();
                }

                throw new ConfigurationErrorsException(DictionaryPhraseRepository.Current.Get("/System Messages/Visitors/Tracking Not Enabled", "Xdb Tracking must be enabled."));
            }
        }

        private static string GetExperienceEditorVisitorContactId()
        {
            return StaticContactId;
        }

        public string FirstName => User?.FirstName;
        public string Email => User?.Email;
        public string LastName => User?.LastName;
        public string Phone => User?.GetPropertyValue("Phone")?.ToString();
    }
}