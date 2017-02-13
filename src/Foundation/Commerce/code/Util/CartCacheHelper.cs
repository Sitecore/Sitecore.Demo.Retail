//---------------------------------------------------------------------
// <copyright file="CartCacheHelper.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The CartCacheHelper class</summary>
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
using System.Globalization;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Caching;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.Commerce.Util
{
    public class CartCacheHelper
    {
        public virtual void InvalidateCartCache([NotNull] string customerId)
        {
            var cacheProvider = GetCacheProvider();
            var id = GetCustomerId(customerId);

            if (!cacheProvider.Contains(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id))
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "CartCacheHelper::InvalidateCartCache - Cart for customer id {0} is not in the cache!", id);
                CommerceTrace.Current.Write(msg);
            }

            cacheProvider.RemoveData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id);

            CartCookieHelper.DeleteCartCookieForCustomer(id);
        }

        public virtual void AddCartToCache(CommerceCart cart)
        {
            var cacheProvider = GetCacheProvider();
            var id = GetCustomerId(cart.CustomerId);

            if (cacheProvider.Contains(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id))
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "CartCacheHelper::AddCartToCache - Cart for customer id {0} is already in the cache!", id);
                CommerceTrace.Current.Write(msg);
            }

            cacheProvider.AddData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id, cart);
            CartCookieHelper.CreateCartCookieForCustomer(id);
        }

        public virtual CommerceCart GetCart([NotNull] string customerId)
        {
            var cacheProvider = GetCacheProvider();

            var id = GetCustomerId(customerId);

            var cart = cacheProvider.GetData<CommerceCart>(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id);

            if (cart == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "CartCacheHelper::GetCart - Cart for customerId {0} does not exist in the cache!", id);
                CommerceTrace.Current.Write(msg);
            }

            return cart;
        }

        protected virtual string GetCustomerId([NotNull] string customerId)
        {
            Guid csCustomerId;
            return Guid.TryParse(customerId, out csCustomerId) ? Guid.Parse(customerId).ToString("D") : customerId;
        }

        private static ICacheProvider GetCacheProvider()
        {
            var cacheProvider = CommerceTypeLoader.GetCacheProvider(CommerceConstants.KnownCacheNames.CommerceCartCache);
            Assert.IsNotNull(cacheProvider, "cacheProvider");

            return cacheProvider;
        }
    }
}