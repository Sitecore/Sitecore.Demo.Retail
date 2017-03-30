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
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Contacts;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Foundation.Alerts.Extensions;
using Sitecore.Foundation.Alerts.Models;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Controllers
{
    public class ProductSearchController : SitecoreController
    {
        private const string CurrentCategoryViewModelKeyName = "CurrentCategoryViewModel";
        private const string CurrentSearchProductResultsKeyName = "CurrentSearchProductResults";

        public ProductSearchController(AccountManager accountManager, CatalogManager catalogManager, ContactFactory contactFactory, CommerceUserContext commerceUserContext, CatalogItemContext catalogItemContext, ProductSearchManager productSearchManager, StorefrontContext storefrontContext)
        {
            CommerceUserContext = commerceUserContext;
            CatalogItemContext = catalogItemContext;
            CatalogManager = catalogManager;
            ProductSearchManager = productSearchManager;
            StorefrontContext = storefrontContext;
        }

        private ProductSearchManager ProductSearchManager { get; }
        public StorefrontContext StorefrontContext { get; }
        private CommerceUserContext CommerceUserContext { get; }
        public CatalogItemContext CatalogItemContext { get; }
        private CatalogManager CatalogManager { get; }

        public ActionResult ProductSearchResultsListHeader(
            [Bind(Prefix = Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductListHeaderViewModel(searchInfo);
            return View(viewModel);
        }

        private ProductListHeaderViewModel GetProductListHeaderViewModel(SearchInfo searchInfo)
        {
            SearchResults searchResults = null;
            var options = searchInfo.SearchOptions;
            if (options != null)
            {
                searchResults = GetSearchResults(searchInfo.Catalog.Name, searchInfo.SearchKeyword, options);
            }

            var viewModel = new ProductListHeaderViewModel();
            viewModel.Initialize(RenderingContext.Current.Rendering, searchResults, searchInfo.SortFields, options);
            return viewModel;
        }

        public ActionResult ProductSearchResultsFacets(
            [Bind(Prefix = Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            if (searchKeyword == null)
            {
                searchKeyword = string.Empty;
            }

            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductFacetsViewModel(searchInfo.Catalog.Name, searchKeyword, searchInfo.SearchOptions);
            return View(viewModel);
        }

        private ProductFacetsViewModel GetProductFacetsViewModel(string catalogName, string searchKeyword, SearchOptions searchOptions)
        {
            SearchResults searchResults = null;
            if (searchOptions != null)
            {
                searchResults = GetSearchResults(catalogName, searchKeyword, searchOptions);
            }

            var viewModel = new ProductFacetsViewModel(searchResults?.Facets ?? searchOptions?.FacetFields);

            return viewModel;
        }

        public ActionResult SearchBar([Bind(Prefix = Constants.QueryString.SearchKeyword)] string searchKeyword)
        {
            var model = new SearchBarViewModel { SearchKeyword = searchKeyword };
            return View(model);
        }

        public ActionResult ProductSearchResultsList(
            [Bind(Prefix = Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductSearchResultViewModel(searchInfo);
            return View(viewModel);
        }

        private SearchResultViewModel GetProductSearchResultViewModel(SearchInfo searchInfo)
        {
            var viewModel = this.GetFromCache<SearchResultViewModel>(CurrentCategoryViewModelKeyName);
            if (viewModel != null)
            {
                return viewModel;
            }
            var options = searchInfo.SearchOptions;
            var results = GetSearchResults(searchInfo.Catalog.Name, searchInfo.SearchKeyword, options);
            if (results == null || results.SearchResultItems.Count <= 0)
            {
                return null;
            }

            CatalogManager.RegisterSearchEvent(searchInfo.SearchKeyword, results.TotalItemCount);

            viewModel = new SearchResultViewModel(results);

            var products = viewModel.Items.Where(i => i is ProductViewModel).Cast<ProductViewModel>().ToList();
            CatalogManager.GetProductBulkPrices(products);
            CatalogManager.InventoryManager.GetProductsStockStatusForList(products);
            foreach (var productViewModel in products)
            {
                productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productViewModel.Item);
            }

            return this.AddToCache(CurrentCategoryViewModelKeyName, viewModel);
        }

        private SearchResults GetSearchResults(string catalogName, string searchKeyword, SearchOptions searchOptions)
        {
            var results = this.GetFromCache<SearchResults>(CurrentSearchProductResultsKeyName);
            if (results != null)
            {
                return results;
            }


            results = ProductSearchManager.GetSearchResults(catalogName, searchKeyword, searchOptions);

            return this.AddToCache(CurrentSearchProductResultsKeyName, results);
        }

        public ActionResult ProductSearchResultsPagination(
            [Bind(Prefix = Constants.QueryString.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetPaginationViewModel(searchInfo);
            if (viewModel.Pagination.NumberOfPages <= 1)
            {
                return new EmptyResult();
            }
            return View(viewModel);
        }

        private PaginationViewModel GetPaginationViewModel(SearchInfo searchInfo)
        {
            var searchOptions = searchInfo.SearchOptions;
            var viewModel = new PaginationViewModel();

            SearchResults searchResults = null;
            if (searchOptions != null)
            {
                searchResults = GetSearchResults(searchInfo.Catalog.Name, searchInfo.SearchKeyword, searchOptions);
            }

            viewModel.Initialize(RenderingContext.Current.Rendering, searchResults, searchOptions);
            return viewModel;
        }

        private void UpdateOptionsWithFacets(IEnumerable<QueryFacet> facets, string valueQueryString, SearchOptions productSearchOptions)
        {
            if (facets == null)
                return;

            var queryFacets = facets.ToList();
            if (!queryFacets.Any())
            {
                return;
            }

            if (!string.IsNullOrEmpty(valueQueryString))
            {
                var facetValuesCombos = valueQueryString.Split('&');

                foreach (var facetValuesCombo in facetValuesCombos)
                {
                    var facetValues = facetValuesCombo.Split('=');
                    var name = facetValues[0];
                    var existingFacet = queryFacets.FirstOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (existingFacet == null)
                    {
                        continue;
                    }
                    var values = facetValues[1].Split(Constants.FacetsSeparator);

                    foreach (var value in values)
                    {
                        existingFacet.Values.Add(value);
                    }
                }
            }

            productSearchOptions.FacetFields = queryFacets;
        }

        private void UpdateOptionsWithSorting(string sortField, SortDirection? sortDirection, SearchOptions productSearchOptions)
        {
            if (!string.IsNullOrEmpty(sortField))
            {
                productSearchOptions.SortField = sortField;

                if (sortDirection.HasValue)
                {
                    productSearchOptions.SortDirection = sortDirection.Value;
                }

                ViewBag.SortField = sortField;
                ViewBag.SortDirection = sortDirection;
            }
        }

        private SearchInfo GetSearchInfo(string searchKeyword, int? pageNumber, string facetValues, string sortField, int? pageSize, SortDirection? sortDirection)
        {
            var searchInfo = this.GetFromCache<SearchInfo>("CurrentProductSearchInfo");
            if (searchInfo != null)
            {
                return searchInfo;
            }

            searchInfo = new SearchInfo
            {
                SearchKeyword = searchKeyword ?? string.Empty,
                RequiredFacets = ProductSearchManager.GetFacetFieldsForItem(RenderingContext.Current.Rendering.Item),
                SortFields = ProductSearchManager.GetSortFieldsForItem(RenderingContext.Current.Rendering.Item),
                Catalog = CatalogManager.CatalogContext.CurrentCatalog,
                ItemsPerPage = pageSize ?? ProductSearchManager.GetItemsPerPageForItem(RenderingContext.Current.Rendering.Item)
            };
            if (searchInfo.ItemsPerPage <= 0)
            {
                searchInfo.ItemsPerPage = 12;
            }

            var productSearchOptions = new SearchOptions(searchInfo.ItemsPerPage, pageNumber.GetValueOrDefault(0));
            UpdateOptionsWithFacets(searchInfo.RequiredFacets, facetValues, productSearchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, productSearchOptions);
            searchInfo.SearchOptions = productSearchOptions;

            return this.AddToCache("CurrentProductSearchInfo", searchInfo);
        }
    }
}