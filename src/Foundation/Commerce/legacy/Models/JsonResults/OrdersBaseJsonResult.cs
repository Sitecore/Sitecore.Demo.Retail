﻿//-----------------------------------------------------------------------
// <copyright file="OrdersBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the OrdersBaseJsonResult class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    public class OrdersBaseJsonResult : BaseJsonResult
    {
        public OrdersBaseJsonResult()
        {
        }

        public OrdersBaseJsonResult(ServiceProviderResult result)
            : base(result)
        {
        }

        public List<OrderHeaderItemBaseJsonResult> Orders { get; } = new List<OrderHeaderItemBaseJsonResult>();

        public void Initialize(IEnumerable<OrderHeader> orderHeaders)
        {
            Assert.ArgumentNotNull(orderHeaders, nameof(orderHeaders));

            foreach (var orderHeader in orderHeaders)
            {
                var headerItem = new OrderHeaderItemBaseJsonResult(orderHeader);
                Orders.Add(headerItem);
            }
        }
    }
}