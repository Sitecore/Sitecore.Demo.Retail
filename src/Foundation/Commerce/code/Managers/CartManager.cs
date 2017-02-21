//-----------------------------------------------------------------------
// <copyright file="CartManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the cart business logic for the site.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Carts;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.Dictionary.Repositories;
using WebGrease.Css.Extensions;
using AddShippingInfoRequest = Sitecore.Commerce.Engine.Connect.Services.Carts.AddShippingInfoRequest;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class CartManager : BaseManager
    {
        public CartManager([NotNull] InventoryManager inventoryManager, [NotNull] CommerceCartServiceProvider cartServiceProvider)
        {
            Assert.ArgumentNotNull(inventoryManager, nameof(inventoryManager));
            Assert.ArgumentNotNull(cartServiceProvider, nameof(cartServiceProvider));

            InventoryManager = inventoryManager;
            CartServiceProvider = cartServiceProvider;
        }

        public InventoryManager InventoryManager { get; protected set; }

        public CartServiceProvider CartServiceProvider { get; protected set; }

        public virtual ManagerResponse<CartResult, CommerceCart> GetCurrentCart([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, bool refresh = false)
        {
            return GetCurrentCart(storefront, visitorContext.GetCustomerId(), refresh);
        }

        public virtual ManagerResponse<CartResult, CommerceCart> GetCurrentCart([NotNull] CommerceStorefront storefront, [NotNull] string customerId, bool refresh = false)
        {
            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            if (refresh)
            {
                cartCache.InvalidateCartCache(customerId);
            }

            var cart = cartCache.GetCart(customerId);
            if (cart != null)
            {
                var result = new CartResult {Cart = cart};
                AddBasketErrorsToResult(result.Cart as CommerceCart, result);
                return new ManagerResponse<CartResult, CommerceCart>(result, cart);
            }

            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, customerId, refresh);
            if (cartResult.Success && cartResult.Cart != null)
            {
                cart = cartResult.Cart as CommerceCart;
                cartResult.Cart = cart;
                cartCache.AddCartToCache(cart);
            }
            else
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
            }

            AddBasketErrorsToResult(cartResult.Cart as CommerceCart, cartResult);

            return new ManagerResponse<CartResult, CommerceCart>(cartResult, cart);
        }

        public virtual ManagerResponse<CartResult, bool> UpdateCartCurrency([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] string currencyCode)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(currencyCode, nameof(currencyCode));

            var result = GetCurrentCart(storefront, visitorContext);
            if (!result.ServiceProviderResult.Success)
            {
                return new ManagerResponse<CartResult, bool>(new CartResult {Success = false}, false);
            }

            var cart = result.Result;
            var changes = new CommerceCart {CurrencyCode = currencyCode};

            var updateCartResult = UpdateCart(storefront, visitorContext, cart, changes);
            if (updateCartResult.ServiceProviderResult.Success)
            {
                var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
                var customerId = visitorContext.GetCustomerId();

                cartCache.InvalidateCartCache(customerId);
            }

            return new ManagerResponse<CartResult, bool>(updateCartResult.ServiceProviderResult, updateCartResult.ServiceProviderResult.Success);
        }

        public virtual ManagerResponse<CartResult, bool> AddLineItemsToCart([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, IEnumerable<AddCartLineInputModel> inputModelList)
        {
            Assert.ArgumentNotNull(inputModelList, nameof(inputModelList));

            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, visitorContext.UserId, false);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<CartResult, bool>(cartResult, cartResult.Success);
            }

            var lines = new List<CartLine>();
            foreach (var inputModel in inputModelList)
            {
                Assert.ArgumentNotNullOrEmpty(inputModel.ProductId, nameof(inputModel.ProductId));
                Assert.ArgumentNotNullOrEmpty(inputModel.CatalogName, nameof(inputModel.CatalogName));
                Assert.ArgumentNotNull(inputModel.Quantity, nameof(inputModel.Quantity));

                if (inputModel.Quantity == null)
                {
                    continue;
                }
                var quantity = (uint) inputModel.Quantity;

                var cartLine = new CommerceCartLine(inputModel.CatalogName, inputModel.ProductId, inputModel.VariantId == "-1" ? null : inputModel.VariantId, quantity)
                {
                    Properties =
                    {
                        ["ProductUrl"] = inputModel.ProductUrl,
                        ["ImageUrl"] = inputModel.ImageUrl
                    }
                };
                // UpdateStockInformation(storefront, visitorContext, cartLine, inputModel.CatalogName);      

                lines.Add(cartLine);
            }

            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            cartCache.InvalidateCartCache(visitorContext.GetCustomerId());

            var cart = cartResult.Cart as CommerceCart;
            var addLinesRequest = new AddCartLinesRequest(cart, lines);
            RefreshCart(addLinesRequest, true);
            var addLinesResult = CartServiceProvider.AddCartLines(addLinesRequest);
            if (addLinesResult.Success && addLinesResult.Cart != null)
            {
                cartCache.AddCartToCache(addLinesResult.Cart as CommerceCart);
            }

            AddBasketErrorsToResult(addLinesResult.Cart as CommerceCart, addLinesResult);

            addLinesResult.WriteToSitecoreLog();
            return new ManagerResponse<CartResult, bool>(addLinesResult, addLinesResult.Success);
        }

        public virtual ManagerResponse<CartResult, CommerceCart> RemoveLineItemFromCart([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] string externalCartLineId)
        {
            Assert.ArgumentNotNullOrEmpty(externalCartLineId, nameof(externalCartLineId));

            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, visitorContext.UserId, false);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<CartResult, CommerceCart>(cartResult, cartResult.Cart as CommerceCart);
            }

            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            cartCache.InvalidateCartCache(visitorContext.GetCustomerId());

            var cart = cartResult.Cart as CommerceCart;
            var lineToRemove = cart.Lines.SingleOrDefault(cl => cl.ExternalCartLineId == externalCartLineId);
            if (lineToRemove == null)
            {
                return new ManagerResponse<CartResult, CommerceCart>(new CartResult {Success = true}, cart);
            }

            var removeLinesRequest = new RemoveCartLinesRequest(cart, new[] {lineToRemove});
            RefreshCart(removeLinesRequest, true);
            var removeLinesResult = CartServiceProvider.RemoveCartLines(removeLinesRequest);
            if (removeLinesResult.Success && removeLinesResult.Cart != null)
            {
                cartCache.AddCartToCache(removeLinesResult.Cart as CommerceCart);
            }

            AddBasketErrorsToResult(removeLinesResult.Cart as CommerceCart, removeLinesResult);

            removeLinesResult.WriteToSitecoreLog();
            return new ManagerResponse<CartResult, CommerceCart>(removeLinesResult, removeLinesResult.Cart as CommerceCart);
        }

        public virtual ManagerResponse<CartResult, CommerceCart> ChangeLineQuantity([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] UpdateCartLineInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.ExternalCartLineId, nameof(inputModel.ExternalCartLineId));

            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, visitorContext.UserId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<CartResult, CommerceCart>(cartResult, cartResult.Cart as CommerceCart);
            }

            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            cartCache.InvalidateCartCache(visitorContext.GetCustomerId());

            var cart = cartResult.Cart;
            var result = new CartResult {Cart = cart, Success = true};
            var cartLineToChange = cart.Lines.SingleOrDefault(cl => cl.Product != null && cl.ExternalCartLineId == inputModel.ExternalCartLineId);
            if (inputModel.Quantity == 0 && cartLineToChange != null)
            {
                result = RemoveCartLines(cart, new[] {cartLineToChange}, true);
            }
            else if (cartLineToChange != null)
            {
                cartLineToChange.Quantity = inputModel.Quantity;
                var request = new UpdateCartLinesRequest(cart, new[] {cartLineToChange});
                RefreshCart(request, true);
                result = CartServiceProvider.UpdateCartLines(request);
            }

            if (result.Success && result.Cart != null)
            {
                cartCache.AddCartToCache(result.Cart as CommerceCart);
            }

            AddBasketErrorsToResult(result.Cart as CommerceCart, result);

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public virtual ManagerResponse<AddPromoCodeResult, CommerceCart> AddPromoCodeToCart([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string promoCode)
        {
            Assert.ArgumentNotNullOrEmpty(promoCode, nameof(promoCode));

            var result = new AddPromoCodeResult {Success = false};
            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, visitorContext.UserId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<AddPromoCodeResult, CommerceCart>(result, cartResult.Cart as CommerceCart);
            }

            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            cartCache.InvalidateCartCache(visitorContext.GetCustomerId());

            var cart = cartResult.Cart as CommerceCart;
            var request = new AddPromoCodeRequest(cart, promoCode);
            RefreshCart(request, true);
            result = ((CommerceCartServiceProvider) CartServiceProvider).AddPromoCode(request);
            if (result.Success && result.Cart != null)
            {
                cartCache.AddCartToCache(result.Cart as CommerceCart);
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<AddPromoCodeResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public virtual ManagerResponse<RemovePromoCodeResult, CommerceCart> RemovePromoCodeFromCart([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string promoCode)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(promoCode, nameof(promoCode));

            var result = new RemovePromoCodeResult {Success = false};
            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, visitorContext.UserId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<RemovePromoCodeResult, CommerceCart>(result, cartResult.Cart as CommerceCart);
            }

            var cart = cartResult.Cart as CommerceCart;

            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            cartCache.InvalidateCartCache(visitorContext.GetCustomerId());

            var request = new RemovePromoCodeRequest(cart, promoCode);
            RefreshCart(request, true); // We need the CS pipelines to run.
            result = ((CommerceCartServiceProvider) CartServiceProvider).RemovePromoCode(request);
            if (result.Success && result.Cart != null)
            {
                cartCache.AddCartToCache(result.Cart as CommerceCart);
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<RemovePromoCodeResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public virtual ManagerResponse<AddShippingInfoResult, CommerceCart> SetShippingMethods([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] SetShippingMethodsInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            var result = new AddShippingInfoResult {Success = false};
            var response = GetCurrentCart(storefront, visitorContext, true);
            if (!response.ServiceProviderResult.Success || response.Result == null)
            {
                return new ManagerResponse<AddShippingInfoResult, CommerceCart>(result, response.Result);
            }

            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (inputModel.ShippingAddresses != null && inputModel.ShippingAddresses.Any())
            {
                var cartParties = cart.Parties.ToList();
                cartParties.AddRange(inputModel.ShippingAddresses.ToParties());
                cart.Parties = cartParties.AsReadOnly();
            }

            var internalShippingList = inputModel.ShippingMethods.ToShippingInfoList();
            var orderPreferenceType = InputModelExtension.GetShippingOptionType(inputModel.OrderShippingPreferenceType);
            if (orderPreferenceType != ShippingOptionType.DeliverItemsIndividually)
            {
                foreach (var shipping in internalShippingList)
                {
                    shipping.LineIDs = (from CommerceCartLine lineItem in cart.Lines select lineItem.ExternalCartLineId).ToList().AsReadOnly();
                }
            }

            var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
            cartCache.InvalidateCartCache(visitorContext.GetCustomerId());

            result = AddShippingInfoToCart(cart, orderPreferenceType, internalShippingList);
            if (result.Success && result.Cart != null)
            {
                cartCache.AddCartToCache(result.Cart as CommerceCart);
            }

            return new ManagerResponse<AddShippingInfoResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public virtual ManagerResponse<CartResult, CommerceCart> SetPaymentMethods([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] PaymentInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            var result = new AddPaymentInfoResult {Success = false};
            var response = GetCurrentCart(storefront, visitorContext, true);
            if (!response.ServiceProviderResult.Success || response.Result == null)
            {
                return new ManagerResponse<CartResult, CommerceCart>(result, response.Result);
            }

            var payments = new List<PaymentInfo>();
            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (inputModel.CreditCardPayment != null && !string.IsNullOrEmpty(inputModel.CreditCardPayment.PaymentMethodID) && inputModel.BillingAddress != null)
            {
                var billingParty = inputModel.BillingAddress.ToParty();
                var parties = cart.Parties.ToList();
                parties.Add(billingParty);
                cart.Parties = parties.AsSafeReadOnly();

                payments.Add(inputModel.CreditCardPayment.ToCreditCardPaymentInfo());
            }

            if (inputModel.FederatedPayment != null && !string.IsNullOrEmpty(inputModel.FederatedPayment.CardToken) && inputModel.BillingAddress != null)
            {
                var billingParty = inputModel.BillingAddress.ToParty();
                var parties = cart.Parties.ToList();
                parties.Add(billingParty);
                cart.Parties = parties.AsSafeReadOnly();

                var federatedPayment = inputModel.FederatedPayment.ToCreditCardPaymentInfo();
                federatedPayment.PartyID = billingParty.PartyId;
                payments.Add(federatedPayment);
            }

            if (inputModel.GiftCardPayment != null && !string.IsNullOrEmpty(inputModel.GiftCardPayment.PaymentMethodID))
            {
                payments.Add(inputModel.GiftCardPayment.ToGiftCardPaymentInfo());
            }

            if (inputModel.LoyaltyCardPayment != null && !string.IsNullOrEmpty(inputModel.LoyaltyCardPayment.PaymentMethodID))
            {
                payments.Add(inputModel.LoyaltyCardPayment.ToLoyaltyCardPaymentInfo());
            }

            var request = new AddPaymentInfoRequest(cart, payments);
            result = CartServiceProvider.AddPaymentInfo(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public virtual ManagerResponse<CartResult, CommerceCart> MergeCarts([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string anonymousVisitorId, Cart anonymousVisitorCart)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(anonymousVisitorId, nameof(anonymousVisitorId));

            var userId = visitorContext.UserId;
            var cartResult = LoadCartByName(storefront.ShopName, CommerceConstants.CartSettings.DefaultCartName, userId, true);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<CartResult, CommerceCart>(cartResult, cartResult.Cart as CommerceCart);
            }

            var currentCart = (CommerceCart) cartResult.Cart;
            var result = new CartResult {Cart = currentCart, Success = true};

            if (userId != anonymousVisitorId)
            {
                var anonymousCartHasPromocodes = anonymousVisitorCart is CommerceCart &&
                                                 ((CommerceCart) anonymousVisitorCart).OrderForms.Any(of => of.PromoCodes.Any());

                if (anonymousVisitorCart != null && (anonymousVisitorCart.Lines.Any() || anonymousCartHasPromocodes))
                {
                    if (currentCart.ShopName == anonymousVisitorCart.ShopName || currentCart.ExternalId != anonymousVisitorCart.ExternalId)
                    {
                        var mergeCartRequest = new MergeCartRequest(anonymousVisitorCart, currentCart);
                        result = CartServiceProvider.MergeCart(mergeCartRequest);
                    }
                }
            }

            if (result.Success && result.Cart != null)
            {
                var cartCache = CommerceTypeLoader.CreateInstance<CartCacheHelper>();
                cartCache.InvalidateCartCache(anonymousVisitorId);
                cartCache.AddCartToCache(result.Cart as CommerceCart);
            }

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public virtual ManagerResponse<CartResult, CommerceCart> UpdateCart([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] CommerceCart cart, [NotNull] CommerceCart cartChanges)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(cart, nameof(cart));
            Assert.ArgumentNotNull(cartChanges, nameof(cartChanges));

            var updateCartRequest = new UpdateCartRequest(cart, cartChanges);
            var result = CartServiceProvider.UpdateCart(updateCartRequest);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        protected virtual CartResult LoadCartByName(string shopName, string cartName, string userName, bool refreshCart = false)
        {
            var request = new LoadCartByNameRequest(shopName, cartName, userName);
            RefreshCart(request, refreshCart);

            var result = CartServiceProvider.LoadCart(request);
            result.WriteToSitecoreLog();
            return result;
        }

        protected virtual CartResult RemoveCartLines(Cart cart, IEnumerable<CartLine> cartLines, bool refreshCart = false)
        {
            Assert.ArgumentNotNull(cart, nameof(cart));
            Assert.ArgumentNotNull(cartLines, nameof(cartLines));

            var request = new RemoveCartLinesRequest(cart, cartLines);
            RefreshCart(request, refreshCart);
            var result = CartServiceProvider.RemoveCartLines(request);
            result.WriteToSitecoreLog();
            return result;
        }

        protected virtual AddShippingInfoResult AddShippingInfoToCart([NotNull] CommerceCart cart, [NotNull] ShippingOptionType orderShippingPreferenceType, [NotNull] IEnumerable<ShippingInfo> shipments)
        {
            Assert.ArgumentNotNull(cart, nameof(cart));
            Assert.ArgumentNotNull(orderShippingPreferenceType, nameof(orderShippingPreferenceType));
            Assert.ArgumentNotNull(shipments, nameof(shipments));

            var request = new AddShippingInfoRequest(cart, shipments.ToList(), orderShippingPreferenceType);
            var result = CartServiceProvider.AddShippingInfo(request);
            result.WriteToSitecoreLog();
            return result;
        }

        protected virtual void UpdateStockInformation([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] CommerceCartLine cartLine, [NotNull] string catalogName)
        {
            Assert.ArgumentNotNull(cartLine, nameof(cartLine));

            var products = new List<InventoryProduct> {new CommerceInventoryProduct {ProductId = cartLine.Product.ProductId, CatalogName = catalogName}};
            var stockInfoResult = InventoryManager.GetStockInformation(storefront, products, StockDetailsLevel.Status).ServiceProviderResult;
            if (stockInfoResult.StockInformation == null || !stockInfoResult.StockInformation.Any())
            {
                return;
            }

            var stockInfo = stockInfoResult.StockInformation.FirstOrDefault();
            var orderableInfo = new OrderableInformation();
            if (stockInfo != null && stockInfo.Status != null)
            {
                if (Equals(stockInfo.Status, StockStatus.PreOrderable))
                {
                    var preOrderableResult = InventoryManager.GetPreOrderableInformation(storefront, products).ServiceProviderResult;
                    if (preOrderableResult.OrderableInformation != null && preOrderableResult.OrderableInformation.Any())
                    {
                        orderableInfo = preOrderableResult.OrderableInformation.FirstOrDefault();
                    }
                }
                else if (Equals(stockInfo.Status, StockStatus.BackOrderable))
                {
                    var backOrderableResult = InventoryManager.GetBackOrderableInformation(storefront, products).ServiceProviderResult;
                    if (backOrderableResult.OrderableInformation != null && backOrderableResult.OrderableInformation.Any())
                    {
                        orderableInfo = backOrderableResult.OrderableInformation.FirstOrDefault();
                    }
                }
            }

            if (stockInfo != null)
            {
                cartLine.Product.StockStatus = stockInfo.Status;
            }

            if (orderableInfo == null)
            {
                return;
            }

            cartLine.Product.InStockDate = orderableInfo.InStockDate;
            cartLine.Product.ShippingDate = orderableInfo.ShippingDate;
        }

        protected virtual List<EmailParty> GetEmailAddressPartiesFromShippingMethods(List<ShippingMethodInputModelItem> inputModelList)
        {
            List<EmailParty> emailPartyList = null;

            if (inputModelList != null && inputModelList.Any())
            {
                var i = 1;
                foreach (var inputModel in inputModelList)
                {
                    if (ShippingOptionType.ElectronicDelivery == System.Convert.ToInt32(inputModel.ShippingPreferenceType, CultureInfo.InvariantCulture))
                    {
                        if (emailPartyList == null)
                        {
                            emailPartyList = new List<EmailParty>();
                        }

                        var party = new EmailParty();

                        party.ExternalId = Guid.NewGuid().ToString();
                        party.Name = $"Shipping_Email_{i}";
                        party.Email = inputModel.ElectronicDeliveryEmail;
                        party.Text = inputModel.ElectronicDeliveryEmailContent;

                        emailPartyList.Add(party);

                        // Set the party id to the newly created email party in order to create the association in CS.
                        inputModel.PartyID = party.ExternalId;

                        i++;
                    }
                }
            }

            return emailPartyList;
        }

        protected virtual List<Party> GetPartiesForPrefix(CommerceCart cart, string prefix)
        {
            var partyList = new List<Party>();

            foreach (var party in cart.Parties)
            {
                if (party is CommerceParty)
                {
                    if (((CommerceParty) party).Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        partyList.Add(party);
                    }
                }
                else if (party is EmailParty)
                {
                    if (((EmailParty) party).Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        partyList.Add(party);
                    }
                }
            }

            return partyList;
        }

        protected virtual void AddBasketErrorsToResult(CommerceCart cart, ServiceProviderResult result)
        {
            if (cart?.Properties[KnownBasketWeaklyTypeProperties.BasketErrors] != null)
            {
                var basketErrors = cart.Properties[KnownBasketWeaklyTypeProperties.BasketErrors] as List<string>;
                if (basketErrors == null)
                {
                    return;
                }
                foreach (var m in basketErrors)
                {
                    result.SystemMessages.Add(new SystemMessage {Message = m});
                }
            }
        }

        private static void RefreshCart(CartRequest request, bool refresh)
        {
            var info = CartRequestInformation.Get(request);

            if (info == null)
            {
                info = new CartRequestInformation(request, refresh);
            }
            else
            {
                info.Refresh = refresh;
            }
        }
    }
}