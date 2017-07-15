//-----------------------------------------------------------------------
// <copyright file="OrderHeaderItemBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the OrderHeaderItemBaseJsonResult class.</summary>
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

using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Models
{
    public class OrderHeaderApiModel
    {
        public OrderHeaderApiModel(OrderHeader header)
        {
            ExternalId = header.ExternalId;
            Status = OrderManager.GetOrderStatusName(header.Status);
            LastModified = ((CommerceOrderHeader) header).LastModified.ToDisplayedDate();
            DetailsUrl = string.Concat("/accountmanagement/myorder", "?id=", header.ExternalId);
            OrderId = header.OrderID;
        }

        public string ExternalId { get; protected set; }

        public string OrderId { get; protected set; }

        public string Status { get; protected set; }

        public string LastModified { get; protected set; }

        public string DetailsUrl { get; protected set; }
    }
}