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
using Sitecore.Commerce.Contacts;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class SharedController : SitecoreController
    {
        private readonly RenderingModel _model;

        public SharedController([NotNull] CatalogManager catalogManager)
        {
            Assert.ArgumentNotNull(catalogManager, nameof(catalogManager));

            _model = new RenderingModel();

            CatalogManager = catalogManager;
        }

        public CatalogManager CatalogManager { get; }

        internal RenderingModel CurrentRenderingModel
        {
            get
            {
                _model.Initialize(RenderingContext.Current.Rendering);
                return _model;
            }
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult ErrorImage()
        {
            return View(CurrentRenderingModel);
        }

        public ActionResult ErrorsSummary()
        {
            return View();
        }

        public ActionResult LanguageSelector()
        {
            return View();
        }

        public ActionResult ShareAndPrint()
        {
            return View();
        }

        public ActionResult PrintOnly()
        {
            return View();
        }

        public ActionResult SocialConnector()
        {
            return View();
        }

        public ActionResult TitleText()
        {
            return View(CurrentRenderingModel);
        }

        public ActionResult TopBarLinks()
        {
            return View(CurrentRenderingModel);
        }

        [HttpPost]
        public JsonResult CultureChosen(string culture)
        {
            var success = false;

            try
            {
                var result = CatalogManager.RaiseCultureChosenPageEvent(StorefrontManager.CurrentStorefront, culture);
                success = result.Result;
            }
            catch (Exception e)
            {
                return Json(new BaseJsonResult("CultureChosen", e), JsonRequestBehavior.AllowGet);
            }

            var json = new BaseJsonResult {Success = success};
            return json;
        }
    }
}