//-----------------------------------------------------------------------
// <copyright file="CartController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CartController class.</summary>
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
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Demo.Retail.Feature.Orders.Website.Models;
using Sitecore.Demo.Retail.Foundation.Commerce.Website;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Util;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Attributes;
using Sitecore.Mvc.Controllers;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Controllers
{
    public class CartController : SitecoreController
    {
        public CartController(CartManager cartManager, CommerceUserContext commerceUserContext, CartCacheHelper cartCacheHelper, PricingManager pricingManager, CurrencyManager currencyManager, StorefrontContext storefrontContext)
        {
            Assert.ArgumentNotNull(cartManager, nameof(cartManager));

            CartManager = cartManager;
            CommerceUserContext = commerceUserContext;
            CartCacheHelper = cartCacheHelper;
            PricingManager = pricingManager;
            CurrencyManager = currencyManager;
            StorefrontContext = storefrontContext;
        }

        private CartManager CartManager { get; }
        private CommerceUserContext CommerceUserContext { get; }
        private CartCacheHelper CartCacheHelper { get; }
        private PricingManager PricingManager { get; }
        private CurrencyManager CurrencyManager { get; }
        public StorefrontContext StorefrontContext { get; }

        [HttpGet]
        public override ActionResult Index()
        {
            return View();
        }

        public ActionResult MiniCart(bool updateCart = false)
        {
            return PartialView();
        }

        [HttpPost]
        [SkipAnalyticsTracking]
        public JsonResult SwitchCurrency(string currency)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(currency))
                {
                    PricingManager.CurrencyChosenPageEvent(CurrencyManager.CurrencyContext.CurrencyCode);
                    CartManager.UpdateCartCurrency(CommerceUserContext.Current.UserId, CurrencyManager.CurrencyContext.CurrencyCode);
                }
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("SwitchCurrency", e), JsonRequestBehavior.AllowGet);
            }

            return new JsonResult();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckoutButtons()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult UpdateMiniCart(bool updateCart = false)
        {
            try
            {
                var response = CartManager.GetCart(CommerceUserContext.Current.UserId, updateCart);
                var result = new MiniCartApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.ServiceProviderResult.Cart);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("BasketUpdate", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult GetCurrentCart()
        {
            try
            {
                var id = CommerceUserContext.Current.UserId;
                var cart = CartCacheHelper.GetCart(id);
                CartApiModel cartResult;

                // The following condition stops the creation of empty carts on startup.
                if (cart == null && CartCookieHelper.DoesCookieExistForCustomer(id))
                {
                    var response = CartManager.GetCart(CommerceUserContext.Current.UserId, true);
                    cartResult = new CartApiModel(response.ServiceProviderResult);
                    if (response.ServiceProviderResult.Success && response.Result != null)
                    {
                        cartResult.Initialize(response.ServiceProviderResult.Cart);
                        if (response.ServiceProviderResult.Cart != null)
                        {
                            CartCacheHelper.AddCartToCache(response.ServiceProviderResult.Cart as CommerceCart);
                        }
                    }
                }
                else
                {
                    cartResult = new CartApiModel();
                    cartResult.Initialize(cart);
                }

                return Json(cartResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetCurrentCart", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult AddCartLine(AddCartLineInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.AddLineItemsToCart(CommerceUserContext.Current.UserId, new List<AddCartLineInputModel> {inputModel});
                var result = new BaseApiModel(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("AddCartLine", e), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult AddCartLines(IEnumerable<AddCartLineInputModel> inputModels)
        {
            try
            {
                Assert.ArgumentNotNull(inputModels, nameof(inputModels));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.AddLineItemsToCart(CommerceUserContext.Current.UserId, inputModels);
                var result = new BaseApiModel(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("AddCartLines", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult DeleteLineItem(DeleteCartLineInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.RemoveLineItemFromCart(CommerceUserContext.Current.UserId, model.ExternalCartLineId);
                var result = new CartApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("DeleteLineItem", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult UpdateLineItem(UpdateCartLineInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.ChangeLineQuantity(CommerceUserContext.Current.UserId, inputModel);
                var result = new CartApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);

                    if (HasBasketErrors(response.Result))
                    {
                        // We clear the cart from the cache when basket errors are detected.  This stops the message from being displayed over and over as the
                        // cart will be retrieved again from CS and the pipelines will be executed.
                        CartCacheHelper.InvalidateCartCache(CommerceUserContext.Current.UserId);
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("UpdateLineItem", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult ApplyDiscount(DiscountInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.AddPromoCodeToCart(CommerceUserContext.Current.UserId, model.PromoCode);
                var result = new CartApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("ApplyDiscount", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult RemoveDiscount(DiscountInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.RemovePromoCodeFromCart(CommerceUserContext.Current.UserId, model.PromoCode);
                var result = new CartApiModel(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("RemoveDiscount", e), JsonRequestBehavior.AllowGet);
            }
        }

        private static bool HasBasketErrors(CartBase cart)
        {
            return cart.Properties.ContainsProperty("_Basket_Errors");
        }
    }
}