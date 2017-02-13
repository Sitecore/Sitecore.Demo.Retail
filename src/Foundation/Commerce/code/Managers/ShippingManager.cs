//-----------------------------------------------------------------------
// <copyright file="ShippingManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the shipping logic for the site.</summary>
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
using System.Linq;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Commerce.Services.Shipping;
using Sitecore.Commerce.Services.Shipping.Generics;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using GetShippingMethodsRequest = Sitecore.Commerce.Engine.Connect.Services.Shipping.GetShippingMethodsRequest;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class ShippingManager : BaseManager
    {
        public ShippingManager([NotNull] ShippingServiceProvider shippingServiceProvider, [NotNull] CartManager cartManager)
        {
            Assert.ArgumentNotNull(shippingServiceProvider, "shippingServiceProvider");
            Assert.ArgumentNotNull(cartManager, "cartManager");

            ShippingServiceProvider = shippingServiceProvider;
            CartManager = cartManager;
        }

        public ShippingServiceProvider ShippingServiceProvider { get; protected set; }

        public CartManager CartManager { get; protected set; }

        public ManagerResponse<GetShippingOptionsResult, List<ShippingOption>> GetShippingPreferences([NotNull] Cart cart)
        {
            Assert.ArgumentNotNull(cart, "cart");

            var request = new GetShippingOptionsRequest(cart);
            var result = ShippingServiceProvider.GetShippingOptions(request);
            if (result.Success && result.ShippingOptions != null)
            {
                return new ManagerResponse<GetShippingOptionsResult, List<ShippingOption>>(result, result.ShippingOptions.ToList());
            }

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetShippingOptionsResult, List<ShippingOption>>(result, null);
        }

        public ManagerResponse<GetShippingMethodsResult, IReadOnlyCollection<ShippingMethod>> GetShippingMethods([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] GetShippingMethodsInputModel inputModel)
        {
            Assert.ArgumentNotNull(storefront, "storefront");
            Assert.ArgumentNotNull(visitorContext, "visitorContext");
            Assert.ArgumentNotNull(inputModel, "inputModel");

            var result = new GetShippingMethodsResult {Success = false};
            var cartResult = CartManager.GetCurrentCart(storefront, visitorContext);
            if (!cartResult.ServiceProviderResult.Success || cartResult.Result == null)
            {
                result.SystemMessages.ToList().AddRange(cartResult.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<GetShippingMethodsResult, IReadOnlyCollection<ShippingMethod>>(result, null);
            }

            var cart = cartResult.Result;
            var preferenceType = InputModelExtension.GetShippingOptionType(inputModel.ShippingPreferenceType);

            var request = new GetShippingMethodsRequest(
                new ShippingOption {ShippingOptionType = preferenceType},
                inputModel.ShippingAddress != null ? inputModel.ShippingAddress.ToParty() : null,
                cart)
            {
                Lines = inputModel.Lines != null ? inputModel.Lines.ToCommerceCartLines() : null
            };

            result = ShippingServiceProvider.GetShippingMethods<Sitecore.Commerce.Services.Shipping.GetShippingMethodsRequest, GetShippingMethodsResult>(request);
            return new ManagerResponse<GetShippingMethodsResult, IReadOnlyCollection<ShippingMethod>>(result, result.ShippingMethods);
        }
    }
}