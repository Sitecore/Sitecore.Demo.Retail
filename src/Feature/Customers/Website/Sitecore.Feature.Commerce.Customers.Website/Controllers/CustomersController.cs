//-----------------------------------------------------------------------
// <copyright file="AccountController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the AccountController class.</summary>
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
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Customers.Website.Models;
using Sitecore.Foundation.Commerce.Website;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Foundation.SitecoreExtensions.Attributes;
using Sitecore.Links;
using Sitecore.Mvc.Controllers;

namespace Sitecore.Feature.Commerce.Customers.Website.Controllers
{
    public class CustomersController : SitecoreController
    {
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess
        }

        public CustomersController(AccountManager accountManager, CountryManager countryManager, CommerceUserContext commerceUserContext, StorefrontContext storefrontContext)
        {
            AccountManager = accountManager;
            CommerceUserContext = commerceUserContext;
            StorefrontContext = storefrontContext;
            CountryManager = countryManager;
        }

        private CountryManager CountryManager { get; }
        private AccountManager AccountManager { get; }
        private CommerceUserContext CommerceUserContext { get; }
        public StorefrontContext StorefrontContext { get; }
        public int MaxNumberOfAddresses => Settings.GetIntSetting("Commerce.MaxNumberOfAddresses", 10);

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult LogOff()
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LogOffAndRedirect()
        {
            AccountManager.Logout();

            return RedirectToLocal("/");
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult Addresses()
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            return View();
        }

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult EditProfile()
        {
            var model = new ProfileModel();

            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            var user = CommerceUserContext.Current;
            if (user == null)
            {
                return View(model);
            }

            model.FirstName = user.FirstName;
            model.Email = user.Email;
            model.EmailRepeat = user.Email;
            model.LastName = user.LastName;
            model.TelephoneNumber = user.Phone as string;

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult ForgotPasswordConfirmation(string userName)
        {
            ViewBag.UserName = userName;

            return View();
        }

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult ChangePassword()
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult Register(RegisterUserInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                var result = this.CreateJsonResult<RegisterApiModel>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var response = AccountManager.RegisterUser(inputModel);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                    AccountManager.Login(response.Result.UserName, inputModel.Password, false);
                }
                else
                {
                    result.SetErrors(response.ServiceProviderResult);
                }

                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("Register", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult ChangePassword(ChangePasswordInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                var result = this.CreateJsonResult<ChangePasswordApiModel>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var response = AccountManager.UpdateUserPassword(CommerceUserContext.Current.UserName, inputModel);
                result = new ChangePasswordApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    result.Initialize(CommerceUserContext.Current.UserName);
                }

                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("ChangePassword", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipAnalyticsTracking]
        public ActionResult AccountHomeProfile()
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            var model = new ProfileModel();
            var user = CommerceUserContext.Current;
            if (user != null)
            {
                model.FirstName = user.FirstName;
                model.Email = user.Email;
                model.LastName = user.LastName;
                model.TelephoneNumber = user.Phone;
            }

            var item = Context.Item.Children.SingleOrDefault(p => p.Name == "EditProfile");

            if (item != null)
            {
                //If there is a specially EditProfile then use it
                ViewBag.EditProfileLink = LinkManager.GetDynamicUrl(item);
            }
            else
            {
                //Else go global Edit Profile
                item = Context.Item.Database.GetItem("/sitecore/content/Home/MyAccount/Profile");
                ViewBag.EditProfileLink = LinkManager.GetDynamicUrl(item);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult AddressList()
        {
            try
            {
                var result = new AddressListItemApiModel();
                var addresses = AllAddresses(result);
                var countries = GetAvailableCountries(result);
                result.Initialize(addresses, countries);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("AddressList", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult AddressDelete(DeleteAddressInputModelItem model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var addresses = new List<IParty>();
                var response = AccountManager.RemovePartiesFromUser(Context.User.Name, model.ExternalId);
                var result = new AddressListItemApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    addresses = AllAddresses(result);
                }

                result.Initialize(addresses, null);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("AddressDelete", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult AddressModify(PartyInputModelItem model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var addresses = new List<IParty>();
                var result = new AddressListItemApiModel();
                if (CommerceUserContext.Current != null)
                {
                    var user = CommerceUserContext.Current;
                    if (string.IsNullOrEmpty(model.ExternalId))
                    {
                        // Verify we have not reached the maximum number of addresses supported.
                        var numberOfAddresses = AllAddresses(result).Count;
                        if (numberOfAddresses >= MaxNumberOfAddresses)
                        {
                            var message = DictionaryPhraseRepository.Current.Get("/Accounts/Max Address Limit Reached", "The maximum number of addresses ({0}) has been reached.");
                            result.Errors.Add(string.Format(message, numberOfAddresses));
                            result.Success = false;
                        }
                        else
                        {
                            model.ExternalId = Guid.NewGuid().ToString("B");

                            var response = AccountManager.AddParties(user.UserName, new List<IParty> { model });
                            result.SetErrors(response.ServiceProviderResult);
                            if (response.ServiceProviderResult.Success)
                            {
                                addresses = AllAddresses(result);
                            }

                            result.Initialize(addresses, null);
                        }
                    }
                    else
                    {
                        var response = AccountManager.UpdateParties(user.UserName, new List<IParty> { model });
                        result.SetErrors(response.ServiceProviderResult);
                        if (response.ServiceProviderResult.Success)
                        {
                            addresses = AllAddresses(result);
                        }

                        result.Initialize(addresses, null);
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("AddressModify", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult UpdateProfile(ProfileModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));
                var result = this.CreateJsonResult<ProfileApiModel>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (CommerceUserContext.Current == null)
                {
                    return Json(result);
                }

                var response = AccountManager.UpdateUser(CommerceUserContext.Current.UserName, model);
                result.SetErrors(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && !string.IsNullOrWhiteSpace(model.Password) && !string.IsNullOrWhiteSpace(model.PasswordRepeat))
                {
                    var changePasswordModel = new ChangePasswordInputModel {NewPassword = model.Password, ConfirmPassword = model.PasswordRepeat};
                    var passwordChangeResponse = AccountManager.UpdateUserPassword(CommerceUserContext.Current.UserName, changePasswordModel);
                    result.SetErrors(passwordChangeResponse.ServiceProviderResult);
                    if (passwordChangeResponse.ServiceProviderResult.Success)
                    {
                        result.Initialize(response.ServiceProviderResult);
                    }
                }

                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("UpdateProfile", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult GetCurrentUser()
        {
            try
            {
                if (CommerceUserContext.Current == null)
                {
                    var anonymousResult = new UserApiModel();
                    return Json(anonymousResult, JsonRequestBehavior.AllowGet);
                }

                var result = new UserApiModel();
                result.Initialize(CommerceUserContext.Current);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetCurrentUser", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult ForgotPassword(ForgotPasswordInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));
                var result = this.CreateJsonResult<ForgotPasswordApiModel>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var resetResponse = AccountManager.ResetUserPassword(model);
                if (!resetResponse.ServiceProviderResult.Success)
                {
                    return Json(new ForgotPasswordApiModel(resetResponse.ServiceProviderResult));
                }

                result.Initialize(model.Email);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("ForgotPassword", e), JsonRequestBehavior.AllowGet);
            }
        }

        public string UpdateUserName(string userName)
        {

            var defaultDomain = AccountManager.GetCommerceUsersDomain();
            return !userName.StartsWith(defaultDomain, StringComparison.OrdinalIgnoreCase) ? string.Concat(defaultDomain, @"\", userName) : userName;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }

        private Dictionary<string, string> GetAvailableCountries(AddressListItemApiModel result)
        {
            var countries = new Dictionary<string, string>();
            var response = CountryManager.GetAvailableCountries();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                countries = response.Result;
            }

            result.SetErrors(response.ServiceProviderResult);
            return countries;
        }

        private List<IParty> AllAddresses(AddressListItemApiModel result)
        {
            var addresses = new List<IParty>();
            var response = AccountManager.GetCustomerParties(CommerceUserContext.Current.UserName);
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                addresses = response.Result.ToList();
            }

            result.SetErrors(response.ServiceProviderResult);
            return addresses;
        }
    }
}