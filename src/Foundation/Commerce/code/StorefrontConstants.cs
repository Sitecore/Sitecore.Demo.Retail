//-----------------------------------------------------------------------
// <copyright file="StorefrontConstants.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Storefront constant definition.</summary>
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
using Sitecore.Data;

namespace Sitecore.Foundation.Commerce
{
    [Obsolete("Move to Templates.cs or similar")]
    public static class StorefrontConstants
    {
        [Obsolete("Move to Templates.cs or similar")]
        public static class Settings
        {
            public static readonly string DefaultCurrencyCode = "USD";
        }

        [Obsolete("Move to Templates.cs or similar")]
        public static class KnownFieldNames
        {
            public static readonly string Cancel = "Cancel";

            public static readonly string CreateUser = "Create user";

            public static readonly string ActivateUser = "Activate user";

            public static readonly string CustomerMessage1 = "Customer Message 1";

            public static readonly string CustomerMessage2 = "Customer Message 2";

            public static readonly string Email = "Email";

            public static readonly string EmailAddressPlaceholder = "Email Address Placeholder";

            public static readonly string EmailMissingMessage = "Email Missing Message";

            public static readonly string FirstNameMissingMessage = "First Name Missing Message";

            public static readonly string LastNameMissingMessage = "Last Name Missing Message";

            public static readonly string FacebookButton = "Facebook Button";

            public static readonly string FacebookText = "Facebook Text";

            public static readonly string ActivationText = "Activation Text";

            public static readonly string ActivateText = "Activate Text";

            public static readonly string LinkAccount = "Link Account";

            public static readonly string FirstName = "First Name";

            public static readonly string FirstNamePlaceholder = "First Name Placeholder";

            public static readonly string FillFormMessage = "Fill Form Message";

            public static readonly string GuestCheckoutButton = "Guest Checkout Button";

            public static readonly string LastName = "Last Name";

            public static readonly string LastNamePlaceholder = "Last Name Placeholder";

            public static readonly string Password = "Password";

            public static readonly string PasswordsDoNotMatchMessage = "Passwords Do Not Match Message";

            public static readonly string PasswordLengthMessage = "Password Length Message";

            public static readonly string PasswordMissingMessage = "Password Missing Message";

            public static readonly string PasswordAgain = "Password Again";

            public static readonly string PasswordPlaceholder = "Password Placeholder";

            public static readonly string Registering = "Registering";

            public static readonly string Activating = "Activating";

            public static readonly string SignInButton = "Sign In Button";

            public static readonly string SigningButton = "Signing Button";

            public static readonly string ShowWhenAuthenticated = "Show when Authenticated";

            public static readonly string ShowAlways = "Show Always";

            public static readonly string Body = "Body";

            public static readonly string Subject = "Subject";

            public static readonly string Key = "Key";

            public static readonly string Value = "Value";

            public static readonly string SenderEmailAddress = "Sender Email Address";

            public static readonly string MapKey = "Map Key";

            public static readonly string NamedSearches = "Named Searches";

            public static readonly string EmailAddressOfExistingCustomer = "Email Address Of Existing Customer";

            public static readonly string EmailOfExistingCustomer = "Email Of Existing Customer";

            public static readonly string EnterEmailForAccountAssociation = "EnterEmailForAccountAssociation";

            public static readonly string ContinueShoppingText = "Continue Shopping Text";

            public static readonly string SignOutText = "Sign Out Text";

            public static readonly string DisclaimerText = "Disclaimer Text";
        }

        [Obsolete("Move to Templates.cs or similar")]
        public static class KnowItemNames
        {
            public static readonly string Mails = "Mails";

            public static readonly string Lookups = "Lookups";

            public static readonly string InventoryStatuses = "Inventory Statuses";

            public static readonly string Relationships = "Relationships";

            public static readonly string OrderStatuses = "Order Statuses";

            public static readonly string Payments = "Payments";

            public static readonly string Shipping = "Shipping";
        }
    }
}