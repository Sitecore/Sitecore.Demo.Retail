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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Caching;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Diagnostics;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Website.Util
{
    [Service]
    public class CartCacheHelper
    {
        public void InvalidateCartCache(string customerId)
        {
            var cacheProvider = GetCacheProvider();
            var id = GetCustomerId(customerId);

            if (!cacheProvider.Contains(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id))
            {
                CommerceTrace.Current.Write($"CartCacheHelper::InvalidateCartCache - Cart for customer id {id} is not in the cache!");
            }

            cacheProvider.RemoveData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id);

            CartCookieHelper.DeleteCartCookieForCustomer(id);
        }

        public void AddCartToCache(CommerceCart cart)
        {
            var cacheProvider = GetCacheProvider();
            var id = GetCustomerId(cart.CustomerId);

            if (cacheProvider.Contains(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id))
            {
                CommerceTrace.Current.Write($"CartCacheHelper::AddCartToCache - Cart for customer id {id} is already in the cache!");
            }

            cacheProvider.AddData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id, cart);
            CartCookieHelper.CreateCartCookieForCustomer(id);
        }

        public CommerceCart GetCart(string customerId)
        {
            var cacheProvider = GetCacheProvider();

            var id = GetCustomerId(customerId);

            var cart = cacheProvider.GetData<CommerceCart>(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.CommerceCartCache, id);

            if (cart == null)
            {
                CommerceTrace.Current.Write($"CartCacheHelper::GetCart - Cart for customerId {id} does not exist in the cache!");
            }

            return cart;
        }

        private string GetCustomerId(string customerId)
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