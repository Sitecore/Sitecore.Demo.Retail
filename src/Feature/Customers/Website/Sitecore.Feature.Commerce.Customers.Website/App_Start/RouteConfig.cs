//-----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the RouteConfig class.</summary>
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
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Customers.Website
{
    public static class RouteConfig
    {
        private static readonly List<ApiControllerMapping> _apiInfoList = new List<ApiControllerMapping>
        {
            new ApiControllerMapping("account-getcurrentuser", "Customers", "GetCurrentUser"),
            new ApiControllerMapping("account-register", "Customers", "Register"),
            new ApiControllerMapping("account-addresslist", "Customers", "AddressList"),
            new ApiControllerMapping("account-addressdelete", "Customers", "AddressDelete"),
            new ApiControllerMapping("account-addressmodify", "Customers", "AddressModify"),
            new ApiControllerMapping("account-updateprofile", "Customers", "UpdateProfile"),
            new ApiControllerMapping("account-changepassword", "Customers", "ChangePassword"),
        };

        public static void RegisterRoutes(RouteCollection routes)
        {
            foreach (var apiInfo in _apiInfoList)
            {
                routes.MapRoute(
                    apiInfo.Name,
                    apiInfo.Url,
                    new {controller = apiInfo.Controller, action = apiInfo.Action, id = UrlParameter.Optional});
            }
        }
    }
}