//-----------------------------------------------------------------------
// <copyright file="SearchController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the SearchController class.</summary>
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Indexing.Models;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class StorefrontSearchController : SitecoreController
    {
        public const string SiteContentPaging = "scpg";
        public const string SiteContentPageSize = "scps";

        public StorefrontSearchController(CatalogManager catalogManager, ICommerceSearchManager commerceSearchManager)
        {
            CatalogManager = catalogManager;
            CommerceSearchManager = commerceSearchManager;
        }

        private CatalogManager CatalogManager { get; }
        public ICommerceSearchManager CommerceSearchManager { get; }

        public ActionResult SearchBar([Bind(Prefix = Foundation.Commerce.Constants.QueryString.SearchKeyword)] string searchKeyword)
        {
            var model = new SearchBarViewModel {SearchKeyword = searchKeyword};
            return View(model);
        }

        public ActionResult SiteContentSearchResultsList(
            [Bind(Prefix = Foundation.Commerce.Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = SiteContentPaging)] int? pageNumber,
            [Bind(Prefix = SiteContentPageSize)] int? pageSize)
        {
            //TODO: Rewrite based on Habitat Foundation.Indexing
            return new EmptyResult();
        }

        public ActionResult SiteContentSearchResultsListHeader(
            [Bind(Prefix = Foundation.Commerce.Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = SiteContentPaging)] int? pageNumber,
            [Bind(Prefix = SiteContentPageSize)] int? pageSize)
        {
            //TODO: Rewrite based on Habitat Foundation.Indexing
            return new EmptyResult();
        }

        public ActionResult SiteContentSearchResultsPagination(
            [Bind(Prefix = Foundation.Commerce.Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = SiteContentPaging)] int? pageNumber,
            [Bind(Prefix = SiteContentPageSize)] int? pageSize)
        {
            //TODO: Rewrite based on Habitat Foundation.Indexing
            return new EmptyResult();
        }
    }
}