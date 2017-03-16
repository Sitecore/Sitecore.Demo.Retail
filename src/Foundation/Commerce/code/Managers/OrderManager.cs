//-----------------------------------------------------------------------
// <copyright file="OrderManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the order business logic for the site.</summary>
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

using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Carts;
using Sitecore.Commerce.Services.Orders;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.Dictionary.Repositories;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class OrderManager : BaseManager
    {
        public OrderManager(OrderServiceProvider orderServiceProvider, [NotNull] CartManager cartManager, CartCacheHelper cartCacheHelper)
        {
            Assert.ArgumentNotNull(orderServiceProvider, nameof(orderServiceProvider));
            Assert.ArgumentNotNull(cartManager, nameof(cartManager));

            OrderServiceProvider = orderServiceProvider;
            CartManager = cartManager;
            CartCacheHelper = cartCacheHelper;
        }

        public OrderServiceProvider OrderServiceProvider { get; protected set; }

        public CartManager CartManager { get; protected set; }
        public CartCacheHelper CartCacheHelper { get; }

        public ManagerResponse<SubmitVisitorOrderResult, CommerceOrder> SubmitVisitorOrder([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] SubmitOrderInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));

            var errorResult = new SubmitVisitorOrderResult {Success = false};

            var response = CartManager.GetCurrentCart(storefront, visitorContext, true);
            if (!response.ServiceProviderResult.Success || response.Result == null)
            {
                response.ServiceProviderResult.SystemMessages.ToList().ForEach(m => errorResult.SystemMessages.Add(m));
                return new ManagerResponse<SubmitVisitorOrderResult, CommerceOrder>(errorResult, null);
            }

            var cart = (CommerceCart) response.ServiceProviderResult.Cart;

            if (cart.Lines.Count == 0)
            {
                errorResult.SystemMessages.Add(new SystemMessage
                {
                    Message = DictionaryPhraseRepository.Current.Get("/System Messages/Orders/Submit Order Has Empty Cart", "Cannot submit and order with an empty cart.")
                });
                return new ManagerResponse<SubmitVisitorOrderResult, CommerceOrder>(errorResult, null);
            }

            cart.Email = inputModel.UserEmail;

            var request = new SubmitVisitorOrderRequest(cart);
            RefreshCartOnOrdersRequest(request);
            errorResult = OrderServiceProvider.SubmitVisitorOrder(request);
            if (errorResult.Success && errorResult.Order != null && errorResult.CartWithErrors == null)
            {
                CartCacheHelper.InvalidateCartCache(visitorContext.GetCustomerId());

                var mailUtil = new MailUtil();

                var wasEmailSent = mailUtil.SendMail("PurchaseConfirmation", inputModel.UserEmail,
                    storefront.SenderEmailAddress, new object(),
                    new object[]
                    {
                        $"{cart.Parties.FirstOrDefault()?.FirstName} {cart.Parties.FirstOrDefault()?.LastName}",
                        errorResult.Order.TrackingNumber,
                        errorResult.Order.OrderDate,
                        string.Join(", ", cart.Lines.Select(x => x.Product.ProductName)),
                        cart.Total.Amount.ToCurrency(cart.Total.CurrencyCode)
                    });

                if (!wasEmailSent)
                {
                    var message = DictionaryPhraseRepository.Current.Get("/System Messages/Orders/Could Not Send Email Error", "Sorry, the email could not be sent");
                    errorResult.SystemMessages.Add(new Sitecore.Commerce.Services.SystemMessage(message));
                }
            }

            errorResult.WriteToSitecoreLog();
            return new ManagerResponse<SubmitVisitorOrderResult, CommerceOrder>(errorResult,
                errorResult.Order as CommerceOrder);
        }

        public CartRequestInformation RefreshCartOnOrdersRequest(OrdersRequest request)
        {
            var info = CartRequestInformation.Get(request);

            if (info == null)
            {
                info = new CartRequestInformation(request, true);
            }
            else
            {
                info.Refresh = true;
            }
            return info;
        }

        public ManagerResponse<GetVisitorOrdersResult, IEnumerable<OrderHeader>> GetOrders(string customerId, string shopName)
        {
            Assert.ArgumentNotNullOrEmpty(customerId, nameof(customerId));
            Assert.ArgumentNotNullOrEmpty(shopName, nameof(shopName));

            var request = new GetVisitorOrdersRequest(customerId, shopName);
            var result = OrderServiceProvider.GetVisitorOrders(request);
            if (result.Success && result.OrderHeaders != null && result.OrderHeaders.Count > 0)
            {
                return new ManagerResponse<GetVisitorOrdersResult, IEnumerable<OrderHeader>>(result, result.OrderHeaders.ToList());
            }

            result.WriteToSitecoreLog();
            //no orders found returns false - we treat it as success
            if (!result.Success && !result.SystemMessages.Any())
                result.Success = true;
            return new ManagerResponse<GetVisitorOrdersResult, IEnumerable<OrderHeader>>(result, new List<OrderHeader>());
        }

        public ManagerResponse<CartResult, CommerceCart> Reorder([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, ReorderInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.OrderId, nameof(inputModel.OrderId));

            var request = new ReorderByCartNameRequest
            {
                CustomerId = visitorContext.GetCustomerId(),
                OrderId = inputModel.OrderId,
                ReorderLineExternalIds = inputModel.ReorderLineExternalIds,
                CartName = CommerceConstants.CartSettings.DefaultCartName,
                OrderShippingPreferenceType = ShippingOptionType.ShipToAddress
            };

            var result = OrderServiceProvider.Reorder(request);
            result.WriteToSitecoreLog();
            return new ManagerResponse<CartResult, CommerceCart>(result, result.Cart as CommerceCart);
        }

        public ManagerResponse<VisitorCancelOrderResult, bool> CancelOrder([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, CancelOrderInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(inputModel, nameof(inputModel));
            Assert.ArgumentNotNullOrEmpty(inputModel.OrderId, nameof(inputModel.OrderId));

            var request = new VisitorCancelOrderRequest(inputModel.OrderId, visitorContext.GetCustomerId(), storefront.ShopName);
            request.OrderLineExternalIds = inputModel.OrderLineExternalIds;
            var result = OrderServiceProvider.VisitorCancelOrder(request);

            result.WriteToSitecoreLog();

            return new ManagerResponse<VisitorCancelOrderResult, bool>(result, result.Success);
        }

        public ManagerResponse<GetVisitorOrderResult, CommerceOrder> GetOrderDetails([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] string orderId)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(orderId, nameof(orderId));

            var customerId = visitorContext.GetCustomerId();
            var request = new GetVisitorOrderRequest(orderId, customerId, storefront.ShopName);
            var result = OrderServiceProvider.GetVisitorOrder(request);
            result.WriteToSitecoreLog();
            return new ManagerResponse<GetVisitorOrderResult, CommerceOrder>(result, result.Order as CommerceOrder);
        }
    }
}