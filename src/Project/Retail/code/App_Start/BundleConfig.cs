//-----------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the BundleConfig class.</summary>
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

namespace Sitecore.Reference.Storefront
{
    using System.Web.Optimization;

    /// <summary>
    /// Used to register all bundles
    /// </summary>
    public static class BundleConfig
    {
        /// <summary>
        /// Called to register any bundles
        /// </summary>
        /// <param name="bundles">The bundles collection to add to</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            bundles.Add(new ScriptBundle("~/js/jquery").Include(
                        "~/Scripts/jquery/jquery-2.0.3.js",
                        "~/Scripts/jquery/jquery.cookie.js",
                        "~/Scripts/jquery/jquery.blockUI.js",
                        "~/Scripts/jquery/jquery-ui-1.10.4.js"));

            bundles.Add(new ScriptBundle("~/js/jqueryval").Include(
                        "~/Scripts/jquery/jquery.validate*",
                        "~/Scripts/jquery/jquery.unobtrusive*"));

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
                "~/Scripts/Catalog/jsuri.js",
                "~/Scripts/autoNumeric.js",
                ////"~/Scripts/Storefront/knockout-3.2.0.debug.js",
                "~/Scripts/Orders/main.js",
                "~/Scripts/Orders/debug-knockout.js",
                "~/Scripts/ViewModels/errorsummary_VM.js",
                "~/Scripts/ViewModels/minicart_VM.js",
                "~/Scripts/ViewModels/delivery_VM.js",
                "~/Scripts/ViewModels/confirm_VM.js",
                "~/Scripts/ViewModels/billing_VM.js",
                "~/Scripts/ViewModels/StepIndicator.js",
                "~/Scripts/ViewModels/checkoutData_VM.js",
                "~/Scripts/ViewModels/wishlist_VM.js",
                "~/Scripts/ViewModels/orders_VM.js",
                "~/Scripts/ViewModels/loyaltycards_VM.js",
                "~/Scripts/ViewModels/addresses_VM.js",
                "~/Scripts/ViewModels/CartKnockoutModels.js",
                "~/Scripts/Maps/maps.js",
                "~/Scripts/Storefront/product-details.js",
                "~/Scripts/Orders/cart.js",
                "~/Scripts/Storefront/passwordmanagement.js",
                "~/Scripts/Storefront/wishlist.js",
                "~/Scripts/Storefront/loyaltycards.js",
                "~/Scripts/Orders/orders.js",
                "~/Scripts/Orders/checkout.js",
                "~/Scripts/Customers/addresses.js",
                "~/Scripts/Customers/register.js",
                "~/Scripts/Storefront/search.js",
                "~/Scripts/Customers/editprofile.js"));
            bundles.Add(new ScriptBundle("~/js/habitat").Include(
                "~/scripts/Sitecore.Foundation.Theming.js"));
        }
    }
}