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
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;
using Sitecore.Feature.Commerce.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Data;
using Sitecore.Links;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Data.Items;
using Newtonsoft.Json;
using System.Web;

namespace Sitecore.Feature.Commerce.Orders.Controllers
{
    public class CheckoutController : SitecoreController
    {
        private const string ConfirmationIdQueryString = "confirmationId";

        public CheckoutController(CartManager cartManager, OrderManager orderManager, AccountManager accountManager, PaymentManager paymentManager, ShippingManager shippingManager, ContactFactory contactFactory, VisitorContextRepository visitorContextRepository, CurrencyManager currencyManager)
        {
            CartManager = cartManager;
            OrderManager = orderManager;
            AccountManager = accountManager;
            PaymentManager = paymentManager;
            ShippingManager = shippingManager;
            VisitorContextRepository = visitorContextRepository;
            CurrencyManager = currencyManager;
        }

        private CartManager CartManager { get; }
        private PaymentManager PaymentManager { get; }
        private ShippingManager ShippingManager { get; }
        private VisitorContextRepository VisitorContextRepository { get; }
        private CurrencyManager CurrencyManager { get; }
        private OrderManager OrderManager { get; }
        private AccountManager AccountManager { get; }

        [AllowAnonymous]
        public ActionResult Checkout()
        {
            if (Request.HttpMethod == "POST")
                return SubmitOrder();

            var model = CreateViewModel();
            if (!model.HasLines && !Context.PageMode.IsExperienceEditor)
            {
#warning Remove hardcoded URL
                var cartPageUrl = "/shoppingcart";
                return Redirect(cartPageUrl);
            }
            return View(model);
        }

        private RedirectResult SubmitOrder()
        {
            try
            {
                var inputModel = new SubmitOrderInputModel();
                inputModel.UserEmail = Request.Cookies["email"]?.Value;
                inputModel.FederatedPayment = new FederatedPaymentInputModelItem();
                inputModel.FederatedPayment.Amount = GetCart().Total.Amount;
                inputModel.FederatedPayment.CardPaymentAcceptCardPrefix = "paypal";
                inputModel.FederatedPayment.CardToken = Request.Form["payment_method_nonce"];

                var response = OrderManager.SubmitVisitorOrder(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
                if (!response.ServiceProviderResult.Success || response.Result == null || response.ServiceProviderResult.CartWithErrors != null)
                {
                    throw new Exception("Error submitting order: " +
                        string.Join(", ", response.ServiceProviderResult.SystemMessages.Select(sm => sm.Message)));
                }

                return Redirect($"OrderConfirmation?{ConfirmationIdQueryString}={response.Result.OrderID}");
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("SubmitOrder", this, e);
                throw;
            }
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
            var json = JsonConvert.SerializeObject(model);
            return Content(json, "application/json");
        }

        private string CleanGuid(string guid)
        {
            return guid.Replace("{", "").Replace("}", "").ToLower();
        }

        private CheckoutViewModel CreateViewModel() {
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

        private CommerceCart GetCart()
        {
            var cartResponse = CartManager.GetCurrentCart(StorefrontManager.CurrentStorefront,
                VisitorContextRepository.GetCurrent(), true);
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

        [AllowAnonymous]
        [HttpGet]
        public ActionResult StartCheckout()
        {
            var response = CartManager.GetCurrentCart(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), true);
            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (!Context.PageMode.IsExperienceEditor && (cart.Lines == null || !cart.Lines.Any()))
            {
#warning Remove hardcoded URL
                var cartPageUrl = "/shoppingcart";
                return Redirect(cartPageUrl);
            }

            return View(new CartViewModel(cart));
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult OrderConfirmation([Bind(Prefix = ConfirmationIdQueryString)] string confirmationId)
        {
            var viewModel = new OrderConfirmationViewModel();
            CommerceOrder order = null;

            if (!string.IsNullOrWhiteSpace(confirmationId))
            {
                var response = OrderManager.GetOrderDetails(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), confirmationId);
                if (response.ServiceProviderResult.Success)
                {
                    order = response.Result;
                }
            }

            viewModel.Initialize(RenderingContext.Current.Rendering, order.TrackingNumber, order);

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
                var response = CartManager.GetCurrentCart(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), true);
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

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.SubmitVisitorOrder(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
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

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = ShippingManager.GetShippingMethods(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
                var result = new ShippingMethodsApiModel(response.ServiceProviderResult);
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
                shipItemsMethod.PartyID = "0";
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
                var response = CartManager.SetShippingMethods(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
                if (!response.ServiceProviderResult.Success || response.Result == null)
                    throw new Exception("Error setting shipping methods: " +
                        string.Join(",", response.ServiceProviderResult.SystemMessages.Select(sm => sm.Message)));
                model.Cart = response.Result;
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("SetShippingMethods", this, e);
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

                var response = CartManager.SetShippingMethods(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
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

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.SetPaymentMethods(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
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

            //var response = this.StoreManager.GetNearbyStores(CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel);
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

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.GetAvailableRegions(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), model.CountryCode);
                var result = new AvailableStatesApiModel(response.ServiceProviderResult);
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
            var response = OrderManager.GetAvailableCountries();
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
            var response = PaymentManager.GetPaymentOptions(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent());
            var paymentOptions = new List<PaymentOption>();
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                paymentOptions = response.Result.ToList();
                paymentOptions.ForEach(x => x.Name = LookupManager.GetPaymentName(x.Name));
            }

            result.PaymentOptions = paymentOptions;
            result.SetErrors(response.ServiceProviderResult);
        }

        [Obsolete("Please refactor")]
        private void GetPaymentMethods(CheckoutApiModel result)
        {
            var paymentMethodList = new List<PaymentMethod>();

            var response = PaymentManager.GetPaymentMethods(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), new PaymentOption {PaymentOptionType = PaymentOptionType.PayCard});
            if (response.ServiceProviderResult.Success)
            {
                paymentMethodList.AddRange(response.Result);
                paymentMethodList.ForEach(x => x.Description = LookupManager.GetPaymentName(x.Description));
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

            var response = ShippingManager.GetShippingMethods(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), new GetShippingMethodsInputModel {ShippingPreferenceType = ShippingOptionType.None.Name});
            if (response.ServiceProviderResult.Success && response.Result.Count > 0)
            {
                shippingMethodJsonResult.Initialize(response.Result.ElementAt(0));
                result.EmailDeliveryMethod = shippingMethodJsonResult;
                return;
            }

            var shippingToStoreJsonResult = new ShippingMethodApiModel();

            result.EmailDeliveryMethod = shippingMethodJsonResult;
            result.ShipToStoreDeliveryMethod = shippingToStoreJsonResult;
            result.SetErrors(response.ServiceProviderResult);
        }

        [Obsolete("Please refactor")]
        private void GetUserInfo(CheckoutApiModel result)
        {
            var isUserAuthenticated = Context.User.IsAuthenticated;
            result.IsUserAuthenticated = isUserAuthenticated;
            result.UserEmail = isUserAuthenticated && !Context.User.Profile.IsAdministrator ? AccountManager.ResolveCommerceUser().Result.Email : string.Empty;
            if (!isUserAuthenticated)
            {
                return;
            }

            var addresses = new List<CommerceParty>();
            var response = AccountManager.GetCurrentCustomerParties(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent());
            if (response.ServiceProviderResult.Success && response.Result != null)
            {
                addresses = response.Result.ToList();
            }

            var addressesResult = new AddressListItemApiModel();
            addressesResult.Initialize(addresses, null);
            result.UserAddresses = addressesResult;
            result.SetErrors(response.ServiceProviderResult);
        }
    }
}