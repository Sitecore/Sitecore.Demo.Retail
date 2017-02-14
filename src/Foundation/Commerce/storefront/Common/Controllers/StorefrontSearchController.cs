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
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models;
using Sitecore.Reference.Storefront.Models.RenderingModels;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class StorefrontSearchController : BaseController
    {
        private const string ChangeSiteContentPageSizeClass = "changeSiteContentPageSize";
        private const string CurrentCategoryViewModelKeyName = "CurrentCategoryViewModel";
        private const string CurrentSearchProductResultsKeyName = "CurrentSearchProductResults";
        private const string CurrentSearchContentResultsKeyName = "CurrentSearchContentResults";
        private const string CurrentSearchInfoKeyName = "CurrentSearchInfo";

        public StorefrontSearchController() : this(WindsorConfig.Container.Resolve<CatalogManager>(), WindsorConfig.Container.Resolve<ContactFactory>())
        {
        }

        public StorefrontSearchController([NotNull] CatalogManager catalogManager, [NotNull] ContactFactory contactFactory) : base(contactFactory)
        {
            Assert.ArgumentNotNull(catalogManager, nameof(catalogManager));
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            CatalogManager = catalogManager;
        }

        public CatalogManager CatalogManager { get; protected set; }

        public ActionResult SearchBar(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword)
        {
            var model = new SearchBarModel {SearchKeyword = searchKeyword};
            return View(CurrentRenderingView, model);
        }

        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult SearchEvent(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            if (searchInfo.SearchOptions != null)
            {
                var searchResult = GetChildProducts(searchInfo.SearchOptions, searchKeyword, searchInfo.Catalog.Name);

                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    CatalogManager.RegisterSearchEvent(CurrentStorefront, searchKeyword, searchResult.TotalItemCount);
                }
            }

            return View(CurrentRenderingView);
        }

        public ActionResult ProductSearchResultsFacets(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            if (searchKeyword == null)
            {
                searchKeyword = string.Empty;
            }

            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductFacetsViewModel(searchInfo.SearchOptions, searchKeyword, searchInfo.Catalog.Name, CurrentRendering);
            return View(GetAbsoluteRenderingView("/Catalog/ProductFacets"), viewModel);
        }

        public ActionResult ProductSearchResultsListHeader(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductListHeaderViewModel(searchInfo.SearchOptions, searchInfo.SortFields, searchInfo.SearchKeyword, searchInfo.Catalog.Name, CurrentRendering);
            return View(GetAbsoluteRenderingView("/Catalog/ProductListHeader"), viewModel);
        }

        public ActionResult ProductSearchResultsList(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductListViewModel(searchInfo.SearchOptions, searchInfo.SortFields, searchInfo.SearchKeyword, searchInfo.Catalog.Name, CurrentRendering);
            return View(GetAbsoluteRenderingView("/Catalog/ProductList"), viewModel);
        }

        public ActionResult ProductSearchResultsPagination(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetPaginationViewModel(searchInfo.SearchOptions, searchInfo.SearchKeyword, searchInfo.Catalog.Name, CurrentRendering);
            return View(GetRenderingView("Pagination"), viewModel);
        }

        public ActionResult SiteContentSearchResultsList(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SiteContentPaging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SiteContentPageSize)] int? pageSize)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, null, null, pageSize, null);
            var viewModel = GetSiteContentListViewModel(searchInfo.SearchOptions, searchKeyword, CurrentRendering);
            return View(GetAbsoluteRenderingView("/Catalog/SiteContentList"), viewModel);
        }

        public ActionResult SiteContentSearchResultsListHeader(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SiteContentPaging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SiteContentPageSize)] int? pageSize)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, null, null, pageSize, null);
            var viewModel = GetSiteContentListHeaderViewModel(searchInfo.SearchOptions, searchInfo.SearchKeyword, CurrentRendering);
            return View(GetAbsoluteRenderingView("/Catalog/ProductListHeader"), viewModel);
        }

        public ActionResult SiteContentSearchResultsPagination(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SiteContentPaging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SiteContentPageSize)] int? pageSize)
        {
            var searchInfo = GetSearchInfo(searchKeyword, pageNumber, null, null, pageSize, null);
            var viewModel = GetSiteContentPaginationModel(searchInfo.SearchOptions, searchInfo.SearchKeyword, CurrentRendering);
            return View(GetRenderingView("Pagination"), viewModel);
        }

        protected virtual ProductListHeaderViewModel GetProductListHeaderViewModel(CommerceSearchOptions productSearchOptions, IEnumerable<CommerceQuerySort> sortFields, string searchKeyword, string catalogName, Rendering rendering)
        {
            var viewModel = new ProductListHeaderViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, searchKeyword, catalogName);
            }

            viewModel.Initialize(rendering, childProducts, sortFields, productSearchOptions);
            return viewModel;
        }

        protected virtual ProductFacetsViewModel GetProductFacetsViewModel(CommerceSearchOptions productSearchOptions, string searchKeyword, string catalogName, Rendering rendering)
        {
            var viewModel = new ProductFacetsViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, searchKeyword, catalogName);
            }

            viewModel.Initialize(rendering, childProducts, productSearchOptions);

            return viewModel;
        }

        protected virtual PaginationViewModel GetPaginationViewModel(CommerceSearchOptions productSearchOptions, string searchKeyword, string catalogName, Rendering rendering)
        {
            var viewModel = new PaginationViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, searchKeyword, catalogName);
            }

            viewModel.Initialize(rendering, childProducts, productSearchOptions);
            return viewModel;
        }

        protected virtual CategoryViewModel GetProductListViewModel(CommerceSearchOptions productSearchOptions, IEnumerable<CommerceQuerySort> sortFields, string searchKeyword, string catalogName, Rendering rendering)
        {
            if (CurrentSiteContext.Items[CurrentCategoryViewModelKeyName] == null)
            {
                var categoryViewModel = new CategoryViewModel();

                var childProducts = GetChildProducts(productSearchOptions, searchKeyword, catalogName);

                categoryViewModel.Initialize(rendering, childProducts, sortFields, productSearchOptions);
                if (childProducts != null && childProducts.SearchResultItems.Count > 0)
                {
                    CatalogManager.GetProductBulkPrices(CurrentVisitorContext, categoryViewModel.ChildProducts);
                    CatalogManager.InventoryManager.GetProductsStockStatusForList(CurrentStorefront, categoryViewModel.ChildProducts);
                    foreach (var productViewModel in categoryViewModel.ChildProducts)
                    {
                        var productItem = childProducts.SearchResultItems.Where(item => item.Name == productViewModel.ProductId).Single();
                        productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);
                    }
                }

                CurrentSiteContext.Items[CurrentCategoryViewModelKeyName] = categoryViewModel;
            }

            var viewModel = (CategoryViewModel) CurrentSiteContext.Items[CurrentCategoryViewModelKeyName];
            return viewModel;
        }

        protected SearchResults GetChildProducts(CommerceSearchOptions searchOptions, string searchKeyword, string catalogName)
        {
            if (CurrentSiteContext.Items[CurrentSearchProductResultsKeyName] != null)
            {
                return (SearchResults) CurrentSiteContext.Items[CurrentSearchProductResultsKeyName];
            }

            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));

            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;
            var facets = Enumerable.Empty<CommerceQueryFacet>();

            if (Item != null && !string.IsNullOrEmpty(searchKeyword.Trim()))
            {
                SearchResponse searchResponse = null;
                searchResponse = SearchNavigation.SearchCatalogItemsByKeyword(searchKeyword, catalogName, searchOptions);

                if (searchResponse != null)
                {
                    returnList.AddRange(searchResponse.ResponseItems);
                    totalProductCount = searchResponse.TotalItemCount;
                    totalPageCount = searchResponse.TotalPageCount;
                    facets = searchResponse.Facets;
                }
            }

            var results = new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
            CurrentSiteContext.Items[CurrentSearchProductResultsKeyName] = results;
            return results;
        }

        protected virtual SearchResults GetSiteContentSearchResults(CommerceSearchOptions searchOptions, string searchKeyword, Rendering rendering)
        {
            if (CurrentSiteContext.Items[CurrentSearchContentResultsKeyName] != null)
            {
                return (SearchResults) CurrentSiteContext.Items[CurrentSearchContentResultsKeyName];
            }

            var searchResults = new SearchResults();
            var searchResponse = SearchNavigation.SearchSiteByKeyword(searchKeyword, searchOptions);
            if (searchResponse != null)
            {
                searchResults = new SearchResults(searchResponse.ResponseItems, searchResponse.TotalItemCount, searchResponse.TotalPageCount, searchOptions.StartPageIndex, searchResponse.Facets);
            }

            CurrentSiteContext.Items[CurrentSearchContentResultsKeyName] = searchResults;
            return searchResults;
        }

        protected virtual SiteContentSearchResultsViewModel GetSiteContentListViewModel(CommerceSearchOptions searchOptions, string searchKeyword, Rendering rendering)
        {
            var model = new SiteContentSearchResultsViewModel();
            model.Initialize(rendering);

            var searchResults = GetSiteContentSearchResults(searchOptions, searchKeyword, rendering);
            if (searchResults != null)
            {
                model.ContentItems = searchResults.SearchResultItems
                    .Select(item => SiteContentViewModel.Create(item))
                    .ToList();
            }

            return model;
        }

        protected virtual PaginationViewModel GetSiteContentPaginationModel(CommerceSearchOptions searchOptions, string searchKeyword, Rendering rendering)
        {
            var viewModel = new PaginationViewModel();

            SearchResults searchResults = null;
            if (searchOptions != null)
            {
                searchResults = GetSiteContentSearchResults(searchOptions, searchKeyword, rendering);
            }

            viewModel.Initialize(rendering, searchResults, searchOptions);
            viewModel.QueryStringToken = StorefrontConstants.QueryStrings.SiteContentPaging;

            return viewModel;
        }

        protected virtual ProductListHeaderViewModel GetSiteContentListHeaderViewModel(CommerceSearchOptions searchOptions, string searchKeyword, Rendering rendering)
        {
            var viewModel = new ProductListHeaderViewModel {PageSizeClass = ChangeSiteContentPageSizeClass};
            SearchResults searchResults = null;
            if (searchOptions != null)
            {
                searchResults = GetSiteContentSearchResults(searchOptions, searchKeyword, rendering);
            }

            viewModel.Initialize(rendering, searchResults, null, searchOptions);

            return viewModel;
        }

        protected void UpdateOptionsWithFacets(IEnumerable<CommerceQueryFacet> facets, string valueQueryString, CommerceSearchOptions productSearchOptions)
        {
            if (facets != null && facets.Any())
            {
                if (!string.IsNullOrEmpty(valueQueryString))
                {
                    var facetValuesCombos = valueQueryString.Split('&');

                    foreach (var facetValuesCombo in facetValuesCombos)
                    {
                        var facetValues = facetValuesCombo.Split('=');

                        var name = facetValues[0];

                        var existingFacet = facets.FirstOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                        if (existingFacet != null)
                        {
                            var values = facetValues[1].Split(StorefrontConstants.QueryStrings.FacetsSeparator);

                            foreach (var value in values)
                            {
                                existingFacet.Values.Add(value);
                            }
                        }
                    }
                }

                productSearchOptions.FacetFields = facets;
            }
        }

        protected void UpdateOptionsWithSorting(string sortField, CommerceConstants.SortDirection? sortDirection, CommerceSearchOptions productSearchOptions)
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

        private SearchInfo GetSearchInfo(string searchKeyword, int? pageNumber, string facetValues, string sortField, int? pageSize, CommerceConstants.SortDirection? sortDirection)
        {
            if (CurrentSiteContext.Items[CurrentSearchInfoKeyName] != null)
            {
                return (SearchInfo) CurrentSiteContext.Items[CurrentSearchInfoKeyName];
            }

            var searchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>();
            var searchInfo = new SearchInfo
            {
                SearchKeyword = searchKeyword ?? string.Empty,
                RequiredFacets = searchManager.GetFacetFieldsForItem(Item),
                SortFields = searchManager.GetSortFieldsForItem(Item),
                Catalog = StorefrontManager.CurrentStorefront.DefaultCatalog,
                ItemsPerPage = pageSize ?? searchManager.GetItemsPerPageForItem(Item)
            };
            if (searchInfo.ItemsPerPage == 0)
            {
                searchInfo.ItemsPerPage = StorefrontConstants.Settings.DefaultItemsPerPage;
            }

            var productSearchOptions = new CommerceSearchOptions(searchInfo.ItemsPerPage, pageNumber.GetValueOrDefault(0));
            UpdateOptionsWithFacets(searchInfo.RequiredFacets, facetValues, productSearchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, productSearchOptions);
            searchInfo.SearchOptions = productSearchOptions;

            CurrentSiteContext.Items[CurrentSearchInfoKeyName] = searchInfo;
            return searchInfo;
        }

        private class SearchInfo
        {
            public string SearchKeyword { get; set; }

            public IEnumerable<CommerceQueryFacet> RequiredFacets { get; set; }

            public IEnumerable<CommerceQuerySort> SortFields { get; set; }

            public int ItemsPerPage { get; set; }

            public Catalog Catalog { get; set; }

            public CommerceSearchOptions SearchOptions { get; set; }
        }
    }
}