//-----------------------------------------------------------------------
// <copyright file="BaseController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the BaseController class.</summary>
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
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Contacts;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class BaseController : SitecoreController
    {
        private Catalog _currentCatalog;
        private ICommerceSearchManager _currentSearchManager;
        private VisitorContext _currentVisitorContext;
        private ISiteContext _siteContext;

        public BaseController(ContactFactory contactFactory)
        {
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            ContactFactory = contactFactory;
        }

        public ContactFactory ContactFactory { get; }
        public ISiteContext CurrentSiteContext => _siteContext ?? (_siteContext = CommerceTypeLoader.CreateInstance<ISiteContext>());

        public virtual VisitorContext CurrentVisitorContext => _currentVisitorContext ?? (_currentVisitorContext = new VisitorContext(ContactFactory));

        public CommerceStorefront CurrentStorefront => StorefrontManager.CurrentStorefront;

        public Catalog CurrentCatalog
        {
            get
            {
                if (_currentCatalog != null)
                {
                    return _currentCatalog;
                }
                _currentCatalog = CurrentStorefront.DefaultCatalog ?? new Catalog(Context.Database.GetItem(CommerceConstants.KnownItemIds.DefaultCatalog));

                return _currentCatalog;
            }
        }

        public ICommerceSearchManager CurrentSearchManager => _currentSearchManager ??
                                                              (_currentSearchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>());

        protected Rendering CurrentRendering => RenderingContext.Current.Rendering;

        protected string CurrentRenderingView => GetRenderingView();

        protected Item Item
        {
            get { return RenderingContext.Current.Rendering.Item; }

            set { RenderingContext.Current.Rendering.Item = value; }
        }

        public ActionResult GetNoDataSourceView()
        {
            if (!Context.PageMode.IsExperienceEditor)
                return new EmptyResult();

            var home = Context.Database.GetItem(Context.Site.RootPath + Context.Site.StartItem);
            var noDatasourceItemPath = home["No Datasource Item"];

            Item noDatasourceItem = null;
            if (!string.IsNullOrEmpty(noDatasourceItemPath))
                noDatasourceItem = Context.Database.GetItem(noDatasourceItemPath);

            NoDataSourceViewModel viewModel = null;
            if (noDatasourceItem != null)
                viewModel = new NoDataSourceViewModel(noDatasourceItem);

            if (viewModel == null)
                return new EmptyResult();

            return View(GetRenderingView("NoDatasource"), viewModel);
        }

        public virtual void ValidateModel(BaseJsonResult result)
        {
            if (ModelState.IsValid)
                return;

            var errors = (from modelValue in ModelState.Values.Where(modelValue => modelValue.Errors.Any())
                from error in modelValue.Errors
                select error.ErrorMessage).ToList();
            result.SetErrors(errors);
        }

        protected string GetRenderingView(string renderingViewName = null)
        {
            var shopName = StorefrontManager.CurrentStorefront.ShopName;
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();

            if (string.IsNullOrWhiteSpace(renderingViewName))
                renderingViewName = RenderingContext.Current.Rendering.RenderingItem.Name;

            // TODO: mbe - Once tothers have checked in their code, rename the View/ShoppingCart folder to View/Cart.
            if (controllerName.Equals("Cart", StringComparison.OrdinalIgnoreCase))
                controllerName = "ShoppingCart";
            else if (controllerName.Equals("WishList", StringComparison.OrdinalIgnoreCase))
                controllerName = "Account";
            else if (controllerName.Equals("Loyalty", StringComparison.OrdinalIgnoreCase))
                controllerName = "Account";
            else if (controllerName.Equals("StorefrontSearch", StringComparison.OrdinalIgnoreCase))
                controllerName = "Search";

            var renderingViewPath = $"~/Views/{shopName}/{controllerName}/{renderingViewName}.cshtml";

            var result = ViewEngines.Engines.FindView(ControllerContext, renderingViewPath, null);

            if (result.View != null)
                return renderingViewPath;

            return string.Format(CultureInfo.InvariantCulture, "~/Views/{0}/{1}/{2}.cshtml", shopName, "Shared",
                renderingViewName);
        }

        protected string GetAbsoluteRenderingView(string subpath)
        {
            var shopName = StorefrontManager.CurrentStorefront.ShopName;

            if (!subpath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                subpath = subpath.Insert(0, "/");

            var renderingViewPath = $"~/Views/{shopName}{subpath}.cshtml";

            return renderingViewPath;
        }
    }
}