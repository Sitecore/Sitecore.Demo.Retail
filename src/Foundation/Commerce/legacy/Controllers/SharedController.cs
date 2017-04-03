//-----------------------------------------------------------------------
// <copyright file="SharedController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the SharedController class.</summary>
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
using System.Web.Mvc;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class SharedController : SitecoreController
    {
        private readonly RenderingModel _model;

        public SharedController(CatalogManager catalogManager, StorefrontManager storefrontManager)
        {
            CatalogManager = catalogManager;
            StorefrontManager = storefrontManager;
        }

        public CatalogManager CatalogManager { get; }
        public StorefrontManager StorefrontManager { get; }

        public ActionResult ErrorsSummary()
        {
            return View();
        }

        public ActionResult LanguageSelector()
        {
            return View();
        }

        public ActionResult SocialConnector()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CultureChosen(string culture)
        {
            var success = false;

            try
            {
                var result = CatalogManager.RaiseCultureChosenPageEvent(StorefrontManager.Current, culture);
                success = result.Result;
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("CultureChosen", e), JsonRequestBehavior.AllowGet);
            }

            var json = new BaseApiModel {Success = success};
            return json;
        }
    }
}