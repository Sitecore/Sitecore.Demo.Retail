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
using System.Configuration;
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
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Security.Authentication;
using CommerceUser = Sitecore.Commerce.Entities.Customers.CommerceUser;

namespace Sitecore.Foundation.Commerce.Website.Managers
{
    public class AccountManager : IManager
    {
        public AccountManager(CustomerServiceProvider customerServiceProvider, ContactFactory contactFactory, MailManager mailManager, StorefrontContext storefrontContext)
        {
            CustomerServiceProvider = customerServiceProvider;
            ContactFactory = contactFactory;
            MailManager = mailManager;
            StorefrontContext = storefrontContext;
        }

        private MailManager MailManager { get; }
        private CustomerServiceProvider CustomerServiceProvider { get; }
        private ContactFactory ContactFactory { get; }
        private StorefrontContext StorefrontContext { get; }

#warning Refactor to use Habitat login
        public bool Login(string userName, string password, bool persistent)
        {
            Assert.ArgumentNotNullOrEmpty(userName, nameof(userName));
            Assert.ArgumentNotNullOrEmpty(password, nameof(password));

            if (!AuthenticationManager.Login(userName, password, persistent))
            {
                return false;
            }
            if (Tracker.Current != null)
            {
                Tracker.Current.Session.Identify(userName);
            }

            return true;
        }

#warning Refactor to use Habitat logout
        public void Logout()
        {
            if (Tracker.Current != null)
            {
                Tracker.Current.EndVisit(true);
            }
            AuthenticationManager.Logout();
        }

        public ManagerResponse<GetUserResult, CommerceUser> GetUser(string userName)
        {
            Assert.ArgumentNotNullOrEmpty(userName, nameof(userName));

            var request = new GetUserRequest(userName);
            var result = CustomerServiceProvider.GetUser(request);
            if (!result.Success || result.CommerceUser == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/User Not Found Error", "The email address does not exists");
                result.SystemMessages.Add(new SystemMessage {Message = message});
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetUserResult, CommerceUser>(result, result.CommerceUser);
        }

        public ManagerResponse<DeleteUserResult, bool> DeleteUser(string userName)
        {
            var commerceUser = GetUser(userName).Result;

            if (commerceUser == null)
            {
                return new ManagerResponse<DeleteUserResult, bool>(new DeleteUserResult {Success = false}, false);
            }

            // NOTE: we do not need to call DeleteCustomer because this will delete the commerce server user under the covers
            var request = new DeleteUserRequest(new CommerceUser {UserName = userName});
            var result = CustomerServiceProvider.DeleteUser(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<DeleteUserResult, bool>(result, result.Success);
        }

        public ManagerResponse<UpdateUserResult, CommerceUser> UpdateUser(string userName, ProfileModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            UpdateUserResult result;

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
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Update User Profile Error", "Cannot update profile details for user {0}.");
                message = string.Format(message, Context.User.LocalName);
                result = new UpdateUserResult {Success = false};
                result.SystemMessages.Add(new SystemMessage {Message = message});
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<UpdateUserResult, CommerceUser>(result, result.CommerceUser);
        }

        public ManagerResponse<GetPartiesResult, IEnumerable<IParty>> GetParties(CommerceCustomer customer)
        {
            Assert.ArgumentNotNull(customer, nameof(customer));

            var request = new GetPartiesRequest(customer);
            var result = CustomerServiceProvider.GetParties(request);
            var partyList = result.Success && result.Parties != null ? result.Parties.Select(p => p.ToEntity()) : new List<IParty>();

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetPartiesResult, IEnumerable<IParty>>(result, partyList);
        }

        public ManagerResponse<GetPartiesResult, IEnumerable<IParty>> GetCustomerParties(string userName)
        {
            var result = new GetPartiesResult {Success = false};
            var getUserResponse = GetUser(userName);
            if (!getUserResponse.ServiceProviderResult.Success || getUserResponse.Result == null)
            {
                result.SystemMessages.ToList().AddRange(getUserResponse.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<GetPartiesResult, IEnumerable<IParty>>(result, null);
            }

            return GetParties(new CommerceCustomer {ExternalId = getUserResponse.Result.ExternalId});
        }

        public ManagerResponse<CustomerResult, bool> RemoveParties(CommerceCustomer user, List<CommerceParty> parties)
        {
            Assert.ArgumentNotNull(user, nameof(user));
            Assert.ArgumentNotNull(parties, nameof(parties));

            var request = new RemovePartiesRequest(user, parties.Cast<Party>().ToList());
            var result = CustomerServiceProvider.RemoveParties(request);
            result.WriteToSitecoreLog();
            return new ManagerResponse<CustomerResult, bool>(result, result.Success);
        }

        public ManagerResponse<CustomerResult, bool> RemovePartiesFromUser(string userName, string addressExternalId)
        {
            Assert.ArgumentNotNullOrEmpty(addressExternalId, nameof(addressExternalId));

            var getUserResponse = GetUser(userName);
            if (getUserResponse.Result == null)
            {
                var customerResult = new CustomerResult {Success = false};
                customerResult.SystemMessages.ToList().AddRange(getUserResponse.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<CustomerResult, bool>(customerResult, false);
            }

            var customer = new CommerceCustomer {ExternalId = getUserResponse.Result.ExternalId};
            var parties = new List<CommerceParty> {new CommerceParty {ExternalId = addressExternalId}};

            return RemoveParties(customer, parties);
        }

        public ManagerResponse<CustomerResult, bool> UpdateParties(string userName, List<IParty> parties)
        {
            Assert.ArgumentNotNull(userName, nameof(userName));
            Assert.ArgumentNotNull(parties, nameof(parties));

            var getUserResponse = GetUser(userName);
            if (getUserResponse.Result == null)
            {
                throw new ArgumentException("User not found, invalid userName", nameof(userName));
            }
            var customer = new CommerceCustomer {ExternalId = getUserResponse.Result.ExternalId};
            var request = new UpdatePartiesRequest(customer, parties.Select(p => p.ToCommerceParty()).ToList());
            var result = CustomerServiceProvider.UpdateParties(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CustomerResult, bool>(result, result.Success);
        }

        public ManagerResponse<AddPartiesResult, bool> AddParties(string userName, List<IParty> parties)
        {
            Assert.ArgumentNotNull(userName, nameof(userName));
            Assert.ArgumentNotNull(parties, nameof(parties));

            var getUserResponse = GetUser(userName);
            if (getUserResponse.Result == null)
            {
                throw new ArgumentException("User not found, invalid userName", nameof(userName));
            }
            var customer = new CommerceCustomer { ExternalId = getUserResponse.Result.ExternalId };
            var request = new AddPartiesRequest(customer, parties.Select(a => a.ToCommerceParty()).ToList());
            var result = CustomerServiceProvider.AddParties(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<AddPartiesResult, bool>(result, result.Success);
        }

#warning Refactor to use Habitat register
        public ManagerResponse<CreateUserResult, CommerceUser> RegisterUser(RegisterUserInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.UserName, nameof(inputModel.UserName));
            Assert.ArgumentNotNullOrEmpty(inputModel.Password, nameof(inputModel.Password));
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            CreateUserResult result;

            // Attempt to register the user
            try
            {
                var request = new CreateUserRequest(inputModel.UserName, inputModel.Password, inputModel.UserName, StorefrontContext.Current.ShopName);
                result = CustomerServiceProvider.CreateUser(request);
                result.WriteToSitecoreLog();

                if (result.Success && result.CommerceUser == null && !result.SystemMessages.Any())
                {
                    // Connect bug:  This is a work around to a Connect bug.  When the user already exists,connect simply aborts the pipeline but
                    // does not set the success flag nor does it return an error message.
                    result.Success = false;
                    result.SystemMessages.Add(new SystemMessage
                    {
                        Message = DictionaryPhraseRepository.Current.Get("/System Messages/Accounts/User Already Exists", "User name already exists. Please enter a different user name.")
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

#warning Refactor to use Habitat change password
        public ManagerResponse<UpdatePasswordResult, bool> UpdateUserPassword(string userName, ChangePasswordInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.OldPassword, nameof(inputModel.OldPassword));
            Assert.ArgumentNotNullOrEmpty(inputModel.NewPassword, nameof(inputModel.NewPassword));

            var request = new UpdatePasswordRequest(userName, inputModel.OldPassword, inputModel.NewPassword);
            var result = CustomerServiceProvider.UpdatePassword(request);
            if (!result.Success && !result.SystemMessages.Any())
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Accounts/Password Could Not Be Reset", "Your password could not be reset. Please verify the data you entered");
                result.SystemMessages.Add(new SystemMessage {Message = message});
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<UpdatePasswordResult, bool>(result, result.Success);
        }

#warning Refactor to use Habitat forgot password
        public ManagerResponse<UpdatePasswordResult, bool> ResetUserPassword(ForgotPasswordInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.Email, nameof(inputModel.Email));

            var result = new UpdatePasswordResult {Success = true};

            var getUserResponse = GetUser(inputModel.Email);
            if (!getUserResponse.ServiceProviderResult.Success || getUserResponse.Result == null)
            {
                result.Success = false;
                foreach (var systemMessage in getUserResponse.ServiceProviderResult.SystemMessages)
                {
                    result.SystemMessages.Add(systemMessage);
                }
            }
            else
            {
                try
                {
                    var userIpAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : string.Empty;
                    var provisionalPassword = Membership.Provider.ResetPassword(getUserResponse.Result.UserName,
                                                                                string.Empty);

                    var wasEmailSent = MailManager.SendMail("ForgotPassword", inputModel.Email, userIpAddress, provisionalPassword);

                    if (!wasEmailSent)
                    {
                        var message = DictionaryPhraseRepository.Current.Get("/System Messages/Accounts/Could Not Send Email Error", "Sorry, the email could not be sent");
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

        public ManagerResponse<CustomerResult, bool> SetPrimaryAddress(string userName, string addressExternalId)
        {
            Assert.ArgumentNotNullOrEmpty(addressExternalId, nameof(addressExternalId));

            var userPartiesResponse = GetCustomerParties(userName);
            if (userPartiesResponse.ServiceProviderResult.Success)
            {
                var customerResult = new CustomerResult {Success = false};
                customerResult.SystemMessages.ToList().AddRange(userPartiesResponse.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<CustomerResult, bool>(customerResult, false);
            }

            var addressesToUpdate = new List<IParty>();

            var currentPrimary = userPartiesResponse.Result.SingleOrDefault(address => address.IsPrimary);
            if (currentPrimary != null)
            {
                currentPrimary.IsPrimary = false;
                addressesToUpdate.Add(currentPrimary);
            }

            var primaryAddress = userPartiesResponse.Result.Single(address => address.PartyId == addressExternalId);
            primaryAddress.IsPrimary = true;
            addressesToUpdate.Add(primaryAddress);

            var updatePartiesResponse = UpdateParties(userName, addressesToUpdate);

            return new ManagerResponse<CustomerResult, bool>(updatePartiesResponse.ServiceProviderResult, updatePartiesResponse.Result);
        }

        public CommerceUser ResolveCommerceUser()
        {
            if (Tracker.Current == null || Tracker.Current.Contact == null || Tracker.Current.Contact.ContactId == Guid.Empty)
            {
                return null;
            }

            var userName = ContactFactory.GetContact();
            var response = GetUser(userName);
            return response.Result;
        }

        private string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/User Already Exists", "User name already exists. Please enter a different user name.");
                case MembershipCreateStatus.DuplicateEmail:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/User Name For Email Exists", "A user name for that e-mail address already exists. Please enter a different e-mail address.");
                case MembershipCreateStatus.InvalidPassword:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Invalid Password Error", "The password provided is invalid. Please enter a valid password value.");
                case MembershipCreateStatus.InvalidEmail:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Invalid Email Error", "The e-mail address provided is invalid. Please check the value and try again.");
                case MembershipCreateStatus.InvalidAnswer:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Password Retrieval Answer Invalid", "The password retrieval answer provided is invalid. Please check the value and try again.");
                case MembershipCreateStatus.InvalidQuestion:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Password Retrieval Question Invalid", "The password retrieval question provided is invalid. Please check the value and try again.");
                case MembershipCreateStatus.InvalidUserName:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/User Name Invalid", "The user name provided is invalid. Please check the value and try again.");
                case MembershipCreateStatus.ProviderError:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Authentication Provider Error", "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                case MembershipCreateStatus.UserRejected:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/User Rejected Error", "The user creation request has been cancelled. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                default:
                    return DictionaryPhraseRepository.Current.Get("/System Messages/Account Manager/Unknown Membership Provider Error", "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
            }
        }

        public string GetCommerceUsersDomain()
        {
            var defaultDomain = CommerceServerSitecoreConfig.Current.DefaultCommerceUsersDomain;
            if (string.IsNullOrWhiteSpace(defaultDomain))
            {
                defaultDomain = CommerceConstants.ProfilesStrings.CommerceUsersDomainName;
            }
            if (string.IsNullOrEmpty(defaultDomain))
            {
                throw new ConfigurationErrorsException("Cannot determine the commerce user domain. Please check your configuration.");
            }
            return defaultDomain;
        }
    }
}