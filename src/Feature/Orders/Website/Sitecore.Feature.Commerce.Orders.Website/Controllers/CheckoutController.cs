//-----------------------------------------------------------------------
// <copyright file="CheckoutController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
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
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Newtonsoft.Json;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Payments;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Orders.Website.Models;
using Sitecore.Foundation.Commerce.Website;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Foundation.SitecoreExtensions.Attributes;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Orders.Website.Controllers
{
    public class CheckoutController : SitecoreController
    {
        private const string ConfirmationIdQueryString = "confirmationId";

        public CheckoutController(CartManager cartManager, OrderManager orderManager, AccountManager accountManager, PaymentManager paymentManager, ShippingManager shippingManager, CommerceUserContext commerceUserContext, CurrencyManager currencyManager, CountryManager countryManager, StorefrontContext storefrontContext)
        {
            CartManager = cartManager;
            OrderManager = orderManager;
            AccountManager = accountManager;
            PaymentManager = paymentManager;
            ShippingManager = shippingManager;
            CommerceUserContext = commerceUserContext;
            CurrencyManager = currencyManager;
            CountryManager = countryManager;
            StorefrontContext = storefrontContext;
        }

        private CartManager CartManager { get; }
        private PaymentManager PaymentManager { get; }
        private ShippingManager ShippingManager { get; }
        private CommerceUserContext CommerceUserContext { get; }
        private CurrencyManager CurrencyManager { get; }
        private CountryManager CountryManager { get; }
        public StorefrontContext StorefrontContext { get; }
        private OrderManager OrderManager { get; }
        private AccountManager AccountManager { get; }

        #region CheckoutRenderingMethods

        [AllowAnonymous]
        public ActionResult Checkout()
        {
            var model = CreateViewModel();
            if (!model.HasLines && !Context.PageMode.IsExperienceEditor)
            {
                //#warning Remove hardcoded URL
                var cartPageUrl = "/shoppingcart";
                return Redirect(cartPageUrl);
            }

            model = SetDefaultUserInfo(model);

            UpdateModel();

            return View(model);
        }

        [AllowAnonymous]
        public CheckoutViewModel SetDefaultUserInfo(CheckoutViewModel model)
        {
            if (CommerceUserContext.Current == null || !Context.User.IsAuthenticated)
                return model;

            if (!string.IsNullOrEmpty(CommerceUserContext.Current.Email))
                model.Cart.Email = CommerceUserContext.Current.Email;

            if (!string.IsNullOrEmpty(CommerceUserContext.Current.FirstName) &&
                !string.IsNullOrEmpty(CommerceUserContext.Current.LastName))
                model.UserName = (CommerceUserContext.Current.FirstName + " " + CommerceUserContext.Current.LastName).Trim();

            var addressResponse = AccountManager.GetCustomerParties(CommerceUserContext.Current.UserName);
            if (!addressResponse.ServiceProviderResult.Success || addressResponse.Result == null)
                return model;

            var addresses = addressResponse.Result.ToList();
            if (addresses.Count < 1)
                return model;

            var defaultAddress = addresses.FirstOrDefault(match => match.IsPrimary) ?? addresses.FirstOrDefault();
            if (defaultAddress == null)
                return model;

            defaultAddress.PartyId = defaultAddress.ExternalId;
            model.DefaultAddress = defaultAddress;

            return model;
        }


        [AllowAnonymous]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult UpdateModel()
        {
            var validationResult = this.CreateJsonResult();
            if (validationResult.HasErrors)
                return Json(validationResult, JsonRequestBehavior.AllowGet);

            var model = CreateViewModel();

            model = SetDefaultUserInfo(model);

            var json = JsonConvert.SerializeObject(model);
            return Content(json, "application/json");
        }

        private string CleanGuid(string guid)
        {
            return guid.Replace("{", "").Replace("}", "").ToLower();
        }

        private CheckoutViewModel CreateViewModel()
        {
            var model = new CheckoutViewModel();
            model.Cart = GetCart();

            InitCountriesRegions(model);
            InitShippingOptions(model);

            InitLineShippingOptions(model);
            InitLineHrefs(model);
            InitLineImgSrcs(model);
            InitPaymentClientToken(model);

            return model;
        }

        public CommerceCart GetCart()
        {
            var cartResponse = CartManager.GetCart(CommerceUserContext.Current.UserId, true);
            return cartResponse.ServiceProviderResult.Cart as CommerceCart;
        }

        private void InitCountriesRegions(CheckoutViewModel model)
        {
            // Get the proper node in Sitecore...
            Item countriesRegionsItem = Context.Database.GetItem("/sitecore/Commerce/Commerce Control Panel/Shared Settings/Countries-Regions");
            foreach (Item countryItem in countriesRegionsItem.Children)
            {
                string country = countryItem["Country Code"] + "|" + countryItem["Name"];
                model.CountriesRegions[country] = new List<string>();
                foreach (Item regionItem in countryItem.Children)
                {
                    string region = regionItem["Code"] + "|" + regionItem["Name"];
                    model.CountriesRegions[country].Add(region);
                }
            }
        }

        private void InitLineHrefs(CheckoutViewModel model)
        {
            if (!model.HasLines)
                return;

            foreach (CommerceCartLineWithImages line in model.Cart.Lines)
            {
                var productVariantItemId = line.Product.SitecoreProductItemId;
                var productVariantItem = Context.Database.GetItem(productVariantItemId);
                var url = LinkManager.GetDynamicUrl(productVariantItem.Parent).TrimEnd('/');
                model.LineHrefs[line.ExternalCartLineId] = url;
            }
        }

        private void InitLineImgSrcs(CheckoutViewModel model)
        {
            if (!model.HasLines)
                return;

            foreach (CommerceCartLineWithImages line in model.Cart.Lines)
            {
                var src = line?.DefaultImage?.ImageUrl(100, 100);
                model.LineImgSrcs[line.ExternalCartLineId] = src;
            }
        }

        private void InitLineShippingOptions(CheckoutViewModel model)
        {
            if (!model.HasLines)
                return;

            var prefsResponse = ShippingManager.GetShippingPreferences(model.Cart);
            if (!prefsResponse.ServiceProviderResult.Success || prefsResponse.Result == null)
            {
                return;
            }

            var lineShippingOptions = prefsResponse.ServiceProviderResult.LineShippingPreferences.ToList();
            foreach (CommerceCartLineWithImages line in model.Cart.Lines)
            {
                var option = lineShippingOptions?.FirstOrDefault(lso =>
                                                                     lso.LineId == line.ExternalCartLineId)?.ShippingOptions?.FirstOrDefault();
                model.LineShippingOptions[line.ExternalCartLineId] = option;
            }

            SetShippingMethods(model);
        }

        private void InitPaymentClientToken(CheckoutViewModel model)
        {
            var response = PaymentManager.GetPaymentClientToken();
            if (response.ServiceProviderResult.Success)
            {
                model.PaymentClientToken = response.ServiceProviderResult.ClientToken;
            }
        }

        private void InitShippingOptions(CheckoutViewModel model)
        {
            Item shipItemsItem = Context.Database.GetItem("/sitecore/Commerce/Commerce Control Panel/Shared Settings/Fulfillment Options/Ship items");
            foreach (Item shippingOptionItem in shipItemsItem.Children)
            {
                string shippingOption = shippingOptionItem["Title"];
                model.ShippingOptions[shippingOptionItem.ID.ToString()] = shippingOption;
            }
        }

        private void SetShippingMethods(CheckoutViewModel model)
        {
            var inputModel = new SetShippingMethodsInputModel();

            var digitalLines = model.Cart.Lines.Where(l =>
                                                          model.LineShippingOptions[l.ExternalCartLineId].Name == "Digital");
            var shipItemsLines = model.Cart.Lines.Where(l =>
                                                            model.LineShippingOptions[l.ExternalCartLineId].Name == "Ship items");

            PartyInputModelItem address = GetPartyInputModelItem();
            string email = Request.Cookies["email"]?.Value;
            if (digitalLines.Any() && shipItemsLines.Any() &&
                address != null && !string.IsNullOrWhiteSpace(email))
                inputModel.OrderShippingPreferenceType = "4";
            else if (digitalLines.Any() && !shipItemsLines.Any() &&
                     !string.IsNullOrWhiteSpace(email))
                inputModel.OrderShippingPreferenceType = "3";
            else if (!digitalLines.Any() && shipItemsLines.Any() &&
                     address != null)
                inputModel.OrderShippingPreferenceType = "1";
            else
                return;

            if (new[] { "4", "1" }.Contains(inputModel.OrderShippingPreferenceType))
            {
                if (address != null)
                    inputModel.ShippingAddresses.Add(address);

                var shipItemsMethod = new ShippingMethodInputModelItem();
                string shippingOptionID = GetShippingOptionID(model);
                shipItemsMethod.ShippingMethodID = CleanGuid(shippingOptionID);
                shipItemsMethod.ShippingMethodName = model.ShippingOptions[shippingOptionID];
                shipItemsMethod.ShippingPreferenceType = "1";
                shipItemsMethod.PartyId = "0";
                shipItemsMethod.LineIDs = shipItemsLines.Select(l => l.ExternalCartLineId).ToList();
                inputModel.ShippingMethods.Add(shipItemsMethod);
            }

            if (new[] { "4", "3" }.Contains(inputModel.OrderShippingPreferenceType))
            {
                var emailMethod = new ShippingMethodInputModelItem();
                Item emailItem = Context.Database.GetItem("/sitecore/Commerce/Commerce Control Panel/Shared Settings/Fulfillment Options/Digital/Email");
                emailMethod.ShippingMethodID = CleanGuid(emailItem.ID.ToString());
                emailMethod.ShippingMethodName = "Email";
                emailMethod.ShippingPreferenceType = "3";
                emailMethod.ElectronicDeliveryEmail = email;
                emailMethod.ElectronicDeliveryEmailContent = "";
                emailMethod.LineIDs = digitalLines.Select(l => l.ExternalCartLineId).ToList();
                inputModel.ShippingMethods.Add(emailMethod);
            }

            try
            {
                var response = CartManager.SetShippingMethods(CommerceUserContext.Current.UserId, inputModel);
                if (!response.ServiceProviderResult.Success || response.Result == null)
                    throw new Exception("Error setting shipping methods: " +
                                        string.Join(",", response.ServiceProviderResult.SystemMessages.Select(sm => sm.Message)));
                model.Cart = response.Result;
            }
            catch (Exception e)
            {
                throw;
            }

        }

        private PartyInputModelItem GetPartyInputModelItem()
        {
            var shippingAddress = Request.Cookies["shippingAddress"]?.Value;
            if (shippingAddress == null)
                return null;

            shippingAddress = HttpUtility.UrlDecode(shippingAddress);
            var item = JsonConvert.DeserializeObject<PartyInputModelItem>(shippingAddress);
            item.PartyId = "0";
            item.ExternalId = "0";
            return item;
        }

        private string GetShippingOptionID(CheckoutViewModel model)
        {
            var shippingOptionID = Request.Cookies["shippingOptionID"]?.Value;
            if (shippingOptionID == null)
                shippingOptionID = model.ShippingOptions.First(so => so.Value == "Ground").Key;
            return shippingOptionID;
        }

        #endregion

        [AllowAnonymous]
        [HttpGet]
        public ActionResult StartCheckout()
        {
            var response = CartManager.GetCart(CommerceUserContext.Current.UserId, true);
            var cart = (CommerceCart)response.ServiceProviderResult.Cart;
            if (!Context.PageMode.IsExperienceEditor && (cart.Lines == null || !cart.Lines.Any()))
            {
#warning Remove hardcoded URL
                var cartPageUrl = "/shoppingcart";
                return Redirect(cartPageUrl);
            }

            var cartViewModel = new CartViewModel(cart);
            return View(cartViewModel);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult OrderConfirmation([Bind(Prefix = ConfirmationIdQueryString)] string confirmationId)
        {
            var viewModel = new OrderConfirmationViewModel();

            if (!string.IsNullOrWhiteSpace(confirmationId))
            {
                var response = OrderManager.GetOrderDetails(CommerceUserContext.Current.UserId, confirmationId);
                if (response.ServiceProviderResult.Success)
                {
                    var order = response.Result;
                    viewModel.Initialize(RenderingContext.Current.Rendering, order.TrackingNumber, OrderManager.GetOrderStatusName(order.Status));
                }
            }

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult GetCheckoutData()
        {
            try
            {
                var result = new CheckoutApiModel();
                var response = CartManager.GetCart(CommerceUserContext.Current.UserId, true);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    var cart = (CommerceCart)response.ServiceProviderResult.Cart;
                    if (cart.Lines != null && cart.Lines.Any())
                    {
                        result.Cart = new CartApiModel(response.ServiceProviderResult);
                        result.Cart.Initialize(response.ServiceProviderResult.Cart);

                        result.ShippingMethods = new List<ShippingMethod>();

                        result.CurrencyCode = CurrencyManager.CurrencyContext.CurrencyCode;

                        AddShippingOptionsToResult(result, cart);
                        if (result.Success)
                        {
                            AddShippingMethodsToResult(result);
                            if (result.Success)
                            {
                                GetAvailableCountries(result);
                                if (result.Success)
                                {
                                    GetPaymentOptions(result);
                                    if (result.Success)
                                    {
                                        GetPaymentMethods(result);
                                        if (result.Success)
                                        {
                                            GetPaymentClientToken(result);
                                            if (result.Success)
                                            {
                                                GetUserInfo(result);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.SetErrors(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetCheckoutData", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult SubmitOrder(SubmitOrderInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                if (String.IsNullOrEmpty(inputModel.UserEmail))
                {
                    inputModel.UserEmail = Request.Cookies["email"]?.Value;
                }

                var validationResult = this.CreateJsonResult();
                var response = OrderManager.SubmitVisitorOrder(CommerceUserContext.Current.UserId, inputModel);
                var result = new SubmitOrderApiModel(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.Result == null || response.ServiceProviderResult.CartWithErrors != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize($"checkout/OrderConfirmation?{ConfirmationIdQueryString}={response.Result.OrderID}");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("SubmitOrder", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult GetShippingMethods(GetShippingMethodsInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = ShippingManager.GetShippingMethods(CommerceUserContext.Current.UserId, inputModel);
                var result = new ShippingMethodsApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    result.Initialize(response.ServiceProviderResult.ShippingMethods, response.ServiceProviderResult.ShippingMethodsPerItem);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetShippingMethods", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult SetShippingMethods(SetShippingMethodsInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.SetShippingMethods(CommerceUserContext.Current.UserId, inputModel);
                var result = new CartApiModel(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.Result == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(response.Result);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("SetShippingMethods", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult SetPaymentMethods(PaymentInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.SetPaymentMethods(CommerceUserContext.Current.UserId, inputModel);
                var result = new CartApiModel(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.Result == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(response.Result);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("SetPaymentMethods", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult GetAvailableRegions(GetAvailableRegionsInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CountryManager.GetAvailableRegions(model.CountryCode);
                var result = new AvailableRegionsApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetAvailableRegions", e), JsonRequestBehavior.AllowGet);
            }
        }

#warning Please refactor
        private void AddShippingOptionsToResult(CheckoutApiModel result, CommerceCart cart)
        {
            var response = ShippingManager.GetShippingPreferences(cart);
            var orderShippingOptions = new List<ShippingOption>();
            var lineShippingOptions = new List<LineShippingOption>();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                orderShippingOptions = response.ServiceProviderResult.ShippingOptions.ToList();
                lineShippingOptions = response.ServiceProviderResult.LineShippingPreferences.ToList();
            }

            result.InitializeShippingOptions(orderShippingOptions);
            result.InitializeLineItemShippingOptions(lineShippingOptions);

            result.SetErrors(response.ServiceProviderResult);
        }

#warning Please refactor
        private void GetAvailableCountries(CheckoutApiModel result)
        {
            var response = CountryManager.GetAvailableCountries();
            var countries = new Dictionary<string, string>();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                countries = response.Result;
            }

            result.Countries = countries;
            result.SetErrors(response.ServiceProviderResult);
        }

#warning Please refactor
        private void GetPaymentOptions(CheckoutApiModel result)
        {
            var response = PaymentManager.GetPaymentOptions(CommerceUserContext.Current.UserId);
            var paymentOptions = new List<PaymentOption>();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                paymentOptions = response.Result.ToList();
                paymentOptions.ForEach(x => x.Name = PaymentManager.GetPaymentName(x.Name));
            }

            result.PaymentOptions = paymentOptions;
            result.SetErrors(response.ServiceProviderResult);
        }

#warning Please refactor
        private void GetPaymentMethods(CheckoutApiModel result)
        {
            var paymentMethodList = new List<PaymentMethod>();

            var response = PaymentManager.GetPaymentMethods(CommerceUserContext.Current.UserId, new PaymentOption { PaymentOptionType = PaymentOptionType.PayCard });
            if (response.ServiceProviderResult.Success)
            {
                paymentMethodList.AddRange(response.Result);
                paymentMethodList.ForEach(x => x.Description = PaymentManager.GetPaymentName(x.Description));
            }

            result.SetErrors(response.ServiceProviderResult);

            result.PaymentMethods = paymentMethodList;
        }

#warning Please refactor
        private void GetPaymentClientToken(CheckoutApiModel result)
        {
            var response = PaymentManager.GetPaymentClientToken();
            if (response.ServiceProviderResult.Success)
            {
                result.PaymentClientToken = response.ServiceProviderResult.ClientToken;
            }

            result.SetErrors(response.ServiceProviderResult);
        }

#warning Please refactor
        private void AddShippingMethodsToResult(CheckoutApiModel result)
        {
            var shippingMethodJsonResult = new ShippingMethodApiModel();

            var response = ShippingManager.GetShippingMethods(CommerceUserContext.Current.UserId, new GetShippingMethodsInputModel { ShippingPreferenceType = ShippingOptionType.None.Name });
            if (response.ServiceProviderResult.Success && response.Result.Count > 0)
            {
                shippingMethodJsonResult.Initialize(response.Result.ElementAt(0));
                result.EmailDeliveryMethod = shippingMethodJsonResult;
                return;
            }

            var shippingToStoreJsonResult = new ShippingMethodApiModel();

            //   result.EmailDeliveryMethod = shippingMethodJsonResult;
            //  result.ShipToStoreDeliveryMethod = shippingToStoreJsonResult;
            result.SetErrors(response.ServiceProviderResult);
        }

#warning Please refactor
        private void GetUserInfo(CheckoutApiModel result)
        {
            if (CommerceUserContext.Current == null)
                return;

            result.IsUserAuthenticated = Context.User.IsAuthenticated;
            result.UserEmail = string.Empty;
            if (!Context.User.IsAuthenticated)
            {
                return;
            }

            result.UserEmail = CommerceUserContext.Current.Email;
            var addresses = new List<IParty>();
            var response = AccountManager.GetCustomerParties(CommerceUserContext.Current.UserName);
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                addresses = response.Result.ToList();
            }

            var addressesResult = new AddressListApiModel();
            addressesResult.Initialize(addresses);
            result.UserAddresses = addressesResult;
            result.SetErrors(response.ServiceProviderResult);
        }
    }
}