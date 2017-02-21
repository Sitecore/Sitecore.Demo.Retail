//-----------------------------------------------------------------------
// <copyright file="NavigationSearchController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the NavigationSearchController class.</summary>
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
using System.Linq.Expressions;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class NavigationSearchController : BaseController
    {
        public NavigationSearchController([NotNull] AccountManager accountManager, [NotNull] ContactFactory contactFactory) : base(accountManager, contactFactory)
        {
        }

        public string IndexName { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public override ActionResult Index()
        {
            var dataSourceQuery = RenderingContext.Current.Rendering.DataSource;

            if (string.IsNullOrWhiteSpace(dataSourceQuery))
            {
                return View(Enumerable.Empty<Category>());
            }
            var response = GetNavigationCategories(dataSourceQuery, new CommerceSearchOptions());
            var navigationResults = response.ResponseItems;

            return View(navigationResults.Select(result => new Category(result)));
        }

        private SearchResponse GetNavigationCategories(string navigationDataSource, CommerceSearchOptions searchOptions)
        {
            ID navigationId;
            var searchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>();
            var searchIndex = searchManager.GetIndex();

            if (navigationDataSource.IsGuid())
            {
                navigationId = ID.Parse(navigationDataSource);
            }
            else
            {
                using (var context = searchIndex.CreateSearchContext())
                {
                    var query = LinqHelper.CreateQuery<SitecoreUISearchResultItem>(context, SearchStringModel.ParseDatasourceString(navigationDataSource)).Select(result => result.GetItem().ID);
                    if (query.Any())
                    {
                        navigationId = query.First();
                    }
                    else
                    {
                        return new SearchResponse();
                    }
                }
            }

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceBaseCatalogSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(item => item.CommerceAncestorIds.Contains(navigationId))
                    .Select(p => new CommerceBaseCatalogSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    });

                searchResults = searchManager.AddSearchOptionsToQuery(searchResults, searchOptions);

                var results = searchResults.GetResults();
                var response = SearchResponse.CreateFromSearchResultsItems(searchOptions, results);

                return response;
            }
        }
    }
}