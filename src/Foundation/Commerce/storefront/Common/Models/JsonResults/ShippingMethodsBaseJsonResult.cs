﻿//-----------------------------------------------------------------------
// <copyright file="ShippingMethodsBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the ShippingMethodsBaseJsonResult class.</summary>
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

namespace Sitecore.Reference.Storefront.Models.JsonResults
{
    using Sitecore.Commerce.Connect.CommerceServer;
    using Sitecore.Commerce.Entities.Shipping;
    using Sitecore.Commerce.Services.Shipping;
    using System.Collections.Generic;

    /// <summary>
    /// The Json result of a request to retrieve nearby store locations.
    /// </summary>
    public class ShippingMethodsBaseJsonResult : BaseJsonResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShippingMethodsBaseJsonResult"/> class.
        /// </summary>
        public ShippingMethodsBaseJsonResult()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShippingMethodsBaseJsonResult"/> class.
        /// </summary>
        /// <param name="result">The service provider result.</param>
        public ShippingMethodsBaseJsonResult(GetShippingMethodsResult result)
            : base(result)
        {
        } 

        /// <summary>
        /// Gets or sets the available order-level shipping methods.
        /// </summary>
        public IEnumerable<ShippingMethodBaseJsonResult> ShippingMethods { get; set; }

        /// <summary>
        /// Initilizes the specified shipping methods.
        /// </summary>
        /// <param name="shippingMethods">The shipping methods.</param>        
        public virtual void Initialize(IEnumerable<ShippingMethod> shippingMethods)
        {
            if (shippingMethods == null)
            {
                return;
            }

            var shippingMethodList = new List<ShippingMethodBaseJsonResult>();

            foreach (var shippingMethod in shippingMethods)
            {
                var jsonResult = CommerceTypeLoader.CreateInstance<ShippingMethodBaseJsonResult>();

                jsonResult.Initialize(shippingMethod);
                shippingMethodList.Add(jsonResult);
            }

            this.ShippingMethods = shippingMethodList;
        }
    }
}