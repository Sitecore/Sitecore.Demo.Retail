//-----------------------------------------------------------------------
// <copyright file="CartCookieHelper.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the cart cookie helper class.</summary>
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
using System.Web;

namespace Sitecore.Foundation.Commerce.Website.Util
{
    public static class CartCookieHelper
    {
        public const string CookieName = "_minicart";

        public const string VisitorIdKey = "VisitorId";

        public const string AnonymousCartCookieName = "asc";

        private const int CookieExpirationInDays = 365;

        public static bool DoesCookieExistForCustomer(string customerId)
        {
            var cartCookie = HttpContext.Current.Request.Cookies[CookieName];

            return cartCookie != null && cartCookie.Values[VisitorIdKey] == customerId;
        }

        public static void CreateCartCookieForCustomer(string customerId)
        {
            var cartCookie = HttpContext.Current.Request.Cookies[CookieName] ?? new HttpCookie(CookieName);
            cartCookie.Values[VisitorIdKey] = customerId;
            cartCookie.HttpOnly = true;
            cartCookie.Expires = DateTime.Now.AddDays(CookieExpirationInDays);
            HttpContext.Current.Response.Cookies.Add(cartCookie);
        }

        public static bool DeleteCartCookieForCustomer(string customerId)
        {
            var cartCookie = HttpContext.Current.Request.Cookies[CookieName];
            if (cartCookie == null)
            {
                return false;
            }

            // invalidate the cookie
            HttpContext.Current.Response.Cookies.Remove(CookieName);
            cartCookie.Expires = DateTime.Now.AddDays(-10);
            cartCookie.Values[VisitorIdKey] = null;
            cartCookie.Value = null;
            HttpContext.Current.Response.SetCookie(cartCookie);

            return true;
        }

        public static string GetAnonymousCartIdFromCookie()
        {
            var cartCookie = HttpContext.Current.Request.Cookies[AnonymousCartCookieName];

            if (cartCookie == null || string.IsNullOrEmpty(cartCookie.Value))
            {
                var cartId = Guid.NewGuid().ToString();
                cartCookie = new HttpCookie(AnonymousCartCookieName, cartId) {HttpOnly = true};
                HttpContext.Current.Response.SetCookie(cartCookie);
                return cartId;
            }

            return cartCookie.Value;
        }
    }
}