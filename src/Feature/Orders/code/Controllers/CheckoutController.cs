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
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Payments;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Orders.Models;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Orders.Controllers
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

        [AllowAnonymous]
        [HttpGet]
        public ActionResult StartCheckout()
        {
            var response = CartManager.GetCart(CommerceUserContext.Current.UserId, true);
            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (cart.Lines == null || !cart.Lines.Any())
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

            if (string.IsNullOrWhiteSpace(confirmationId))
            {
                var response = OrderManager.GetOrderDetails(CommerceUserContext.Current.UserId, confirmationId);
                if (response.ServiceProviderResult.Success)
                {
                    var order = response.Result;
                    viewModel.Initialize(RenderingContext.Current.Rendering, order.TrackingNumber, OrderManager.GetOrderStatusName(order.StatusCode));
                }
            }

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult GetCheckoutData()
        {
            try
            {
                var result = new CheckoutApiModel();
                var response = CartManager.GetCart(CommerceUserContext.Current.UserId, true);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    var cart = (CommerceCart) response.ServiceProviderResult.Cart;
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
        public JsonResult SubmitOrder(SubmitOrderInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

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

        [Obsolete("Please refactor")]
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

        [Obsolete("Please refactor")]
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

        [Obsolete("Please refactor")]
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

        [Obsolete("Please refactor")]
        private void GetPaymentMethods(CheckoutApiModel result)
        {
            var paymentMethodList = new List<PaymentMethod>();

            var response = PaymentManager.GetPaymentMethods(CommerceUserContext.Current.UserId, new PaymentOption {PaymentOptionType = PaymentOptionType.PayCard});
            if (response.ServiceProviderResult.Success)
            {
                paymentMethodList.AddRange(response.Result);
                paymentMethodList.ForEach(x => x.Description = PaymentManager.GetPaymentName(x.Description));
            }

            result.SetErrors(response.ServiceProviderResult);

            result.PaymentMethods = paymentMethodList;
        }

        [Obsolete("Please refactor")]
        private void GetPaymentClientToken(CheckoutApiModel result)
        {
            var response = PaymentManager.GetPaymentClientToken();
            if (response.ServiceProviderResult.Success)
            {
                result.PaymentClientToken = response.ServiceProviderResult.ClientToken;
            }

            result.SetErrors(response.ServiceProviderResult);
        }

        [Obsolete("Please refactor")]
        private void AddShippingMethodsToResult(CheckoutApiModel result)
        {
            var shippingMethodJsonResult = new ShippingMethodApiModel();

            var response = ShippingManager.GetShippingMethods(CommerceUserContext.Current.UserId, new GetShippingMethodsInputModel {ShippingPreferenceType = ShippingOptionType.None.Name});
            if (response.ServiceProviderResult.Success && response.Result.Count > 0)
            {
                shippingMethodJsonResult.Initialize(response.Result.ElementAt(0));
                result.EmailDeliveryMethod = shippingMethodJsonResult;
                return;
            }

            result.EmailDeliveryMethod = shippingMethodJsonResult;
            result.SetErrors(response.ServiceProviderResult);
        }

        [Obsolete("Please refactor")]
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