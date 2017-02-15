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
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class SharedController : BaseController
    {
        private readonly RenderingModel _model;

        public SharedController([NotNull] ContactFactory contactFactory, [NotNull] CatalogManager catalogManager) : base(contactFactory)
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
                _model.Initialize(CurrentRendering);
                return _model;
            }
        }

        public ActionResult Layout()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult NavigationExtraStructure()
        {
            return View(GetRenderingView("Structures/NavigationExtraStructure"));
        }

        public ActionResult NavigationStructure()
        {
            return View(GetRenderingView("Structures/NavigationStructure"));
        }

        public ActionResult SingleColumn()
        {
            return View(GetRenderingView("Structures/SingleColumn"));
        }

        public ActionResult ThreeColumn()
        {
            return View(GetRenderingView("Structures/ThreeColumn"), CurrentRenderingModel);
        }

        public ActionResult TopStructure()
        {
            return View(GetRenderingView("Structures/TopStructure"));
        }

        public ActionResult TwoColumnCenterRight()
        {
            return View(GetRenderingView("Structures/TwoColumnCenterRight"), CurrentRenderingModel);
        }

        public ActionResult TwoColumnLeftCenter()
        {
            return View(GetRenderingView("Structures/TwoColumnLeftCenter"), CurrentRenderingModel);
        }

        public ActionResult Breadcrumbs()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult Error()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult ErrorImage()
        {
            return View(CurrentRenderingView, CurrentRenderingModel);
        }

        public ActionResult ErrorsSummary()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult FooterNavigation()
        {
            return View(CurrentRenderingView, CurrentRenderingModel);
        }

        public ActionResult LanguageSelector()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult Logo()
        {
            return View(CurrentRenderingView, CurrentRenderingModel);
        }

        public ActionResult ShareAndPrint()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult PrintOnly()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult SocialConnector()
        {
            return View(CurrentRenderingView);
        }

        public ActionResult TitleText()
        {
            return View(CurrentRenderingView, CurrentRenderingModel);
        }

        public ActionResult TopBarLinks()
        {
            return View(CurrentRenderingView, CurrentRenderingModel);
        }

        [HttpPost]
        public JsonResult CultureChosen(string culture)
        {
            var success = false;

            try
            {
                var result = CatalogManager.RaiseCultureChosenPageEvent(CurrentStorefront, culture);
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