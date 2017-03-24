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
using Sitecore.Feature.Commerce.Customers.Models;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Links;
using Sitecore.Mvc.Controllers;

namespace Sitecore.Feature.Commerce.Customers.Controllers
{
    public class CustomersController : SitecoreController
    {
        public enum ManageMessageId
        {
            ChangePasswordSuccess,

            SetPasswordSuccess,

            RemoveLoginSuccess
        }

        public CustomersController(AccountManager accountManager, CountryManager countryManager, VisitorContextRepository visitorContextRepository, StorefrontManager storefrontManager)
        {
            AccountManager = accountManager;
            VisitorContextRepository = visitorContextRepository;
            StorefrontManager = storefrontManager;
            CountryManager = countryManager;
        }

        private CountryManager CountryManager { get; }
        private AccountManager AccountManager { get; }
        private VisitorContextRepository VisitorContextRepository { get; }
        public StorefrontManager StorefrontManager { get; }
        public int MaxNumberOfAddresses => Settings.GetIntSetting("Storefront.MaxNumberOfAddresses", 10);

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
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
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

            var commerceUser = AccountManager.GetUser(Context.User.Name).Result;
            if (commerceUser == null)
            {
                return View(model);
            }

            model.FirstName = commerceUser.FirstName;
            model.Email = commerceUser.Email;
            model.EmailRepeat = commerceUser.Email;
            model.LastName = commerceUser.LastName;
            model.TelephoneNumber = commerceUser.GetPropertyValue("Phone") as string;

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

                var anonymousVisitorId = VisitorContextRepository.GetCurrent().UserId;

                var response = AccountManager.RegisterUser(StorefrontManager.Current, inputModel);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                    AccountManager.Login(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), anonymousVisitorId, response.Result.UserName, inputModel.Password, false);
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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult Login(LoginViewModel model)
        {
            var anonymousVisitorId = VisitorContextRepository.GetCurrent().UserId;

            if (ModelState.IsValid && AccountManager.Login(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), anonymousVisitorId, UpdateUserName(model.UserName), model.Password, model.RememberMe))
            {
                return RedirectToLocal("/");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
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

                var response = AccountManager.UpdateUserPassword(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), inputModel);
                result = new ChangePasswordApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    result.Initialize(VisitorContextRepository.GetCurrent().UserName);
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
        public ActionResult AccountHomeProfile()
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            var model = new ProfileModel();

            if (Context.User.IsAuthenticated && !Context.User.Profile.IsAdministrator)
            {
                var commerceUser = AccountManager.GetUser(Context.User.Name).Result;
                if (commerceUser != null)
                {
                    model.FirstName = commerceUser.FirstName;
                    model.Email = commerceUser.Email;
                    model.LastName = commerceUser.LastName;
                    model.TelephoneNumber = commerceUser.GetPropertyValue("Phone") as string;
                }
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
                var response = AccountManager.RemovePartiesFromCurrentUser(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), model.ExternalId);
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
                var userResponse = AccountManager.GetUser(Context.User.Name);
                var result = new AddressListItemApiModel(userResponse.ServiceProviderResult);
                if (userResponse.ServiceProviderResult.Success && userResponse.Result != null)
                {
                    var commerceUser = userResponse.Result;

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

                            var response = AccountManager.AddParties(StorefrontManager.Current, commerceUser.ExternalId, new List<IParty> { model });
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
                        var response = AccountManager.UpdateParties(StorefrontManager.Current, commerceUser.ExternalId, new List<IParty> { model });
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

                if (!Context.User.IsAuthenticated || Context.User.Profile.IsAdministrator)
                {
                    return Json(result);
                }

                var response = AccountManager.UpdateUser(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), model);
                result.SetErrors(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && !string.IsNullOrWhiteSpace(model.Password) && !string.IsNullOrWhiteSpace(model.PasswordRepeat))
                {
                    var changePasswordModel = new ChangePasswordInputModel {NewPassword = model.Password, ConfirmPassword = model.PasswordRepeat};
                    var passwordChangeResponse = AccountManager.UpdateUserPassword(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), changePasswordModel);
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
        public JsonResult GetCurrentUser()
        {
            try
            {
                if (!Context.User.IsAuthenticated || Context.User.Profile.IsAdministrator)
                {
                    var anonymousResult = new UserApiModel();
                    return Json(anonymousResult, JsonRequestBehavior.AllowGet);
                }

                var response = AccountManager.GetUser(Context.User.Name);
                var result = new UserApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

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

                var resetResponse = AccountManager.ResetUserPassword(StorefrontManager.Current, model);
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
            var response = AccountManager.GetCurrentCustomerParties(StorefrontManager.Current, VisitorContextRepository.GetCurrent());
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                addresses = response.Result.ToList();
            }

            result.SetErrors(response.ServiceProviderResult);
            return addresses;
        }
    }
}