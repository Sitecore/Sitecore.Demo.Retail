//-----------------------------------------------------------------------
// <copyright file="PaymentManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the payment business logic for the site.</summary>
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
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Entities.Payments;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Payments;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using GetPaymentMethodsRequest = Sitecore.Commerce.Engine.Connect.Services.Payments.GetPaymentMethodsRequest;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class PaymentManager : BaseManager
    {
        public PaymentManager([NotNull] PaymentServiceProvider paymentServiceProvider, [NotNull] CartManager cartManager)
        {
            Assert.ArgumentNotNull(paymentServiceProvider, nameof(paymentServiceProvider));

            PaymentServiceProvider = paymentServiceProvider;
            CartManager = cartManager;
        }

        public PaymentServiceProvider PaymentServiceProvider { get; protected set; }

        public CartManager CartManager { get; protected set; }

        public ManagerResponse<GetPaymentOptionsResult, IEnumerable<PaymentOption>> GetPaymentOptions([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));

            var result = new GetPaymentOptionsResult {Success = false};
            var cartResult = CartManager.GetCurrentCart(storefront, visitorContext);
            if (!cartResult.ServiceProviderResult.Success || cartResult.Result == null)
            {
                result.SystemMessages.ToList().AddRange(cartResult.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<GetPaymentOptionsResult, IEnumerable<PaymentOption>>(result, null);
            }

            var request = new GetPaymentOptionsRequest(storefront.ShopName, cartResult.Result);
            result = PaymentServiceProvider.GetPaymentOptions(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<GetPaymentOptionsResult, IEnumerable<PaymentOption>>(result, result.PaymentOptions.ToList());
        }

        public ManagerResponse<GetPaymentMethodsResult, IEnumerable<PaymentMethod>> GetPaymentMethods([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, PaymentOption paymentOption)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(paymentOption, nameof(paymentOption));

            var result = new GetPaymentMethodsResult {Success = false};
            var cartResult = CartManager.GetCurrentCart(storefront, visitorContext);
            if (!cartResult.ServiceProviderResult.Success || cartResult.Result == null)
            {
                result.SystemMessages.ToList().AddRange(cartResult.ServiceProviderResult.SystemMessages);
                return new ManagerResponse<GetPaymentMethodsResult, IEnumerable<PaymentMethod>>(result, null);
            }

            var request = new GetPaymentMethodsRequest(cartResult.Result, paymentOption);
            result = PaymentServiceProvider.GetPaymentMethods(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<GetPaymentMethodsResult, IEnumerable<PaymentMethod>>(result, result.PaymentMethods.ToList());
        }

        public ManagerResponse<PaymentClientTokenResult, string> GetPaymentClientToken()
        {
            var request = new ServiceProviderRequest();
            var result = PaymentServiceProvider.RunPipeline<ServiceProviderRequest, PaymentClientTokenResult>("commerce.payments.getClientToken", request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<PaymentClientTokenResult, string>(result, result.ClientToken);
        }
    }
}