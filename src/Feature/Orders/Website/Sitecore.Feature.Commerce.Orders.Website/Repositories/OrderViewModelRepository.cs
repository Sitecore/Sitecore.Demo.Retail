using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Engine.Connect.Entities.Carts;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.GiftCards;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Orders.Website.Models;
using Sitecore.Foundation.Commerce.Website;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Links;

namespace Sitecore.Feature.Commerce.Orders.Website.Repositories
{
    [Service]
    public class OrderViewModelRepository
    {
        public OrderViewModelRepository(CommerceUserContext commerceUserContext, OrderManager orderManager, StorefrontContext storefrontContext, CatalogManager catalogManager)
        {
            CommerceUserContext = commerceUserContext;
            OrderManager = orderManager;
            StorefrontContext = storefrontContext;
            CatalogManager = catalogManager;
        }

        public CommerceUserContext CommerceUserContext { get; }

        public OrderManager OrderManager { get; }
        public StorefrontContext StorefrontContext { get; }
        public CatalogManager CatalogManager { get; }

        public OrderViewModel Get(OrderHeader orderHeader)
        {
            var commerceOrderHeader = orderHeader as CommerceOrderHeader;
            if (commerceOrderHeader == null)
                return null;
            return new OrderViewModel
            {
                OrderId = commerceOrderHeader.OrderID,
                ExternalId = commerceOrderHeader.ExternalId,
                StatusText = OrderManager.GetOrderStatusName(commerceOrderHeader.Status),
                Status = commerceOrderHeader.Status,
                LastModified = commerceOrderHeader.LastModified,
                Created = commerceOrderHeader.Created,
                Url = $"/accountmanagement/myorder?id={commerceOrderHeader.ExternalId}",
                
            };
        }

        public OrderViewModel Get(string externalId)
        {
            if (externalId == null)
            {
                throw new ArgumentNullException(nameof(externalId));
            }
            var commerceOrder = OrderManager.GetOrderDetails(CommerceUserContext.Current.UserId, externalId).Result;
            if (commerceOrder == null)
                return null;

            var lines = CreateOrderLines(commerceOrder).ToList();
            var commerceTotal = (CommerceTotal)commerceOrder.Total;
            return new OrderViewModel()
            {
                IsItemShipping = commerceOrder.Shipping != null && commerceOrder.Shipping.Count > 1 && commerceOrder.Lines.Count > 1,
                OrderId = GetOrderId(commerceOrder),
                ExternalId = commerceOrder.ExternalId,
                StatusText = OrderManager.GetOrderStatusName(commerceOrder.Status),
                Status = commerceOrder.Status,
                LastModified = commerceOrder.LastModified,
                Created = commerceOrder.Created,
                Url = $"/accountmanagement/myorder?id={commerceOrder.ExternalId}",
                Lines = lines,
                Subtotal = commerceTotal.Subtotal,
                Currency = commerceTotal.CurrencyCode,
                TotalSavings = commerceTotal.LineItemDiscountAmount + commerceTotal.OrderLevelDiscountAmount,
                ShippingTotal = commerceTotal.ShippingTotal,
                TaxTotal = commerceTotal.TaxTotal?.Amount ?? 0,
                Total = commerceTotal.Amount,
                ShippingAddresses = GetShippingAddresses(commerceOrder).ToList(),
                BillingAddresses = GetBillingAddresses(commerceOrder).ToList(),
                PaymentInfo = GetPaymentInfo(commerceOrder).ToList(),
                Adjustments = commerceOrder.Adjustments?.Select(a => a.Description) ?? Enumerable.Empty<string>(),
            };
        }

        private static string GetOrderId(CommerceOrder commerceOrder)
        {
            //BUG? CommerceOrder returns a guid in OrderId - and should return the human readable ID which is in TrackingNumber
            return commerceOrder.TrackingNumber;
        }

        private IEnumerable<PaymentInfoViewModel> GetPaymentInfo(CommerceOrder commerceOrder)
        {
            foreach (var payment in commerceOrder.Payment)
            {
                var paymentInfo = new PaymentInfoViewModel();
                if (payment is CommerceCreditCardPaymentInfo)
                {
                    var creditCard = payment as CommerceCreditCardPaymentInfo;
                    paymentInfo.CardType = creditCard.CardType;
                    paymentInfo.CreditCardNumber = creditCard.CreditCardNumber;
                    paymentInfo.CustomerName = creditCard.CustomerNameOnPayment;
                    paymentInfo.ExpirationMonth = creditCard.ExpirationMonth;
                    paymentInfo.ExpirationYear = creditCard.ExpirationMonth;
                    paymentInfo.PaymentType = PaymentType.CreditCard;
                    paymentInfo.Amount = creditCard.Amount;
                }
                else if (payment is GiftCardPaymentInfo)
                {
                    var giftCard = payment as GiftCardPaymentInfo;
                    paymentInfo.PaymentType = PaymentType.GiftCard;
                    paymentInfo.GiftCardId = giftCard.PaymentMethodID;
                    paymentInfo.Amount = giftCard.Amount;
                }
                else if (payment is FederatedPaymentInfo)
                {
                    var federated = payment as FederatedPaymentInfo;
                    paymentInfo.PaymentType = PaymentType.Federated;
                    paymentInfo.Amount = federated.Amount;
                }
                else
                {
                    continue;
                }
                yield return paymentInfo;
            }
        }

        private IEnumerable<IParty> GetBillingAddresses(CommerceOrder commerceOrder)
        {
            foreach (var payment in commerceOrder.Payment)
            {
                var partyId = payment.PartyID;
                var party = commerceOrder.Parties.FirstOrDefault(p => p.ExternalId.Equals(partyId, StringComparison.OrdinalIgnoreCase));
                if (party != null)
                    yield return party.ToEntity();
            }
        }

        private IEnumerable<IParty> GetShippingAddresses(CommerceOrder commerceOrder)
        {
            foreach (var shippingInfo in commerceOrder.Shipping)
            {
                var partyId = shippingInfo.PartyID;
                var party = commerceOrder.Parties.FirstOrDefault(p => p.ExternalId.Equals(partyId, StringComparison.OrdinalIgnoreCase));
                if (party != null)
                    yield return party.ToEntity();
            }
        }

        private IEnumerable<OrderLineViewModel> CreateOrderLines(CommerceOrder commerceOrder)
        {
            foreach (var orderLine in commerceOrder.Lines)
            {
                if (orderLine.Product == null)
                    continue;
                var product = CatalogManager.GetProduct(orderLine.Product.ProductId);
                var shippingInfo = GetOrderLineShippingInfo(commerceOrder, orderLine);
                
                yield return new OrderLineViewModel()
                {
                    OrderLineId = orderLine.ExternalCartLineId,
                    Savings = ((CommerceTotal) orderLine.Total).LineItemDiscountAmount,
                    Image = GetProductImage(orderLine),
                    ProductUrl = product != null ? LinkManager.GetDynamicUrl(product) : null,
                    Title = (orderLine.Product as CommerceCartProduct)?.DisplayName ?? orderLine.Product?.ProductName,
                    ProductColor = orderLine.Product.Properties["Color"]?.ToString(),
                    ShippingMethodName = shippingInfo?.Properties["ShippingMethodName"]?.ToString(),
                    ShippingAddress = GetShippingAddress(commerceOrder, shippingInfo),
                    ShippingEmail = shippingInfo?.ElectronicDeliveryEmail,
                    ItemPrice = orderLine.Product.Price.Amount,
                    Total = orderLine.Total.Amount,
                    Currency = orderLine.Total.CurrencyCode,
                    Quantity = orderLine.Quantity,
                    Adjustments = orderLine.Adjustments?.Select(a => a.Description) ?? Enumerable.Empty<string>(),
                    Discount = ((CommerceTotal)orderLine.Total).LineItemDiscountAmount
                };
            }
        }

        private IParty GetShippingAddress(CommerceOrder commerceOrder, CommerceShippingInfo shippingInfo)
        {
            var linePartyId = shippingInfo?.PartyID;
            if (string.IsNullOrEmpty(linePartyId))
                return null;
            return commerceOrder.Parties.FirstOrDefault(p => p.ExternalId.Equals(linePartyId, StringComparison.OrdinalIgnoreCase))?.ToEntity();
        }

        private static CommerceShippingInfo GetOrderLineShippingInfo(CommerceOrder commerceOrder, CartLine orderLine)
        {
            var uniqueShippingInfo = commerceOrder.Shipping.FirstOrDefault(shipping => shipping.LineIDs.ToList().Contains(orderLine.ExternalCartLineId) && shipping.LineIDs.Count == 1) as CommerceShippingInfo;
            if (uniqueShippingInfo != null)
                return uniqueShippingInfo;
            return commerceOrder.Shipping.FirstOrDefault(s => s.LineIDs.Contains(orderLine.ExternalCartLineId)) as CommerceShippingInfo;
        }

        private MediaItem GetProductImage(CartLine orderLine)
        {
            var images = orderLine.Properties["_product_Images"] as string;
            if (string.IsNullOrWhiteSpace(images))
            {
                return null;
            }
            var imagesList = images.Split('|');
            var imageInfoArray = imagesList[0].Split(',');

            return Sitecore.Context.Database.GetItem(ID.Parse(imageInfoArray[0]));
        }
    }
}