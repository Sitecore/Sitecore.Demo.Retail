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
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Configuration;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Customers;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Links;
using Sitecore.Mvc.Controllers;
using Sitecore.Reference.Storefront.Infrastructure;
using Sitecore.Reference.Storefront.Models;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class AccountController : SitecoreController
    {
        public enum ManageMessageId
        {
            ChangePasswordSuccess,

            SetPasswordSuccess,

            RemoveLoginSuccess
        }

        public AccountController([NotNull] OrderManager orderManager, [NotNull] AccountManager accountManager, VisitorContextRepository visitorContextRepository)
        {
            Assert.ArgumentNotNull(orderManager, nameof(orderManager));

            OrderManager = orderManager;
            AccountManager = accountManager;
            VisitorContextRepository = visitorContextRepository;
        }

        private OrderManager OrderManager { get; set; }
        private AccountManager AccountManager { get; }
        private VisitorContextRepository VisitorContextRepository { get; }


        [Authorize]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult Reorder(ReorderInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.Reorder(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
                var result = new BaseJsonResult(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("Reorder", this);
                return Json(new BaseJsonResult("Reorder", e), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult CancelOrder(CancelOrderInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                var validationResult = this.CreateJsonResult<CancelOrderBaseJsonResult>();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.CancelOrder(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
                var result = new CancelOrderBaseJsonResult(response.ServiceProviderResult);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("CancelOrder", this);
                return Json(new BaseJsonResult("CancelOrder", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public override ActionResult Index()
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
        public ActionResult LogOff()
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
                var result = this.CreateJsonResult<RegisterBaseJsonResult>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var anonymousVisitorId = VisitorContextRepository.GetCurrent().UserId;

                var response = AccountManager.RegisterUser(StorefrontManager.CurrentStorefront, inputModel);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                    AccountManager.Login(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), anonymousVisitorId, response.Result.UserName, inputModel.Password, false);
                }
                else
                {
                    result.SetErrors(response.ServiceProviderResult);
                }

                return Json(result);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("Register", this, e);
                return Json(new BaseJsonResult("Register", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult Login(LoginViewModel model)
        {
            var anonymousVisitorId = VisitorContextRepository.GetCurrent().UserId;

            if (ModelState.IsValid && AccountManager.Login(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), anonymousVisitorId, UpdateUserName(model.UserName), model.Password, model.RememberMe))
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
                var result = this.CreateJsonResult<ChangePasswordBaseJsonResult>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var response = AccountManager.UpdateUserPassword(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
                result = new ChangePasswordBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    result.Initialize(VisitorContextRepository.GetCurrent().UserName);
                }

                return Json(result);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("ChangePassword", this, e);
                return Json(new BaseJsonResult("ChangePassword", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult MyOrders()
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            var commerceUser = AccountManager.GetUser(Context.User.Name).Result;
            var orders = OrderManager.GetOrders(commerceUser.ExternalId, StorefrontManager.CurrentStorefront.ShopName).Result;
            return View(orders.ToList());
        }

        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult MyOrder(string id)
        {
            if (!Context.User.IsAuthenticated)
            {
                return Redirect("/login");
            }

            var response = OrderManager.GetOrderDetails(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), id);
            ViewBag.IsItemShipping = response.Result.Shipping != null && response.Result.Shipping.Count > 1 && response.Result.Lines.Count > 1;
            return View(response.Result);
        }

        [HttpPost]
        [Authorize]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult RecentOrders()
        {
            try
            {
                var recentOrders = new List<OrderHeader>();

                var userResponse = AccountManager.GetUser(Context.User.Name);
                var result = new OrdersBaseJsonResult(userResponse.ServiceProviderResult);
                if (userResponse.ServiceProviderResult.Success && userResponse.Result != null)
                {
                    var commerceUser = userResponse.Result;
                    var response = OrderManager.GetOrders(commerceUser.ExternalId, StorefrontManager.CurrentStorefront.ShopName);
                    result.SetErrors(response.ServiceProviderResult);
                    if (response.ServiceProviderResult.Success && response.Result != null)
                    {
                        var orders = response.Result.Cast<CommerceOrderHeader>().ToList();
                        recentOrders = orders.Where(order => order.LastModified > DateTime.Today.AddDays(-30)).Take(5).Cast<OrderHeader>().ToList();
                    }
                }

                result.Initialize(recentOrders);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("RecentOrders", this, e);
                return Json(new BaseJsonResult("RecentOrders", e), JsonRequestBehavior.AllowGet);
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
        [StorefrontSessionState(SessionStateBehavior.ReadOnly)]
        public JsonResult AddressList()
        {
            try
            {
                var result = new AddressListItemJsonResult();
                var addresses = AllAddresses(result);
                var countries = GetAvailableCountries(result);
                result.Initialize(addresses, countries);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("AddressList", this, e);
                return Json(new BaseJsonResult("AddressList", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult AddressDelete(DeletePartyInputModelItem model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var addresses = new List<CommerceParty>();
                var response = AccountManager.RemovePartiesFromCurrentUser(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), model.ExternalId);
                var result = new AddressListItemJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    addresses = AllAddresses(result);
                }

                result.Initialize(addresses, null);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("AddressDelete", this, e);
                return Json(new BaseJsonResult("AddressDelete", e), JsonRequestBehavior.AllowGet);
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

                var addresses = new List<CommerceParty>();
                var userResponse = AccountManager.GetUser(Context.User.Name);
                var result = new AddressListItemJsonResult(userResponse.ServiceProviderResult);
                if (userResponse.ServiceProviderResult.Success && userResponse.Result != null)
                {
                    var commerceUser = userResponse.Result;
                    var customer = new CommerceCustomer {ExternalId = commerceUser.ExternalId};
                    var party = new CommerceParty
                    {
                        ExternalId = model.ExternalId,
                        Name = model.Name,
                        Address1 = model.Address1,
                        City = model.City,
                        Country = model.Country,
                        State = model.State,
                        ZipPostalCode = model.ZipPostalCode,
                        PartyId = model.PartyId,
                        IsPrimary = model.IsPrimary
                    };

                    if (string.IsNullOrEmpty(party.ExternalId))
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
                            party.ExternalId = Guid.NewGuid().ToString("B");

                            var response = AccountManager.AddParties(StorefrontManager.CurrentStorefront, customer, new List<Party> {party});
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
                        var response = AccountManager.UpdateParties(StorefrontManager.CurrentStorefront, customer, new List<Party> {party});
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
                CommerceLog.Current.Error("AddressModify", this, e);
                return Json(new BaseJsonResult("AddressModify", e), JsonRequestBehavior.AllowGet);
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
                var result = this.CreateJsonResult<ProfileBaseJsonResult>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (!Context.User.IsAuthenticated || Context.User.Profile.IsAdministrator)
                {
                    return Json(result);
                }

                var response = AccountManager.UpdateUser(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), model);
                result.SetErrors(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && !string.IsNullOrWhiteSpace(model.Password) && !string.IsNullOrWhiteSpace(model.PasswordRepeat))
                {
                    var changePasswordModel = new ChangePasswordInputModel {NewPassword = model.Password, ConfirmPassword = model.PasswordRepeat};
                    var passwordChangeResponse = AccountManager.UpdateUserPassword(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), changePasswordModel);
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
                CommerceLog.Current.Error("UpdateProfile", this, e);
                return Json(new BaseJsonResult("UpdateProfile", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [StorefrontSessionState(SessionStateBehavior.ReadOnly)]
        public JsonResult GetCurrentUser()
        {
            try
            {
                if (!Context.User.IsAuthenticated || Context.User.Profile.IsAdministrator)
                {
                    var anonymousResult = new UserBaseJsonResult();
                    anonymousResult.Initialize(new CommerceUser());
                    return Json(anonymousResult, JsonRequestBehavior.AllowGet);
                }

                var response = AccountManager.GetUser(Context.User.Name);
                var result = new UserBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("GetCurrentUser", this, e);
                return Json(new BaseJsonResult("GetCurrentUser", e), JsonRequestBehavior.AllowGet);
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
                var result = this.CreateJsonResult<ForgotPasswordBaseJsonResult>();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var resetResponse = AccountManager.ResetUserPassword(StorefrontManager.CurrentStorefront, model);
                if (!resetResponse.ServiceProviderResult.Success)
                {
                    return Json(new ForgotPasswordBaseJsonResult(resetResponse.ServiceProviderResult));
                }

                result.Initialize(model.Email);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("ForgotPassword", this, e);
                return Json(new BaseJsonResult("ForgotPassword", e), JsonRequestBehavior.AllowGet);
            }
        }

        public string UpdateUserName(string userName)
        {
            var defaultDomain = CommerceServerSitecoreConfig.Current.DefaultCommerceUsersDomain;
            if (string.IsNullOrWhiteSpace(defaultDomain))
            {
                defaultDomain = CommerceConstants.ProfilesStrings.CommerceUsersDomainName;
            }

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

        private Dictionary<string, string> GetAvailableCountries(AddressListItemJsonResult result)
        {
            var countries = new Dictionary<string, string>();
            var response = OrderManager.GetAvailableCountries();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                countries = response.Result;
            }

            result.SetErrors(response.ServiceProviderResult);
            return countries;
        }

        private List<CommerceParty> AllAddresses(AddressListItemJsonResult result)
        {
            var addresses = new List<CommerceParty>();
            var response = AccountManager.GetCurrentCustomerParties(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent());
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                addresses = response.Result.ToList();
            }

            result.SetErrors(response.ServiceProviderResult);
            return addresses;
        }
        public int MaxNumberOfAddresses => Settings.GetIntSetting("Storefront.MaxNumberOfAddresses", 10);
    }
}