//-----------------------------------------------------------------------
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
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Foundation.Commerce.Website.Util;
using Sitecore.Foundation.Dictionary.Repositories;
using WebGrease.Css.Extensions;
using AddShippingInfoRequest = Sitecore.Commerce.Engine.Connect.Services.Carts.AddShippingInfoRequest;

namespace Sitecore.Foundation.Commerce.Website.Managers
{
    public class CartManager : IManager
    {
        public CartManager(InventoryManager inventoryManager, CommerceCartServiceProvider cartServiceProvider, CartCacheHelper cartCacheHelper, StorefrontContext storefrontContext)
        {
            InventoryManager = inventoryManager;
            CartServiceProvider = cartServiceProvider;
            CartCacheHelper = cartCacheHelper;
            StorefrontContext = storefrontContext;
        }

        private CartCacheHelper CartCacheHelper { get; }
        private StorefrontContext StorefrontContext { get; }
        private InventoryManager InventoryManager { get; }
        private CartServiceProvider CartServiceProvider { get; }

#warning Please refactor - should return an entity not the connect model
        public ManagerResponse<CartResult, CommerceCart> GetCart(string userId, bool refresh = false)
        {
            if (refresh)
            {
                CartCacheHelper.InvalidateCartCache(userId);
            }

            var cart = CartCacheHelper.GetCart(userId);
            if (cart != null)
            {
                var result = new CartResult {Cart = cart};
                AddBasketErrorsToResult(result.Cart as CommerceCart, result);
                return new ManagerResponse<CartResult, CommerceCart>(result, cart);
            }

            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, userId, refresh);
            if (cartResult.Success && cartResult.Cart != null)
            {
                cart = cartResult.Cart as CommerceCart;
                cartResult.Cart = cart;
                CartCacheHelper.AddCartToCache(cart);
            }
            else
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
            }

            AddBasketErrorsToResult(cartResult.Cart as CommerceCart, cartResult);

            return new ManagerResponse<CartResult, CommerceCart>(cartResult, cart);
        }

        public ManagerResponse<CartResult, bool> UpdateCartCurrency(string userId, string currencyCode)
        {
            Assert.ArgumentNotNullOrEmpty(currencyCode, nameof(currencyCode));

            var result = GetCart(userId);
            if (!result.ServiceProviderResult.Success)
            {
                return new ManagerResponse<CartResult, bool>(new CartResult {Success = false}, false);
            }

            var cart = result.Result;
            var changes = new CommerceCart {CurrencyCode = currencyCode};

            var updateCartResult = UpdateCart(cart, changes);
            if (updateCartResult.ServiceProviderResult.Success)
            {
                CartCacheHelper.InvalidateCartCache(userId);
            }

            return new ManagerResponse<CartResult, bool>(updateCartResult.ServiceProviderResult, updateCartResult.ServiceProviderResult.Success);
        }

        public ManagerResponse<CartResult, bool> AddLineItemsToCart(string userId, IEnumerable<AddCartLineInputModel> inputModelList)
        {
            Assert.ArgumentNotNull(inputModelList, nameof(inputModelList));

            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, userId, false);
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
                //UpdateStockInformation(cartLine, inputModel.CatalogName);      

                lines.Add(cartLine);
            }

            CartCacheHelper.InvalidateCartCache(userId);

            var cart = cartResult.Cart as CommerceCart;
            var addLinesRequest = new AddCartLinesRequest(cart, lines);
            RefreshCart(addLinesRequest, true);
            var addLinesResult = CartServiceProvider.AddCartLines(addLinesRequest);
            if (addLinesResult.Success && addLinesResult.Cart != null)
            {
                CartCacheHelper.AddCartToCache(addLinesResult.Cart as CommerceCart);
            }

            AddBasketErrorsToResult(addLinesResult.Cart as CommerceCart, addLinesResult);

            addLinesResult.WriteToSitecoreLog();
            return new ManagerResponse<CartResult, bool>(addLinesResult, addLinesResult.Success);
        }

        public ManagerResponse<CartResult, CommerceCart> RemoveLineItemFromCart(string userId, string externalCartLineId)
        {
            Assert.ArgumentNotNullOrEmpty(externalCartLineId, nameof(externalCartLineId));

            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, userId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<CartResult, CommerceCart>(cartResult, cartResult.Cart as CommerceCart);
            }

            CartCacheHelper.InvalidateCartCache(userId);

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
                CartCacheHelper.AddCartToCache(removeLinesResult.Cart as CommerceCart);
            }

            AddBasketErrorsToResult(removeLinesResult.Cart as CommerceCart, removeLinesResult);

            removeLinesResult.WriteToSitecoreLog();
            return new ManagerResponse<CartResult, CommerceCart>(removeLinesResult, removeLinesResult.Cart as CommerceCart);
        }

        public ManagerResponse<CartResult, CommerceCart> ChangeLineQuantity(string userId, UpdateCartLineInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.ExternalCartLineId, nameof(inputModel.ExternalCartLineId));

            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, userId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<CartResult, CommerceCart>(cartResult, cartResult.Cart as CommerceCart);
            }

            CartCacheHelper.InvalidateCartCache(userId);

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
                CartCacheHelper.AddCartToCache(result.Cart as CommerceCart);
            }

            AddBasketErrorsToResult(result.Cart as CommerceCart, result);

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<AddPromoCodeResult, CommerceCart> AddPromoCodeToCart(string userId, string promoCode)
        {
            Assert.ArgumentNotNullOrEmpty(promoCode, nameof(promoCode));

            var result = new AddPromoCodeResult {Success = false};
            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, userId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<AddPromoCodeResult, CommerceCart>(result, cartResult.Cart as CommerceCart);
            }

            CartCacheHelper.InvalidateCartCache(userId);

            var cart = cartResult.Cart as CommerceCart;
            var request = new AddPromoCodeRequest(cart, promoCode);
            RefreshCart(request, true);
            result = ((CommerceCartServiceProvider) CartServiceProvider).AddPromoCode(request);
            if (result.Success && result.Cart != null)
            {
                CartCacheHelper.AddCartToCache(result.Cart as CommerceCart);
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<AddPromoCodeResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<RemovePromoCodeResult, CommerceCart> RemovePromoCodeFromCart(string userId, string promoCode)
        {
            Assert.ArgumentNotNullOrEmpty(promoCode, nameof(promoCode));

            var result = new RemovePromoCodeResult {Success = false};
            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, userId);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                var message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user");
                cartResult.SystemMessages.Add(new SystemMessage {Message = message});
                return new ManagerResponse<RemovePromoCodeResult, CommerceCart>(result, cartResult.Cart as CommerceCart);
            }

            var cart = cartResult.Cart as CommerceCart;

            CartCacheHelper.InvalidateCartCache(userId);

            var request = new RemovePromoCodeRequest(cart, promoCode);
            RefreshCart(request, true); // We need the CS pipelines to run.
            result = ((CommerceCartServiceProvider) CartServiceProvider).RemovePromoCode(request);
            if (result.Success && result.Cart != null)
            {
                CartCacheHelper.AddCartToCache(result.Cart as CommerceCart);
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<RemovePromoCodeResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<AddShippingInfoResult, CommerceCart> SetShippingMethods(string userId, SetShippingMethodsInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            var result = new AddShippingInfoResult {Success = false};
            var response = GetCart(userId, true);
            if (!response.ServiceProviderResult.Success || response.Result == null)
            {
                return new ManagerResponse<AddShippingInfoResult, CommerceCart>(result, response.Result);
            }

            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (inputModel.ShippingAddresses != null && inputModel.ShippingAddresses.Any())
            {
                var cartParties = cart.Parties.ToList();
                cartParties.AddRange(inputModel.ShippingAddresses.Select(item => item.ToParty()).ToList());
                cart.Parties = cartParties.AsReadOnly();
            }

            var internalShippingList = inputModel.ShippingMethods.Select(item => item.ToShippingInfo()).ToList();
            var orderPreferenceType = InputModelExtension.GetShippingOptionType(inputModel.OrderShippingPreferenceType);
            if (orderPreferenceType != ShippingOptionType.DeliverItemsIndividually)
            {
                foreach (var shipping in internalShippingList)
                {
                    shipping.LineIDs = (from CommerceCartLine lineItem in cart.Lines select lineItem.ExternalCartLineId).ToList().AsReadOnly();
                }
            }

            CartCacheHelper.InvalidateCartCache(userId);

            result = AddShippingInfoToCart(cart, orderPreferenceType, internalShippingList);
            if (result.Success && result.Cart != null)
            {
                CartCacheHelper.AddCartToCache(result.Cart as CommerceCart);
            }

            return new ManagerResponse<AddShippingInfoResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<CartResult, CommerceCart> SetPaymentMethods(string userId, PaymentInputModel inputModel)
        {
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            var result = new AddPaymentInfoResult {Success = false};
            var response = GetCart(userId, true);
            if (!response.ServiceProviderResult.Success || response.Result == null)
            {
                return new ManagerResponse<CartResult, CommerceCart>(result, response.Result);
            }

            var payments = new List<PaymentInfo>();
            var cart = (CommerceCart) response.ServiceProviderResult.Cart;
            if (!string.IsNullOrEmpty(inputModel.CreditCardPayment?.PaymentMethodID) && inputModel.BillingAddress != null)
            {
                var billingParty = inputModel.BillingAddress.ToParty();
                var parties = cart.Parties.ToList();
                parties.Add(billingParty);
                cart.Parties = parties.AsSafeReadOnly();

                payments.Add(inputModel.CreditCardPayment.ToCreditCardPaymentInfo());
            }

            if (!string.IsNullOrEmpty(inputModel.FederatedPayment?.CardToken) && inputModel.BillingAddress != null)
            {
                var billingParty = inputModel.BillingAddress.ToParty();
                var parties = cart.Parties.ToList();
                parties.Add(billingParty);
                cart.Parties = parties.AsSafeReadOnly();

                var federatedPayment = inputModel.FederatedPayment.ToFederatedPaymentInfo();
                federatedPayment.PartyID = billingParty.PartyId;
                payments.Add(federatedPayment);
            }

            if (!string.IsNullOrEmpty(inputModel.GiftCardPayment?.PaymentMethodID))
            {
                payments.Add(inputModel.GiftCardPayment.ToGiftCardPaymentInfo());
            }

            var request = new AddPaymentInfoRequest(cart, payments);
            result = CartServiceProvider.AddPaymentInfo(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<CartResult, CommerceCart> MergeCarts(string newUserId, string previousUserId, Cart anonymousVisitorCart)
        {
            Assert.ArgumentNotNullOrEmpty(previousUserId, nameof(previousUserId));
            Assert.ArgumentNotNullOrEmpty(newUserId, nameof(newUserId));

            var cartResult = LoadCartByName(CommerceConstants.CartSettings.DefaultCartName, newUserId, true);
            if (!cartResult.Success || cartResult.Cart == null)
            {
                cartResult.SystemMessages.Add(new SystemMessage {Message = DictionaryPhraseRepository.Current.Get("/System Messages/Cart/Cart Not Found Error", "Could not retrieve the cart for the current user")});
                return new ManagerResponse<CartResult, CommerceCart>(cartResult, cartResult.Cart as CommerceCart);
            }

            var currentCart = (CommerceCart) cartResult.Cart;
            var result = new CartResult {Cart = currentCart, Success = true};

            if (newUserId != previousUserId)
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
                CartCacheHelper.InvalidateCartCache(previousUserId);
                CartCacheHelper.AddCartToCache(result.Cart as CommerceCart);
            }

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<CartResult, CommerceCart> UpdateCart(CommerceCart cart, CommerceCart cartChanges)
        {
            Assert.ArgumentNotNull(cart, nameof(cart));
            Assert.ArgumentNotNull(cartChanges, nameof(cartChanges));

            var updateCartRequest = new UpdateCartRequest(cart, cartChanges);
            var result = CartServiceProvider.UpdateCart(updateCartRequest);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        private CartResult LoadCartByName(string cartName, string userName, bool refreshCart = false)
        {
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new LoadCartByNameRequest(StorefrontContext.Current.ShopName, cartName, userName);
            RefreshCart(request, refreshCart);

            var result = CartServiceProvider.LoadCart(request);
            result.WriteToSitecoreLog();
            return result;
        }

        private CartResult RemoveCartLines(Cart cart, IEnumerable<CartLine> cartLines, bool refreshCart = false)
        {
            Assert.ArgumentNotNull(cart, nameof(cart));
            Assert.ArgumentNotNull(cartLines, nameof(cartLines));

            var request = new RemoveCartLinesRequest(cart, cartLines);
            RefreshCart(request, refreshCart);
            var result = CartServiceProvider.RemoveCartLines(request);
            result.WriteToSitecoreLog();
            return result;
        }

        private AddShippingInfoResult AddShippingInfoToCart(CommerceCart cart, ShippingOptionType orderShippingPreferenceType, IEnumerable<ShippingInfo> shipments)
        {
            Assert.ArgumentNotNull(cart, nameof(cart));
            Assert.ArgumentNotNull(orderShippingPreferenceType, nameof(orderShippingPreferenceType));
            Assert.ArgumentNotNull(shipments, nameof(shipments));

            var request = new AddShippingInfoRequest(cart, shipments.ToList(), orderShippingPreferenceType);
            var result = CartServiceProvider.AddShippingInfo(request);
            result.WriteToSitecoreLog();
            return result;
        }

        private void UpdateStockInformation(CommerceCartLine cartLine, string catalogName)
        {
            Assert.ArgumentNotNull(cartLine, nameof(cartLine));

            var products = new List<InventoryProduct> {new CommerceInventoryProduct {ProductId = cartLine.Product.ProductId, CatalogName = catalogName}};
            var stockInfoResult = InventoryManager.GetStockInformation(products, StockDetailsLevel.Status).ServiceProviderResult;
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
                    var preOrderableResult = InventoryManager.GetPreOrderableInformation(products).ServiceProviderResult;
                    if (preOrderableResult.OrderableInformation != null && preOrderableResult.OrderableInformation.Any())
                    {
                        orderableInfo = preOrderableResult.OrderableInformation.FirstOrDefault();
                    }
                }
                else if (Equals(stockInfo.Status, StockStatus.BackOrderable))
                {
                    var backOrderableResult = InventoryManager.GetBackOrderableInformation(products).ServiceProviderResult;
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

        private List<EmailParty> GetEmailAddressPartiesFromShippingMethods(List<ShippingMethodInputModelItem> inputModelList)
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
                        inputModel.PartyId = party.ExternalId;

                        i++;
                    }
                }
            }

            return emailPartyList;
        }

        private List<Party> GetPartiesForPrefix(CommerceCart cart, string prefix)
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

        private void AddBasketErrorsToResult(CommerceCart cart, ServiceProviderResult result)
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