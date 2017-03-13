//---------------------------------------------------------------------
// <copyright file="InitializeBundles.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The route ininitialization</summary>
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

using System.Web.Optimization;
using Sitecore.Pipelines;

namespace Sitecore.Reference.Storefront.Infrastructure.SitecorePipelines
{
    public class InitializeBundles
    {
        public void Process(PipelineArgs args)
        {
            if (Context.IsUnitTesting)
            {
                return;
            }

            BundleTable.EnableOptimizations = true;
            RegisterBundles(BundleTable.Bundles);
        }

        private static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            bundles.Add(new ScriptBundle("~/js/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.cookie.js",
                "~/Scripts/jquery.blockUI.js",
                "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/js/jqueryval").Include(
                "~/Scripts/jquery.validate*",
                "~/Scripts/jquery.unobtrusive*"));

            bundles.Add(new ScriptBundle("~/js/bootstrap").Include(
                "~/Scripts/bootstrap-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/js/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/js/knockout").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.validation-{version}.js"));

            bundles.Add(new ScriptBundle("~/js/storefront/bootstrap").Include(
                "~/Scripts/Storefront/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/js/storefront").Include(
                "~/Scripts/jsuri-{version}.js",
                "~/Scripts/autoNumeric.js",
                "~/Scripts/Storefront/main.js",
                "~/Scripts/Storefront/debug-knockout.js",
                "~/Scripts/Storefront/ViewModels/errorsummary_VM.js",
                "~/Scripts/Storefront/ViewModels/minicart_VM.js",
                "~/Scripts/Storefront/ViewModels/delivery_VM.js",
                "~/Scripts/Storefront/ViewModels/confirm_VM.js",
                "~/Scripts/Storefront/ViewModels/billing_VM.js",
                "~/Scripts/Storefront/ViewModels/StepIndicator.js",
                "~/Scripts/Storefront/ViewModels/checkoutData_VM.js",
                "~/Scripts/Storefront/ViewModels/orders_VM.js",
                "~/Scripts/Storefront/ViewModels/addresses_VM.js",
                "~/Scripts/Storefront/ViewModels/CartKnockoutModels.js",
                "~/Scripts/Storefront/maps.js",
                "~/Scripts/Storefront/product-details.js",
                "~/Scripts/Storefront/cart.js",
                "~/Scripts/Storefront/passwordmanagement.js",
                "~/Scripts/Storefront/order.js",
                "~/Scripts/Storefront/checkout.js",
                "~/Scripts/Storefront/addresses.js",
                "~/Scripts/Storefront/register.js",
                "~/Scripts/Storefront/search.js",
                "~/Scripts/Storefront/editprofile.js"));

            bundles.Add(new ScriptBundle("~/js/habitat").Include(
                "~/scripts/Sitecore.Foundation.Theming.js"));
        }
    }
}