﻿//-----------------------------------------------------------------------
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
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Reference.Storefront.Infrastructure;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class CartController : CSBaseController
    {
        public CartController([NotNull] CartManager cartManager, [NotNull] AccountManager accountManager, [NotNull] ContactFactory contactFactory) : base(accountManager, contactFactory)
        {
            Assert.ArgumentNotNull(cartManager, "cartManager");

            CartManager = cartManager;
        }

        public CartManager CartManager { get; protected set; }

        [HttpGet]
        public override ActionResult Index()
        {
            return View(GetRenderingView("ShoppingCart"));
        }

        public ActionResult MiniCart(bool updateCart = false)
        {
            return PartialView(GetRenderingView("MiniCart"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckoutButtons()
        {
            return View(CurrentRenderingView);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult UpdateMiniCart(bool updateCart = false)
        {
            try
            {
                var response = CartManager.GetCurrentCart(CurrentStorefront, CurrentVisitorContext, updateCart);
                var result = new MiniCartBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.ServiceProviderResult.Cart);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("BasketUpdate", this, e);
                return Json(new BaseJsonResult("BasketUpdate", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [StorefrontSessionState(SessionStateBehavior.ReadOnly)]
        public JsonResult GetCurrentCart()
        {
            try
            {
                var id = CurrentVisitorContext.GetCustomerId();
                var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
                var cart = cartCache.GetCart(id);
                CSCartBaseJsonResult cartResult;

                // The following condition stops the creation of empty carts on startup.
                if (cart == null && CartCookieHelper.DoesCookieExistForCustomer(id))
                {
                    var response = CartManager.GetCurrentCart(CurrentStorefront, CurrentVisitorContext, true);
                    cartResult = new CSCartBaseJsonResult(response.ServiceProviderResult);
                    if (response.ServiceProviderResult.Success && response.Result != null)
                    {
                        cartResult.Initialize(response.ServiceProviderResult.Cart);
                        if (response.ServiceProviderResult.Cart != null)
                        {
                            cartCache.AddCartToCache(response.ServiceProviderResult.Cart as CommerceCart);
                        }
                    }
                }
                else
                {
                    cartResult = new CSCartBaseJsonResult();
                    cartResult.Initialize(cart);
                }

                return Json(cartResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("GetCurrentCart", this, e);
                return Json(new BaseJsonResult("GetCurrentCart", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult AddCartLine(AddCartLineInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, "inputModel");

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.AddLineItemsToCart(CurrentStorefront, CurrentVisitorContext, new List<AddCartLineInputModel> {inputModel});
                var result = new BaseJsonResult(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("AddCartLine", this);
                return Json(new BaseJsonResult("AddCartLine", e), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult AddCartLines(IEnumerable<AddCartLineInputModel> inputModels)
        {
            try
            {
                Assert.ArgumentNotNull(inputModels, "inputModels");

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.AddLineItemsToCart(CurrentStorefront, CurrentVisitorContext, inputModels);
                var result = new BaseJsonResult(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("AddCartLines", this);
                return Json(new BaseJsonResult("AddCartLines", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult DeleteLineItem(DeleteCartLineInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, "model");

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.RemoveLineItemFromCart(CurrentStorefront, CurrentVisitorContext, model.ExternalCartLineId);
                var result = new CSCartBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("DeleteLineItem", this, e);
                return Json(new BaseJsonResult("DeleteLineItem", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult UpdateLineItem(UpdateCartLineInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, "inputModel");

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.ChangeLineQuantity(CurrentStorefront, CurrentVisitorContext, inputModel);
                var result = new CSCartBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);

                    if (HasBasketErrors(response.Result))
                    {
                        // We clear the cart from the cache when basket errors are detected.  This stops the message from being displayed over and over as the
                        // cart will be retrieved again from CS and the pipelines will be executed.
                        var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
                        cartCache.InvalidateCartCache(CurrentVisitorContext.GetCustomerId());
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("UpdateLineItem", this, e);
                return Json(new BaseJsonResult("UpdateLineItem", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult ApplyDiscount(DiscountInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, "model");

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.AddPromoCodeToCart(CurrentStorefront, CurrentVisitorContext, model.PromoCode);
                var result = new CSCartBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("ApplyDiscount", this, e);
                return Json(new BaseJsonResult("ApplyDiscount", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult RemoveDiscount(DiscountInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, "model");

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = CartManager.RemovePromoCodeFromCart(CurrentStorefront, CurrentVisitorContext, model.PromoCode);
                var result = new CSCartBaseJsonResult(response.ServiceProviderResult);
                if (response.ServiceProviderResult.Success && response.Result != null)
                {
                    result.Initialize(response.Result);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("RemoveDiscount", this, e);
                return Json(new BaseJsonResult("RemoveDiscount", e), JsonRequestBehavior.AllowGet);
            }
        }

        private static bool HasBasketErrors(CartBase cart)
        {
            return cart.Properties.ContainsProperty("_Basket_Errors");
        }
    }
}