//-----------------------------------------------------------------------
// <copyright file="AccountManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the account business logic for the site.</summary>
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using Sitecore.Analytics;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Configuration;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Customers;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Security.Authentication;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class AccountManager : BaseManager
    {
        public AccountManager(CartManager cartManager, [NotNull] CustomerServiceProvider customerServiceProvider,
            [NotNull] ContactFactory contactFactory)
        {
            Assert.ArgumentNotNull(customerServiceProvider, "customerServiceProvider");
            Assert.ArgumentNotNull(contactFactory, "contactFactory");

            CartManager = cartManager;
            CustomerServiceProvider = customerServiceProvider;
            ContactFactory = contactFactory;
        }

        public CartManager CartManager { get; set; }

        public CustomerServiceProvider CustomerServiceProvider { get; protected set; }

        public ContactFactory ContactFactory { get; set; }

        public virtual bool Login([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext,
            [NotNull] string anonymousVisitorId, string userName, string password, bool persistent)
        {
            Assert.ArgumentNotNullOrEmpty(userName, "userName");
            Assert.ArgumentNotNullOrEmpty(password, "password");

            var anonymousVisitorCart = CartManager.GetCurrentCart(storefront, anonymousVisitorId).Result;

            var isLoggedIn = AuthenticationManager.Login(userName, password, persistent);
            if (isLoggedIn)
            {
                if (Tracker.Current != null)
                    Tracker.Current.Session.Identify(userName);
                visitorContext.SetCommerceUser(ResolveCommerceUser().Result);
                CartManager.MergeCarts(storefront, visitorContext, anonymousVisitorId, anonymousVisitorCart);
            }

            return isLoggedIn;
        }

        public virtual void Logout()
        {
            if (Tracker.Current != null)
                Tracker.Current.EndVisit(true);
            HttpContext.Current.Session.Abandon();
            AuthenticationManager.Logout();
        }

        public virtual ManagerResponse<GetUserResult, CommerceUser> GetUser(string userName)
        {
            Assert.ArgumentNotNullOrEmpty(userName, "userName");

            var request = new GetUserRequest(userName);
            var result = CustomerServiceProvider.GetUser(request);
            if (!result.Success || result.CommerceUser == null)
            {
                var message = StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UserNotFoundError);
                result.SystemMessages.Add(new SystemMessage {Message = message});
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetUserResult, CommerceUser>(result, result.CommerceUser);
        }

        public virtual ManagerResponse<DeleteUserResult, bool> DeleteUser([NotNull] CommerceStorefront storefront,
            [NotNull] VisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");

            var userName = visitorContext.UserName;
            var commerceUser = GetUser(userName).Result;

            if (commerceUser == null)
                return new ManagerResponse<DeleteUserResult, bool>(new DeleteUserResult {Success = false}, false);

            // NOTE: we do not need to call DeleteCustomer because this will delete the commerce server user under the covers
            var request = new DeleteUserRequest(new CommerceUser {UserName = userName});
            var result = CustomerServiceProvider.DeleteUser(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<DeleteUserResult, bool>(result, result.Success);
        }

        public virtual ManagerResponse<UpdateUserResult, CommerceUser> UpdateUser([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, ProfileModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");
            Assert.ArgumentNotNull(inputModel, "inputModel");

            UpdateUserResult result;

            var userName = visitorContext.UserName;
            var commerceUser = GetUser(userName).Result;
            if (commerceUser != null)
            {
                commerceUser.FirstName = inputModel.FirstName;
                commerceUser.LastName = inputModel.LastName;
                commerceUser.Email = inputModel.Email;
                commerceUser.SetPropertyValue("Phone", inputModel.TelephoneNumber);

                try
                {
                    var request = new UpdateUserRequest(commerceUser);
                    result = CustomerServiceProvider.UpdateUser(request);
                }
                catch (Exception ex)
                {
                    result = new UpdateUserResult {Success = false};
                    result.SystemMessages.Add(new SystemMessage {Message = ex.Message + "/" + ex.StackTrace});
                }
            }
            else
            {
                // user is authenticated, but not in the CommerceUsers domain - probably here because we are in edit or preview mode
                var message =
                    StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UpdateUserProfileError);
                message = string.Format(CultureInfo.InvariantCulture, message, Context.User.LocalName);
                result = new UpdateUserResult {Success = false};
                result.SystemMessages.Add(new SystemMessage {Message = message});
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<UpdateUserResult, CommerceUser>(result, result.CommerceUser);
        }

        public virtual ManagerResponse<GetPartiesResult, IEnumerable<CommerceParty>> GetParties(
            [NotNull] CommerceStorefront storefront, [NotNull] CommerceCustomer customer)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(customer, "user");

            var request = new GetPartiesRequest(customer);
            var result = CustomerServiceProvider.GetParties(request);
            var partyList = result.Success && result.Parties != null
                ? result.Parties.Cast<CommerceParty>()
                : new List<CommerceParty>();

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetPartiesResult, IEnumerable<CommerceParty>>(result, partyList);
        }

        public virtual ManagerResponse<GetPartiesResult, IEnumerable<CommerceParty>> GetCurrentCustomerParties(
            [NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");

            var result = new GetPartiesResult {Success = false};
            var getUserResponse = GetUser(visitorContext.UserName);
            if (!getUserResponse.ServiceProviderResult.Success || getUserResponse.Result == null)
            {
                result.SystemMessages.ToList().AddRange(getUserResponse.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<GetPartiesResult, IEnumerable<CommerceParty>>(result, null);
            }

            return GetParties(storefront, new CommerceCustomer {ExternalId = getUserResponse.Result.ExternalId});
        }

        public virtual ManagerResponse<CustomerResult, bool> RemoveParties([NotNull] CommerceStorefront storefront,
            [NotNull] CommerceCustomer user, List<CommerceParty> parties)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(user, "user");
            Assert.ArgumentNotNull(parties, "parties");

            var request = new RemovePartiesRequest(user, parties.Cast<Party>().ToList());
            var result = CustomerServiceProvider.RemoveParties(request);
            result.WriteToSitecoreLog();
            return new ManagerResponse<CustomerResult, bool>(result, result.Success);
        }

        public virtual ManagerResponse<CustomerResult, bool> RemovePartiesFromCurrentUser(
            [NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext,
            [NotNull] string addressExternalId)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");
            Assert.ArgumentNotNullOrEmpty(addressExternalId, "addresseExternalId");

            var getUserResponse = GetUser(Context.User.Name);
            if (getUserResponse.Result == null)
            {
                var customerResult = new CustomerResult {Success = false};
                customerResult.SystemMessages.ToList().AddRange(getUserResponse.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<CustomerResult, bool>(customerResult, false);
            }

            var customer = new CommerceCustomer {ExternalId = getUserResponse.Result.ExternalId};
            var parties = new List<CommerceParty> {new CommerceParty {ExternalId = addressExternalId}};

            return RemoveParties(storefront, customer, parties);
        }

        public virtual ManagerResponse<CustomerResult, bool> UpdateParties([NotNull] CommerceStorefront storefront,
            [NotNull] CommerceCustomer user, List<Party> parties)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(user, "user");
            Assert.ArgumentNotNull(parties, "parties");

            var request = new UpdatePartiesRequest(user, parties);
            var result = CustomerServiceProvider.UpdateParties(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CustomerResult, bool>(result, result.Success);
        }

        public virtual ManagerResponse<AddPartiesResult, bool> AddParties([NotNull] CommerceStorefront storefront,
            [NotNull] CommerceCustomer user, List<Party> parties)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(user, "user");
            Assert.ArgumentNotNull(parties, "parties");

            var request = new AddPartiesRequest(user, parties);
            var result = CustomerServiceProvider.AddParties(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<AddPartiesResult, bool>(result, result.Success);
        }

        public virtual ManagerResponse<CreateUserResult, CommerceUser> RegisterUser(
            [NotNull] CommerceStorefront storefront, RegisterUserInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(inputModel, "inputModel");
            Assert.ArgumentNotNullOrEmpty(inputModel.UserName, "inputModel.UserName");
            Assert.ArgumentNotNullOrEmpty(inputModel.Password, "inputModel.Password");

            CreateUserResult result;

            // Attempt to register the user
            try
            {
                var request = new CreateUserRequest(inputModel.UserName, inputModel.Password, inputModel.UserName,
                    storefront.ShopName);
                result = CustomerServiceProvider.CreateUser(request);
                result.WriteToSitecoreLog();

                if (result.Success && result.CommerceUser == null && !result.SystemMessages.Any())
                {
                    // Connect bug:  This is a work around to a Connect bug.  When the user already exists,connect simply aborts the pipeline but
                    // does not set the success flag nor does it return an error message.
                    result.Success = false;
                    result.SystemMessages.Add(new SystemMessage
                    {
                        Message =
                            StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UserAlreadyExists)
                    });
                }
            }
            catch (MembershipCreateUserException e)
            {
                result = new CreateUserResult {Success = false};
                result.SystemMessages.Add(new SystemMessage {Message = ErrorCodeToString(e.StatusCode)});
            }

            return new ManagerResponse<CreateUserResult, CommerceUser>(result, result.CommerceUser);
        }

        public virtual ManagerResponse<UpdatePasswordResult, bool> UpdateUserPassword(
            [NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext,
            ChangePasswordInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");
            Assert.ArgumentNotNull(inputModel, "inputModel");
            Assert.ArgumentNotNullOrEmpty(inputModel.OldPassword, "inputModel.OldPassword");
            Assert.ArgumentNotNullOrEmpty(inputModel.NewPassword, "inputModel.NewPassword");

            var userName = visitorContext.UserName;

            var request = new UpdatePasswordRequest(userName, inputModel.OldPassword, inputModel.NewPassword);
            var result = CustomerServiceProvider.UpdatePassword(request);
            if (!result.Success && !result.SystemMessages.Any())
            {
                var message =
                    StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.PasswordCouldNotBeReset);
                result.SystemMessages.Add(new SystemMessage {Message = message});
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<UpdatePasswordResult, bool>(result, result.Success);
        }

        public virtual ManagerResponse<UpdatePasswordResult, bool> ResetUserPassword(
            [NotNull] CommerceStorefront storefront, ForgotPasswordInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(inputModel, "inputModel");
            Assert.ArgumentNotNullOrEmpty(inputModel.Email, "inputModel.Email");

            var result = new UpdatePasswordResult {Success = true};

            var getUserResponse = GetUser(inputModel.Email);
            if (!getUserResponse.ServiceProviderResult.Success || getUserResponse.Result == null)
            {
                result.Success = false;
                foreach (var systemMessage in getUserResponse.ServiceProviderResult.SystemMessages)
                    result.SystemMessages.Add(systemMessage);
            }
            else
            {
                try
                {
                    var userIpAddress = HttpContext.Current != null
                        ? HttpContext.Current.Request.UserHostAddress
                        : string.Empty;
                    var provisionalPassword = Membership.Provider.ResetPassword(getUserResponse.Result.UserName,
                        string.Empty);

                    var mailUtil = new MailUtil();
                    var wasEmailSent = mailUtil.SendMail("ForgotPassword", inputModel.Email,
                        storefront.SenderEmailAddress, new object(), new object[] {userIpAddress, provisionalPassword});
                    if (!wasEmailSent)
                    {
                        var message =
                            StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.CouldNotSentEmailError);
                        result.Success = false;
                        result.SystemMessages.Add(new SystemMessage {Message = message});
                    }
                }
                catch (Exception e)
                {
                    result.Success = false;
                    result.SystemMessages.Add(new SystemMessage {Message = e.Message});
                }

                result.WriteToSitecoreLog();
            }

            return new ManagerResponse<UpdatePasswordResult, bool>(result, result.Success);
        }

        public virtual ManagerResponse<CustomerResult, bool> SetPrimaryAddress([NotNull] CommerceStorefront storefront,
            [NotNull] VisitorContext visitorContext, [NotNull] string addressExternalId)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");
            Assert.ArgumentNotNullOrEmpty(addressExternalId, "addressExternalId");

            var userPartiesResponse = GetCurrentCustomerParties(storefront, visitorContext);
            if (userPartiesResponse.ServiceProviderResult.Success)
            {
                var customerResult = new CustomerResult {Success = false};
                customerResult.SystemMessages.ToList()
                    .AddRange(userPartiesResponse.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<CustomerResult, bool>(customerResult, false);
            }

            var addressesToUpdate = new List<CommerceParty>();

            var notPrimary = userPartiesResponse.Result.SingleOrDefault(address => address.IsPrimary);
            if (notPrimary != null)
            {
                notPrimary.IsPrimary = false;
                addressesToUpdate.Add(notPrimary);
            }

            var primaryAddress = userPartiesResponse.Result.Single(address => address.PartyId == addressExternalId);

            //primaryAddress.IsPrimary = true;
            addressesToUpdate.Add(primaryAddress);

            var updatePartiesResponse = UpdateParties(storefront,
                new CommerceCustomer {ExternalId = visitorContext.UserId}, addressesToUpdate.Cast<Party>().ToList());

            return new ManagerResponse<CustomerResult, bool>(updatePartiesResponse.ServiceProviderResult,
                updatePartiesResponse.Result);
        }

        public ManagerResponse<GetUserResult, CommerceUser> ResolveCommerceUser()
        {
            if (Tracker.Current == null || Tracker.Current.Contact == null ||
                Tracker.Current.Contact.ContactId == Guid.Empty)
                return new ManagerResponse<GetUserResult, CommerceUser>(new GetUserResult {Success = true},
                    new CommerceUser {FirstName = "Test", LastName = "User"});

            var userName = ContactFactory.GetContact();
            var response = GetUser(userName);
            var commerceUser = response.Result;

            Assert.IsNotNull(commerceUser, "The user '{0}' could not be found.", userName);

            return new ManagerResponse<GetUserResult, CommerceUser>(response.ServiceProviderResult, commerceUser);
        }

        protected virtual string UpdateUserName(string userName)
        {
            var defaultDomain = CommerceServerSitecoreConfig.Current.DefaultCommerceUsersDomain;
            if (string.IsNullOrWhiteSpace(defaultDomain))
                defaultDomain = CommerceConstants.ProfilesStrings.CommerceUsersDomainName;

            return !userName.StartsWith(defaultDomain, StringComparison.OrdinalIgnoreCase)
                ? string.Concat(defaultDomain, @"\", userName)
                : userName;
        }

        protected virtual string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UserAlreadyExists);

                case MembershipCreateStatus.DuplicateEmail:
                    return StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UserNameForEmailExists);

                case MembershipCreateStatus.InvalidPassword:
                    return StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.InvalidPasswordError);

                case MembershipCreateStatus.InvalidEmail:
                    return StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.InvalidEmailError);

                case MembershipCreateStatus.InvalidAnswer:
                    return
                        StorefrontManager.GetSystemMessage(
                            StorefrontConstants.SystemMessages.PasswordRetrievalAnswerInvalid);

                case MembershipCreateStatus.InvalidQuestion:
                    return
                        StorefrontManager.GetSystemMessage(
                            StorefrontConstants.SystemMessages.PasswordRetrievalQuestionInvalid);

                case MembershipCreateStatus.InvalidUserName:
                    return StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UserNameInvalid);

                case MembershipCreateStatus.ProviderError:
                    return
                        StorefrontManager.GetSystemMessage(
                            StorefrontConstants.SystemMessages.AuthenticationProviderError);

                case MembershipCreateStatus.UserRejected:
                    return StorefrontManager.GetSystemMessage(StorefrontConstants.SystemMessages.UserRejectedError);

                default:
                    return
                        StorefrontManager.GetSystemMessage(
                            StorefrontConstants.SystemMessages.UnknownMembershipProviderError);
            }
        }
    }
}