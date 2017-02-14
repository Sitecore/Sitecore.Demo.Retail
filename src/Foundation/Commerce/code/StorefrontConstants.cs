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
        public static class SystemMessages
        {
            public static readonly string AuthenticationProviderError = "AuthenticationProviderError";

            public static readonly string CartNotFoundError = "CartNotFoundError";

            public static readonly string CouldNotCreateUser = "CouldNotCreatedUser";

            public static readonly string CouldNotFindEmailBodyMessageError = "CouldNotFindEmailBodyMessageError";

            public static readonly string CouldNotFindEmailSubjectMessageError = "CouldNotFindEmailSubjectMessageError";

            public static readonly string CouldNotSentEmailError = "CouldNotSentEmailError";

            public static readonly string InvalidEmailError = "InvalidEmailError";

            public static readonly string InvalidPasswordError = "InvalidPasswordError";

            public static readonly string MailSentToMessage = "MailSentToMessage";

            public static readonly string MaxAddressLimitReached = "MaxAddresseLimitReached";

            public static readonly string MaxLoyaltyProgramsToJoinReached = "MaxLoyaltyProgramsToJoinReached";

            public static readonly string MaxWishListLineLimitReached = "MaxWishListLineLimitReached";

            public static readonly string MaxWishListLimitReached = "MaxWishListLimitReached";

            public static readonly string PasswordCouldNotBeReset = "PasswordCouldNotBeReset";

            public static readonly string PasswordRetrievalAnswerInvalid = "PasswordRetrievalAnswerInvalid";

            public static readonly string PasswordRetrievalQuestionInvalid = "PasswordRetrievalQuestionInvalid";

            public static readonly string SubmitOrderHasEmptyCart = "SubmitOrderHasEmptyCart";

            public static readonly string TrackingNotEnabled = "TrackingNotEnabled";

            public static readonly string UnknownMembershipProviderError = "UnknownMembershipProviderError";

            public static readonly string UpdateUserProfileError = "UpdateUserProfileError";

            public static readonly string UserAlreadyExists = "UserAlreadyExists";

            public static readonly string UserNameForEmailExists = "UserNameForEmailExists";

            public static readonly string UserNameInvalid = "UserNameInvalid";

            public static readonly string UserNotFoundError = "UserNotFoundError";

            public static readonly string UserRejectedError = "UserRejectedError";

            public static readonly string DefaultCurrencyNotSetException = "DefaultCurrencyNotSetException";

            public static readonly string InvalidCurrencyError = "InvalidCurrencyError";

            public static readonly string LoginFailed = "LoginFailed";

            public static readonly string AuthorizationCodeMissing = "AuthorizationCodeMissing";

            public static readonly string CancelPendingRequest = "CancelPendingRequest";

            public static readonly string AccountNotFound = "AccountNotFound";

            public static readonly string ActivationCodeSent = "ActivationCodeSent";

            public static readonly string WrongActivationCode = "WrongActivationCode";

            public static readonly string LinkupSucceeded = "LinkupSucceeded";

            public static readonly string CardAuthorizationFailed = "CardAuthorizationFailed";
        }

        public static class Settings
        {
            public static readonly string WebsiteName = "Storefront";

            public static readonly int DefaultItemsPerPage = 12;

            public static readonly string DefaultCurrencyCode = "USD";
        }

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

            public static readonly string ActivationCodeMissingMessage = "Activation Code Missing Message";

            public static readonly string ResendActivationCodeMessage = "Resend Activation Code Message";

            public static readonly string ActivationCode = "Activation Code";

            public static readonly string FacebookButton = "Facebook Button";

            public static readonly string FacebookText = "Facebook Text";

            public static readonly string ActivationText = "Activation Text";

            public static readonly string ActivateText = "Activate Text";

            public static readonly string LinkAccount = "Link Account";

            public static readonly string FirstName = "First Name";

            public static readonly string FirstNamePlaceholder = "First Name Placeholder";

            public static readonly string FillFormMessage = "Fill Form Message";

            public static readonly string GenerateSecureLink = "Generate Secure Link";

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

            public static readonly string MaxNumberOfAddresses = "Max Number of Addresses";

            public static readonly string MaxNumberOfWishLists = "Max Number of WishLists";

            public static readonly string MaxNumberOfWishListItems = "Max Number of WishList Items";

            public static readonly string UseIndexFileForProductStatusInLists = "Use Index File For Product Status In Lists";

            public static readonly string FormsAuthentication = "Forms Authentication";

            public static readonly string OperatingUnitNumber = "OperatingUnitNumber";

            public static readonly string MapKey = "Map Key";

            public static readonly string NamedSearches = "Named Searches";

            public static readonly string Title = "Title";

            public static readonly string ProductList = "Product List";

            public static readonly string CurrencyDescription = "Currency Description";

            public static readonly string CurrencySymbol = "Currency Symbol";

            public static readonly string CurrencySymbolPosition = "Currency Symbol Position";

            public static readonly string CurrencyNumberFormatCulture = "Currency Number Format Culture";

            public static readonly string SupportedCurrencies = "Supported Currencies";

            public static readonly string DefaultCurrency = "Default Currency";

            public static readonly string NewContosoAccount = "New Contoso Account";

            public static readonly string LinkContosoAccount = "Link Contoso Account";

            public static readonly string EmailAddressOfExistingCustomer = "Email Address Of Existing Customer";

            public static readonly string EmailOfExistingCustomer = "Email Of Existing Customer";

            public static readonly string EnterEmailForAccountAssociation = "EnterEmailForAccountAssociation";

            public static readonly string ContinueShoppingText = "Continue Shopping Text";

            public static readonly string SignOutText = "Sign Out Text";

            public static readonly string DisclaimerText = "Disclaimer Text";
        }

        public static class KnownTemplateItemIds
        {
            public static readonly ID Home = new ID("{FB9DBD60-CBA2-490D-9C72-997271D576A3}");

            public static readonly ID NamedSearch = new ID("{F3C0CD6C-9FA9-442D-BD3A-5A25E292F2F7}");

            public static readonly ID StandardPage = new ID("{16E859D2-6542-407A-AC65-F34BCAD3EB3D}");

            public static readonly ID SecuredPage = new ID("{02CCCF95-7BE5-4549-81F9-AC97A22D6816}");

            public static readonly ID SelectedProducts = new ID("{A45D0030-79F2-4DBF-9A74-226A33C58249}");
        }

        public static class KnowItemNames
        {
            public static readonly string Mails = "Mails";

            public static readonly string Lookups = "Lookups";

            public static readonly string SystemMessages = "System Messages";

            public static readonly string InventoryStatuses = "Inventory Statuses";

            public static readonly string Relationships = "Relationships";

            public static readonly string OrderStatuses = "Order Statuses";

            public static readonly string Currencies = "Currencies";

            public static readonly string CurrencyDisplay = "Currency Display";

            public static readonly string Payments = "Payments";

            public static readonly string Shipping = "Shipping";
        }

        public static class QueryStrings
        {
            public const string ConfirmationId = "confirmationId";

            public const string Paging = "pg";

            public const string SiteContentPaging = "scpg";

            public const string Sort = "s";

            public const string SortDirection = "sd";

            public const string Facets = "f";

            public const char FacetsSeparator = '|';

            public const string SearchKeyword = "q";

            public const string PageSize = "ps";

            public const string SiteContentPageSize = "scps";
        }

        public static class ItemFields
        {
            public static readonly string DisplayInSearchResults = "DisplayInSearchResults";

            public static readonly string Title = "Title";

            public static readonly string SummaryText = "SummaryText";
        }
    }
}