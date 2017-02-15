//-----------------------------------------------------------------------
// <copyright file="CommerceServerStorefrontConstants.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Storefront constant definition.</summary>
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

namespace Sitecore.Foundation.Commerce
{
    [Obsolete("Move to Templates.cs or similar")]
    public static class CommerceServerStorefrontConstants
    {
        public static class KnownFieldNames
        {
            public const string CommerceServerPaymentMethods = "CS Payment Methods";

            public const string CommerceServerShippingMethods = "CS Shipping Methods";

            public const string CountryLocationPath = "Country location path";

            public const string CountryName = "Name";

            public const string CountryCode = "Country Code";

            public const string PaymentOptionValue = "Payment Option Value";

            public const string RegionName = "Name";

            public const string ShippingOptionValue = "Shipping Option Value";

            public const string ShippingOptionsLocationPath = "Shipping Options location path";

            public const string SupportsWishLists = "Supports Wishlists";

            public const string SupportsLoyaltyProgram = "Supports Loyalty Program ";

            public const string SupportsGirstCardPayment = "Supports Girft Card Payment";

            public const string Value = "Value";
        }
    }
}