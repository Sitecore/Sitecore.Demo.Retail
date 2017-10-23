//-----------------------------------------------------------------------
// <copyright file="UrlExtensionMethods.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the ProfileExtensions class.</summary>
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

using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Sitecore.Foundation.Commerce.Website.Util;

namespace Sitecore.Foundation.Commerce.Website.Extensions
{
    public static class UrlExtensionMethods
    {
        public static string AddPageNumber(this UrlHelper helper, int page, string queryStringToken)
        {
            var current = UrlBuilder.CurrentUrl;
            current.QueryList.AddOrSet(queryStringToken, page.ToString(CultureInfo.InvariantCulture));

            CleanNestedCollections(current);

            var url = current.ToString(true);

            return url;
        }

        private static void CleanNestedCollections(UrlBuilder current)
        {
            foreach (var key in current.QueryList.AllKeys)
            {
                var value = current.QueryList[key];
                if (value == null)
                {
                    continue;
                }
                var decodedValue = HttpUtility.UrlDecode(value);
                current.QueryList.AddOrSet(key, decodedValue);
            }
        }
    }
}