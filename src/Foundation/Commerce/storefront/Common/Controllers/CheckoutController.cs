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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Payments;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Reference.Storefront.Models;
using Sitecore.Reference.Storefront.Models.JsonResults;
using Sitecore.Reference.Storefront.Models.RenderingModels;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class CheckoutController : CSBaseController
    {
        public CheckoutController(
            [NotNull] CartManager cartManager,
            [NotNull] OrderManager orderManager,
            [NotNull] AccountManager accountManager,
            [NotNull] PaymentManager paymentManager,
            [NotNull] ShippingManager shippingManager,
            [NotNull] ContactFactory contactFactory)
            : base(accountManager, contactFactory)
        {
            Assert.ArgumentNotNull(cartManager, nameof(cartManager));
            Assert.ArgumentNotNull(orderManager, nameof(orderManager));
            Assert.ArgumentNotNull(paymentManager, nameof(paymentManager));
            Assert.ArgumentNotNull(shippingManager, nameof(shippingManager));

            CartManager = cartManager;
            OrderManager = orderManager;
            PaymentManager = paymentManager;
            ShippingManager = shippingManager;
        }

        public CartManager CartManager { get; protected set; }

        public PaymentManager PaymentManager { get; protected set; }

        public ShippingManager ShippingManager { get; protected set; }

        public OrderManager OrderManager { get; protected set; }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult StartCheckout()
        {
            var response = CartManager.GetCurrentCart(CurrentStorefront, CurrentVisitorContext, true);
            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (cart.Lines == null || !cart.Lines.Any())
            {
                var cartPageUrl = StorefrontManager.StorefrontUri("/shoppingcart");
                return Redirect(cartPageUrl);
            }

            return View(CurrentRenderingView, new CartRenderingModel(cart));
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult OrderConfirmation([Bind(Prefix = StorefrontConstants.QueryStrings.ConfirmationId)] string confirmationId)
        {
            var viewModel = new OrderConfirmationViewModel();
            CommerceOrder order = null;

            if (!string.IsNullOrWhiteSpace(confirmationId))
            {
                var response = OrderManager.GetOrderDetails(CurrentStorefront, CurrentVisitorContext, confirmationId);
                if (response.ServiceProviderResult.Success)
                {
                    order = response.Result;
                }
            }

            viewModel.Initialize(CurrentRendering, order.TrackingNumber, order);

            return View(CurrentRenderingView, viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult GetCheckoutData()
        {
            try
            {
                var result = new CheckoutDataBaseJsonResult();
                var response = CartManager.GetCurrentCart(CurrentStorefront, CurrentVisitorContext, true);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    var cart = (CommerceCart) response.ServiceProviderResult.Cart;
                    if (cart.Lines != null && cart.Lines.Any())
                    {
                        result.Cart = new CSCartBaseJsonResult(response.ServiceProviderResult);
                        result.Cart.Initialize(response.ServiceProviderResult.Cart);

                        result.ShippingMethods = new List<ShippingMethod>();
                        result.CartLoyaltyCardNumber = cart.LoyaltyCardID;

                        result.CurrencyCode = StorefrontManager.GetCustomerCurrency();

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
                CommerceLog.Current.Error("GetCheckoutData", this, e);
                return Json(new BaseJsonResult("GetCheckoutData", e), JsonRequestBehavior.AllowGet);
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

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.SubmitVisitorOrder(CurrentStorefront, CurrentVisitorContext, inputModel);
                var result = new SubmitOrderBaseJsonResult(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.Result == null || response.ServiceProviderResult.CartWithErrors != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(string.Concat(StorefrontManager.StorefrontUri("checkout/OrderConfirmation"), "?confirmationId=", response.Result.OrderID));
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("SubmitOrder", this, e);
                return Json(new BaseJsonResult("SubmitOrder", e), JsonRequestBehavior.AllowGet);
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

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = ShippingManager.GetShippingMethods(CurrentStorefront, CurrentVisitorContext, inputModel);
                var result = new ShippingMethodsJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success)
                {
                    result.Initialize(response.ServiceProviderResult.ShippingMethods, response.ServiceProviderResult.ShippingMethodsPerItem);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("GetShippingMethods", this, e);
                return Json(new BaseJsonResult("GetShippingMethods", e), JsonRequestBehavior.AllowGet);
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

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.SetShippingMethods(CurrentStorefront, CurrentVisitorContext, inputModel);
                var result = new CSCartBaseJsonResult(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.Result == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(response.Result);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("SetShippingMethods", this, e);
                return Json(new BaseJsonResult("SetShippingMethods", e), JsonRequestBehavior.AllowGet);
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

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.SetPaymentMethods(CurrentStorefront, CurrentVisitorContext, inputModel);
                var result = new CSCartBaseJsonResult(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.Result == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(response.Result);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("SetPaymentMethods", this, e);
                return Json(new BaseJsonResult("SetPaymentMethods", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult GetNearbyStoresJson(GetNearbyStoresInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            //var response = this.StoreManager.GetNearbyStores(CurrentStorefront, CurrentVisitorContext, inputModel);
            //var result = new GetNearbyStoresJsonResult(response.ServiceProviderResult);
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(new {success = false, errors = new List<string> {"Not supported in CS Connect"}});
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult GetAvailableStates(GetAvailableStatesInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.GetAvailableRegions(CurrentStorefront, CurrentVisitorContext, model.CountryCode);
                var result = new AvailableStatesBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("GetAvailableStates", this, e);
                return Json(new BaseJsonResult("GetAvailableStates", e), JsonRequestBehavior.AllowGet);
            }
        }

        private void AddShippingOptionsToResult(CheckoutDataBaseJsonResult result, CommerceCart cart)
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

        private void GetAvailableCountries(CheckoutDataBaseJsonResult result)
        {
            var response = OrderManager.GetAvailableCountries();
            var countries = new Dictionary<string, string>();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                countries = response.Result;
            }

            result.Countries = countries;
            result.SetErrors(response.ServiceProviderResult);
        }

        private void GetPaymentOptions(CheckoutDataBaseJsonResult result)
        {
            var response = PaymentManager.GetPaymentOptions(CurrentStorefront, CurrentVisitorContext);
            var paymentOptions = new List<PaymentOption>();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                paymentOptions = response.Result.ToList();
                paymentOptions.ForEach(x => x.Name = StorefrontManager.GetPaymentName(x.Name));
            }

            result.PaymentOptions = paymentOptions;
            result.SetErrors(response.ServiceProviderResult);
        }

        private void GetPaymentMethods(CheckoutDataBaseJsonResult result)
        {
            var paymentMethodList = new List<PaymentMethod>();

            var response = PaymentManager.GetPaymentMethods(CurrentStorefront, CurrentVisitorContext, new PaymentOption {PaymentOptionType = PaymentOptionType.PayCard});
            if (response.ServiceProviderResult.Success)
            {
                paymentMethodList.AddRange(response.Result);
                paymentMethodList.ForEach(x => x.Description = StorefrontManager.GetPaymentName(x.Description));
            }

            result.SetErrors(response.ServiceProviderResult);

            result.PaymentMethods = paymentMethodList;
        }

        private void GetPaymentClientToken(CheckoutDataBaseJsonResult result)
        {
            var response = PaymentManager.GetPaymentClientToken();
            if (response.ServiceProviderResult.Success)
            {
                result.PaymentClientToken = response.ServiceProviderResult.ClientToken;
            }

            result.SetErrors(response.ServiceProviderResult);
        }

        private void AddShippingMethodsToResult(CheckoutDataBaseJsonResult result)
        {
            var shippingMethodJsonResult = CommerceTypeLoader.CreateInstance<ShippingMethodBaseJsonResult>();

            var response = ShippingManager.GetShippingMethods(CurrentStorefront, CurrentVisitorContext, new GetShippingMethodsInputModel {ShippingPreferenceType = ShippingOptionType.None.Name});
            if (response.ServiceProviderResult.Success && response.Result.Count > 0)
            {
                shippingMethodJsonResult.Initialize(response.Result.ElementAt(0));
                result.EmailDeliveryMethod = shippingMethodJsonResult;
                return;
            }

            var shippingToStoreJsonResult = CommerceTypeLoader.CreateInstance<ShippingMethodBaseJsonResult>();

            result.EmailDeliveryMethod = shippingMethodJsonResult;
            result.ShipToStoreDeliveryMethod = shippingToStoreJsonResult;
            result.SetErrors(response.ServiceProviderResult);
        }

        private void GetUserInfo(CheckoutDataBaseJsonResult result)
        {
            var isUserAuthenticated = Context.User.IsAuthenticated;
            result.IsUserAuthenticated = isUserAuthenticated;
            result.UserEmail = isUserAuthenticated && !Context.User.Profile.IsAdministrator ? AccountManager.ResolveCommerceUser().Result.Email : string.Empty;
            if (!isUserAuthenticated)
            {
                return;
            }

            var addresses = new List<CommerceParty>();
            var response = AccountManager.GetCurrentCustomerParties(CurrentStorefront, CurrentVisitorContext);
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                addresses = response.Result.ToList();
            }

            var addressesResult = new AddressListItemJsonResult();
            addressesResult.Initialize(addresses, null);
            result.UserAddresses = addressesResult;
            result.SetErrors(response.ServiceProviderResult);
        }
    }
}