//---------------------------------------------------------------------
// <copyright file="InputModelExtension.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Extensions for dealing with the translation of action requests .</summary>
//---------------------------------------------------------------------
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
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Engine.Connect.Entities.Carts;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.GiftCards;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.Commerce.Website.Models.InputModels;

namespace Sitecore.Foundation.Commerce.Website.Extensions
{
    public static class InputModelExtension
    {
        public static List<CommerceCartLine> ToCommerceCartLines(this IEnumerable<CartLineInputModelItem> items)
        {
            return items.Select(item => new CommerceCartLine {ExternalCartLineId = item.ExternalCartLineId}).ToList();
        }

        public static CommerceCreditCardPaymentInfo ToCreditCardPaymentInfo(this CreditCardPaymentInputModelItem item)
        {
            var paymentInfo = new CommerceCreditCardPaymentInfo
            {
                Amount = item.Amount,
                CreditCardNumber = item.CreditCardNumber,
                CustomerNameOnPayment = item.CustomerNameOnPayment,
                ExpirationMonth = item.ExpirationMonth,
                ExpirationYear = item.ExpirationYear,
                PartyID = item.PartyId,
                PaymentMethodID = item.PaymentMethodID,
                ValidationCode = item.ValidationCode
            };

            return paymentInfo;
        }

        public static IParty ToEntity(this Party party)
        {
            return new PartyEntity
            {
                Name = (party as CommerceParty)?.Name ?? string.Join(" ", party.FirstName, party.LastName),
                IsPrimary = party.IsPrimary,
                ExternalId = party.ExternalId,
                Address1 = party.Address1,
                Address2 = party.Address2,
                City = party.City,
                Region = party.State,
                ZipPostalCode = party.ZipPostalCode,
                Country = (party as CommerceParty)?.CountryCode ?? (party as CommerceParty)?.Country,
                PartyId = party.PartyId,
            };
        }

        internal static Party ToCommerceParty(this IParty party)
        {
            return new CommerceParty
            {
                Name = party.Name,
                IsPrimary = party.IsPrimary,
                ExternalId = party.ExternalId,
                Address1 = party.Address1,
                Address2 = party.Address2,
                City = party.City,
                RegionCode = party.Region,
                ZipPostalCode = party.ZipPostalCode,
                CountryCode = party.Country,
                PartyId = party.PartyId,
                State = party.Region
            };
        }

        public static FederatedPaymentInfo ToFederatedPaymentInfo(this FederatedPaymentInputModelItem item)
        {
            var paymentInfo = new FederatedPaymentInfo
            {
                Amount = item.Amount,
                CardToken = item.CardToken,
                PaymentMethodID = GetPaymentOptionId("Federated")
            };

            return paymentInfo;
        }

        public static GiftCardPaymentInfo ToGiftCardPaymentInfo(this GiftCardPaymentInputModelItem item)
        {
            var paymentInfo = new GiftCardPaymentInfo
            {
                Amount = item.Amount,
                ExternalId = item.PaymentMethodID,
                PaymentMethodID = GetPaymentOptionId("Gift Card")
            };

            return paymentInfo;
        }

        public static ShippingOptionType GetShippingOptionType(string optionType)
        {
            if (optionType.Equals(ShippingOptionType.ShipToAddress.Value.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                return ShippingOptionType.ShipToAddress;
            }

            if (optionType.Equals(ShippingOptionType.PickupFromStore.Value.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                return ShippingOptionType.PickupFromStore;
            }

            if (optionType.Equals(ShippingOptionType.ElectronicDelivery.Value.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                return ShippingOptionType.ElectronicDelivery;
            }

            if (optionType.Equals(ShippingOptionType.DeliverItemsIndividually.Value.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                return ShippingOptionType.DeliverItemsIndividually;
            }

            return ShippingOptionType.None;
        }

        private static string GetPaymentOptionId(string paymentType)
        {
            var paymentOptions = Context.Database.GetItem("/sitecore/Commerce/Commerce Control Panel/Shared Settings/Payment Options");
            if (paymentOptions != null && paymentOptions.Children.Any())
            {
                var paymentOption = paymentOptions.Children.FirstOrDefault(o => o.Name.Equals(paymentType, StringComparison.OrdinalIgnoreCase));
                if (paymentOption != null)
                {
                    return paymentOption.ID.ToGuid().ToString("D");
                }
            }

            return string.Empty;
        }

        public static Party ToNewShippingParty(this PartyInputModelItem item)
        {
            var party = new CommerceParty
            {
                Address1 = item.Address1,
                City = item.City,
                Country = item.Country,
                ExternalId = string.IsNullOrWhiteSpace(item.PartyId) || item.PartyId == "0" ? Guid.NewGuid().ToString() : item.PartyId,
                Name = $"Shipping_{item.Name}",
                PartyId = item.PartyId,
                State = item.Region,
                ZipPostalCode = item.ZipPostalCode
            };

            return party;
        }

        public static CommerceParty ToNewBillingParty(this PartyInputModelItem item)
        {
            var party = new CommerceParty
            {
                Address1 = item.Address1,
                City = item.City,
                Country = item.Country,
                ExternalId = Guid.NewGuid().ToString(),
                Name = $"Billing_{item.Name}",
                PartyId = item.PartyId,
                State = item.Region,
                ZipPostalCode = item.ZipPostalCode
            };

            return party;
        }

        public static CommerceParty ToParty(this PartyInputModelItem item)
        {
            var party = new CommerceParty
            {
                Address1 = item.Address1,
                City = item.City,
                Country = item.Country,
                ExternalId = item.ExternalId,
                Name = item.Name,
                PartyId = item.PartyId,
                State = item.Region,
                ZipPostalCode = item.ZipPostalCode
            };

            return party;
        }

        public static CommerceShippingInfo ToShippingInfo(this ShippingMethodInputModelItem item)
        {
            var shippingInfo = new CommerceShippingInfo
            {
                PartyID = item.PartyId,
                ShippingMethodID = item.ShippingMethodID,
                ShippingMethodName = item.ShippingMethodName,
                ShippingOptionType = GetShippingOptionType(item.ShippingPreferenceType),
                ElectronicDeliveryEmail = item.ElectronicDeliveryEmail,
                ElectronicDeliveryEmailContent = item.ElectronicDeliveryEmailContent,
                LineIDs = item.LineIDs?.AsReadOnly() ?? new List<string>().AsReadOnly()
            };

            return shippingInfo;
        }
    }
}