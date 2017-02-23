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
using Sitecore.Configuration;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Foundation.Commerce;
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
        private const string CurrentSearchInfoKeyName = "CurrentProductSearchInfo";

        public ProductSearchController([NotNull] AccountManager accountManager, [NotNull] CatalogManager catalogManager, [NotNull] ContactFactory contactFactory, [NotNull] VisitorContextRepository visitorContextRepository, SiteContextRepository siteContextRepository)
        {
            Assert.ArgumentNotNull(catalogManager, nameof(catalogManager));
            Assert.ArgumentNotNull(visitorContextRepository, nameof(visitorContextRepository));

            VisitorContextRepository = visitorContextRepository;
            SiteContextRepository = siteContextRepository;
            CatalogManager = catalogManager;
        }

        private VisitorContextRepository VisitorContextRepository { get; }
        public SiteContextRepository SiteContextRepository { get; }
        private CatalogManager CatalogManager { get; }

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
                    CatalogManager.RegisterSearchEvent(StorefrontManager.CurrentStorefront, searchKeyword, searchResult.TotalItemCount);
                }
            }

            return View();
        }

        public ActionResult ProductSearchResultsListHeader(
            [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = this.GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = this.GetProductListHeaderViewModel(searchInfo);
            return this.View(viewModel);
        }

        private ProductListHeaderViewModel GetProductListHeaderViewModel(SearchInfo searchInfo)
        {
            SearchResults childProducts = null;
            if (searchInfo.SearchOptions != null)
            {
                childProducts = GetChildProducts(searchInfo.SearchOptions, searchInfo.SearchKeyword, searchInfo.Catalog.Name);
            }

            var viewModel = new ProductListHeaderViewModel();
            viewModel.Initialize(RenderingContext.Current.Rendering, childProducts, searchInfo.SortFields, searchInfo.SearchOptions);
            return viewModel;
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

            var searchInfo = this.GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = this.GetProductFacetsViewModel(searchInfo.SearchOptions, searchKeyword, searchInfo.Catalog.Name, RenderingContext.Current.Rendering);
            return this.View(viewModel);
        }

        private ProductFacetsViewModel GetProductFacetsViewModel(CommerceSearchOptions productSearchOptions, string searchKeyword, string catalogName, Rendering rendering)
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

        public ActionResult ProductSearchResultsList(
                   [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
                   [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
                   [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
                   [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
                   [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
                   [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = this.GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = GetProductListViewModel(searchInfo);
            return this.View(viewModel);
        }

        private CategoryViewModel GetProductListViewModel(SearchInfo searchInfo)
        {
            if (SiteContextRepository.GetCurrent().Items[CurrentCategoryViewModelKeyName] == null)
            {
                var categoryViewModel = new CategoryViewModel();

                var childProducts = GetChildProducts(searchInfo.SearchOptions, searchInfo.SearchKeyword, searchInfo.Catalog.Name);

                categoryViewModel.Initialize(RenderingContext.Current.Rendering, childProducts, searchInfo.SortFields, searchInfo.SearchOptions);
                if (childProducts != null && childProducts.SearchResultItems.Count > 0)
                {
                    CatalogManager.GetProductBulkPrices(VisitorContextRepository.GetCurrent(), categoryViewModel.ChildProducts);
                    CatalogManager.InventoryManager.GetProductsStockStatusForList(StorefrontManager.CurrentStorefront, categoryViewModel.ChildProducts);
                    foreach (var productViewModel in categoryViewModel.ChildProducts)
                    {
                        var productItem = childProducts.SearchResultItems.Single(item => item.Name == productViewModel.ProductId);
                        productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);
                    }
                }

                SiteContextRepository.GetCurrent().Items[CurrentCategoryViewModelKeyName] = categoryViewModel;
            }

            var viewModel = (CategoryViewModel) SiteContextRepository.GetCurrent().Items[CurrentCategoryViewModelKeyName];
            return viewModel;
        }

        private SearchResults GetChildProducts(CommerceSearchOptions searchOptions, string searchKeyword, string catalogName)
        {
            if (SiteContextRepository.GetCurrent().Items[CurrentSearchProductResultsKeyName] != null)
            {
                return (SearchResults)SiteContextRepository.GetCurrent().Items[CurrentSearchProductResultsKeyName];
            }

            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));

            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;
            var facets = Enumerable.Empty<CommerceQueryFacet>();

            if (RenderingContext.Current.Rendering.Item != null && !string.IsNullOrEmpty(searchKeyword.Trim()))
            {
                SearchResponse searchResponse = null;
                searchResponse = SearchCatalogItemsByKeyword(searchKeyword, catalogName, searchOptions);

                if (searchResponse != null)
                {
                    returnList.AddRange(searchResponse.ResponseItems);
                    totalProductCount = searchResponse.TotalItemCount;
                    totalPageCount = searchResponse.TotalPageCount;
                    facets = searchResponse.Facets;
                }
            }

            var results = new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
            SiteContextRepository.GetCurrent().Items[CurrentSearchProductResultsKeyName] = results;
            return results;
        }

        public ActionResult ProductSearchResultsPagination(
                    [Bind(Prefix = StorefrontConstants.QueryStrings.SearchKeyword)] string searchKeyword,
                    [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
                    [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
                    [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
                    [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
                    [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var searchInfo = this.GetSearchInfo(searchKeyword, pageNumber, facetValues, sortField, pageSize, sortDirection);
            var viewModel = this.GetPaginationViewModel(searchInfo);
            return this.View(viewModel);
        }

        private PaginationViewModel GetPaginationViewModel(SearchInfo searchInfo)
        {
            var productSearchOptions = searchInfo.SearchOptions;
            var viewModel = new PaginationViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = this.GetChildProducts(productSearchOptions, searchInfo.SearchKeyword, searchInfo.Catalog.Name);
            }

            viewModel.Initialize(RenderingContext.Current.Rendering, childProducts, productSearchOptions);
            return viewModel;
        }

        private void UpdateOptionsWithFacets(IEnumerable<CommerceQueryFacet> facets, string valueQueryString, CommerceSearchOptions productSearchOptions)
        {
            if (facets == null || !facets.Any())
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
                    var existingFacet = facets.FirstOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (existingFacet == null)
                    {
                        continue;
                    }
                    var values = facetValues[1].Split(StorefrontConstants.QueryStrings.FacetsSeparator);

                    foreach (var value in values)
                    {
                        existingFacet.Values.Add(value);
                    }
                }
            }

            productSearchOptions.FacetFields = facets;
        }

        private void UpdateOptionsWithSorting(string sortField, CommerceConstants.SortDirection? sortDirection, CommerceSearchOptions productSearchOptions)
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
            if (SiteContextRepository.GetCurrent().Items[CurrentSearchInfoKeyName] != null)
            {
                return (SearchInfo)SiteContextRepository.GetCurrent().Items[CurrentSearchInfoKeyName];
            }

            var searchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>();
            var searchInfo = new SearchInfo
            {
                SearchKeyword = searchKeyword ?? string.Empty,
                RequiredFacets = searchManager.GetFacetFieldsForItem(RenderingContext.Current.Rendering.Item),
                SortFields = searchManager.GetSortFieldsForItem(RenderingContext.Current.Rendering.Item),
                Catalog = CatalogManager.CurrentCatalog,
                ItemsPerPage = pageSize ?? searchManager.GetItemsPerPageForItem(RenderingContext.Current.Rendering.Item)
            };
            if (searchInfo.ItemsPerPage <= 0)
            {
                searchInfo.ItemsPerPage = 12;
            }

            var productSearchOptions = new CommerceSearchOptions(searchInfo.ItemsPerPage, pageNumber.GetValueOrDefault(0));
            UpdateOptionsWithFacets(searchInfo.RequiredFacets, facetValues, productSearchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, productSearchOptions);
            searchInfo.SearchOptions = productSearchOptions;

            SiteContextRepository.GetCurrent().Items[CurrentSearchInfoKeyName] = searchInfo;
            return searchInfo;
        }

        private SearchResponse SearchCatalogItemsByKeyword(string keyword, string catalogName, CommerceSearchOptions searchOptions)
        {
            Assert.ArgumentNotNullOrEmpty(catalogName, nameof(catalogName));
            var searchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>();
            var searchIndex = searchManager.GetIndex(catalogName);

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceProductSearchResultItem>()
                    .Where(item => item.Name.Equals(keyword) || item["_displayname"].Equals(keyword) || item.Content.Contains(keyword))
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Product || item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.CatalogName == catalogName)
                    .Where(item => item.Language == Context.Language.Name)
                    .Select(p => new CommerceProductSearchResultItem
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

        private class SearchInfo
        {
            public string SearchKeyword { get; set; }

            public IEnumerable<CommerceQueryFacet> RequiredFacets { get; set; }

            public IEnumerable<CommerceQuerySort> SortFields { get; set; }

            public int ItemsPerPage { get; set; }

            public Foundation.Commerce.Models.Catalog Catalog { get; set; }

            public CommerceSearchOptions SearchOptions { get; set; }
        }
    }
}