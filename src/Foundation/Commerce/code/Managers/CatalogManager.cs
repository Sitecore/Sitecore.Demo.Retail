//-----------------------------------------------------------------------
// <copyright file="CatalogManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
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
using System.Text;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Engine.Connect.Entities.Prices;
using Sitecore.Commerce.Entities.Catalog;
using Sitecore.Commerce.Entities.Prices;
using Sitecore.Commerce.Services.Catalog;
using Sitecore.Commerce.Services.Globalization;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.Search;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class CatalogManager : BaseManager
    {
        private Catalog _currentCatalog;

        public CatalogManager([NotNull] CatalogServiceProvider catalogServiceProvider, [NotNull] GlobalizationServiceProvider globalizationServiceProvider, [NotNull] PricingManager pricingManager, [NotNull] InventoryManager inventoryManager, SiteContextRepository siteContextRepository, ICommerceSearchManager commerceSearchManager)
        {
            Assert.ArgumentNotNull(catalogServiceProvider, nameof(catalogServiceProvider));
            Assert.ArgumentNotNull(pricingManager, nameof(pricingManager));
            Assert.ArgumentNotNull(inventoryManager, nameof(inventoryManager));

            CatalogServiceProvider = catalogServiceProvider;
            GlobalizationServiceProvider = globalizationServiceProvider;
            PricingManager = pricingManager;
            InventoryManager = inventoryManager;
            SiteContextRepository = siteContextRepository;
            CommerceSearchManager = commerceSearchManager;
        }

        public ICommerceSearchManager CommerceSearchManager { get; set; }
        public CatalogServiceProvider CatalogServiceProvider { get; protected set; }
        public GlobalizationServiceProvider GlobalizationServiceProvider { get; protected set; }
        public InventoryManager InventoryManager { get; protected set; }
        public SiteContextRepository SiteContextRepository { get; }
        public PricingManager PricingManager { get; protected set; }

        public Catalog CurrentCatalog
        {
            get
            {
                if (_currentCatalog != null)
                {
                    return _currentCatalog;
                }

                _currentCatalog = GetCurrentCatalog();

                return _currentCatalog;
            }
        }

        public Catalog GetCurrentCatalog()
        {
            //If this is not in a storefront then return the default catalog
            if (StorefrontManager.CurrentStorefront?.HomeItem != null)
            {
                var catalogs = ((MultilistField) StorefrontManager.CurrentStorefront?.HomeItem.Fields["Catalogs"]).GetItems();
                if (catalogs.Any())
                {
                    return new Catalog(catalogs.First());
                }
            }

            var defaultCatalogContainer = Context.Database.GetItem(CommerceConstants.KnownItemIds.DefaultCatalog);
            var catalogPath = defaultCatalogContainer.Fields["Catalog"].Source + "/" + defaultCatalogContainer.Fields["Catalog"].Value;
            var defaultCatalog = Context.Database.GetItem(catalogPath);
            return new Catalog(defaultCatalog);
        }


        public CatalogResult VisitedCategoryPage([NotNull] CommerceStorefront storefront, [NotNull] string categoryId, string categoryName)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            var request = new VisitedCategoryPageRequest(storefront.ShopName, categoryId, categoryName);
            return CatalogServiceProvider.VisitedCategoryPage(request);
        }

        public CatalogResult VisitedProductDetailsPage([NotNull] CommerceStorefront storefront, [NotNull] string productId, string productName, string parentCategoryId, string parentCategoryName)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            var request = new VisitedProductDetailsPageRequest(storefront.ShopName, productId, productName, parentCategoryId, parentCategoryName);
            return CatalogServiceProvider.VisitedProductDetailsPage(request);
        }

        public SearchResults GetProductSearchResults(Item dataSource, CommerceSearchOptions productSearchOptions)
        {
            Assert.ArgumentNotNull(productSearchOptions, nameof(productSearchOptions));
            Assert.ArgumentCondition(dataSource.IsDerived(Templates.Commerce.SearchSettings.ID), nameof(dataSource), "Item must derive from the CommerceSearchSettings template");

            if (dataSource == null)
            {
                return null;
            }

            var totalProductCount = 0;
            var totalPageCount = 0;

            var returnList = new List<Item>();
            IEnumerable<CommerceQueryFacet> facets = null;
            var searchOptions = new CommerceSearchOptions(-1, 0);
            var searchResponse = FindCatalogItems(dataSource, searchOptions);
            if (searchResponse != null)
            {
                returnList.AddRange(searchResponse.ResponseItems);

                totalProductCount = searchResponse.TotalItemCount;
                totalPageCount = searchResponse.TotalPageCount;
                facets = searchResponse.Facets;
            }

            return new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
        }

        public Category GetCategory(string categoryId)
        {
            var categoryItem = GetCategory(categoryId, CurrentCatalog.Name);
            return GetCategory(categoryItem);
        }

        public Category GetCategory(Item item)
        {
            var category = new Category(item)
            {
                RequiredFacets = CommerceSearchManager.GetFacetFieldsForItem(item).ToList(),
                SortFields = CommerceSearchManager.GetSortFieldsForItem(item).ToList(),
                ItemsPerPage = CommerceSearchManager.GetItemsPerPageForItem(item)
            };

            return category;
        }

        public void GetProductPrice([NotNull] VisitorContext visitorContext, ICatalogProduct productViewModel)
        {
            if (productViewModel == null)
            {
                return;
            }

            var includeVariants = productViewModel.Variants != null && productViewModel.Variants.Any();
            var pricesResponse = PricingManager.GetProductPrices(StorefrontManager.CurrentStorefront, visitorContext, productViewModel.CatalogName, productViewModel.ProductId, includeVariants, null);
            if (pricesResponse == null || !pricesResponse.ServiceProviderResult.Success || pricesResponse.Result == null)
            {
                return;
            }

            Price price;
            if (pricesResponse.Result.TryGetValue(productViewModel.ProductId, out price))
            {
                var extendedPrice = (ExtendedCommercePrice) price;
                productViewModel.ListPrice = price.Amount;
                productViewModel.AdjustedPrice = extendedPrice.ListPrice;
            }

            if (!includeVariants)
            {
                return;
            }

            foreach (var variant in productViewModel.Variants)
            {
                if (!pricesResponse.Result.TryGetValue(variant.VariantId, out price))
                {
                    continue;
                }

                var extendedPrice = (ExtendedCommercePrice) price;
                variant.ListPrice = extendedPrice.Amount;
                variant.AdjustedPrice = extendedPrice.ListPrice;
            }
        }

        public void GetProductBulkPrices([NotNull] VisitorContext visitorContext, IEnumerable<ICatalogProduct> catalogProducts)
        {
            var catalogProductsArray = catalogProducts as ICatalogProduct[] ?? catalogProducts?.ToArray();
            if (catalogProductsArray == null || !catalogProductsArray.Any())
            {
                return;
            }

            var catalogName = catalogProductsArray.Select(p => p.CatalogName).First();
            var productIds = catalogProductsArray.Select(p => p.ProductId).ToList();

            var pricesResponse = PricingManager.GetProductBulkPrices(StorefrontManager.CurrentStorefront, visitorContext, catalogName, productIds, null);
            var prices = pricesResponse?.Result ?? new Dictionary<string, Price>();

            foreach (var product in catalogProductsArray)
            {
                Price price;
                if (!prices.Any() || !prices.TryGetValue(product.ProductId, out price))
                {
                    continue;
                }

                var extendedPrice = (ExtendedCommercePrice) price;

                product.ListPrice = extendedPrice.Amount;
                product.AdjustedPrice = extendedPrice.ListPrice;

                product.LowestPricedVariantAdjustedPrice = extendedPrice.LowestPricedVariant;
                product.LowestPricedVariantListPrice = extendedPrice.LowestPricedVariantListPrice;
                product.HighestPricedVariantAdjustedPrice = extendedPrice.HighestPricedVariant;
            }
        }

        public decimal GetProductRating(Item productItem)
        {
            var ratingString = productItem[Templates.HasRating.Fields.Rating];
            decimal rating;
            return decimal.TryParse(ratingString, out rating) ? rating : 0;
        }

        public ManagerResponse<CatalogResult, bool> FacetApplied([NotNull] CommerceStorefront storefront,
            string facet, bool isApplied)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            var request = new FacetAppliedRequest(storefront.ShopName, facet, isApplied);
            var result = CatalogServiceProvider.FacetApplied(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CatalogResult, bool>(result, result.Success);
        }

        public ManagerResponse<CatalogResult, bool> SortOrderApplied([NotNull] CommerceStorefront storefront,
            string sortKey, CommerceConstants.SortDirection? sortDirection)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            var connectSortDirection = SortDirection.Ascending;
            if (sortDirection.HasValue)
            {
                switch (sortDirection.Value)
                {
                    case CommerceConstants.SortDirection.Asc:
                        connectSortDirection = SortDirection.Ascending;
                        break;

                    default:
                        connectSortDirection = SortDirection.Descending;
                        break;
                }
            }

            var request = new ProductSortingRequest(storefront.ShopName, sortKey, connectSortDirection);
            var result = CatalogServiceProvider.ProductSorting(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CatalogResult, bool>(result, result.Success);
        }

        public ManagerResponse<CatalogResult, bool> RegisterSearchEvent([NotNull] CommerceStorefront storefront, string searchKeyword, int numberOfHits)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNullOrEmpty(searchKeyword, nameof(searchKeyword));

            var request = new SearchInitiatedRequest(storefront.ShopName, searchKeyword, numberOfHits);
            var result = CatalogServiceProvider.SearchInitiated(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CatalogResult, bool>(result, result.Success);
        }

        public ManagerResponse<GlobalizationResult, bool> RaiseCultureChosenPageEvent([NotNull] CommerceStorefront storefront, string culture)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNullOrEmpty(culture, nameof(culture));

            var result = GlobalizationServiceProvider.CultureChosen(new CultureChosenRequest(storefront.ShopName, culture));

            return new ManagerResponse<GlobalizationResult, bool>(result, result.Success);
        }

        public SearchResponse FindCatalogItems(Item queryItem, CommerceSearchOptions searchOptions)
        {
            Assert.ArgumentNotNull(queryItem, nameof(queryItem));
            Assert.ArgumentNotNull(searchOptions, nameof(searchOptions));

            var defaultBucketQuery = queryItem[Templates.Commerce.SearchSettings.Fields.DefaultBucketQuery];
            var persistentBucketFilter = CleanLanguageFromFilter(queryItem[Templates.Commerce.SearchSettings.Fields.PersistentBucketFilter]);

            var searchIndex = CommerceSearchManager.GetIndex();

            var defaultQuery = defaultBucketQuery.Replace("&", ";");
            var persistentQuery = persistentBucketFilter.Replace("&", ";");
            var combinedQuery = CombineQueries(persistentQuery, defaultQuery);

            try
            {
                var searchStringModel = SearchStringModel.ParseDatasourceString(combinedQuery);
                using (var context = searchIndex.CreateSearchContext(SearchSecurityOptions.EnableSecurityCheck))
                {
                    var query = LinqHelper.CreateQuery<SitecoreUISearchResultItem>(context, searchStringModel).Where(item => item.Language == Context.Language.Name);

                    query = CommerceSearchManager.AddSearchOptionsToQuery(query, searchOptions);

                    var results = query.GetResults();
                    var response = SearchResponse.CreateFromUISearchResultsItems(searchOptions, results);

                    return response;
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not find catalog items. Bucket query failed", e, this);
                return null;
            }
        }

        private static string CombineQueries(string query1, string query2)
        {
            if (!string.IsNullOrWhiteSpace(query1) && !string.IsNullOrWhiteSpace(query2))
            {
                return string.Concat(query1, ";", query2);
            }
            if (!string.IsNullOrWhiteSpace(query1))
            {
                return query1;
            }
            return query2;
        }


        public SearchResults GetChildProducts(CommerceSearchOptions searchOptions, Item categoryItem)
        {
            IEnumerable<CommerceQueryFacet> facets = null;
            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;

            var searchResponse = GetCategoryProducts(categoryItem, searchOptions);

            if (searchResponse != null)
            {
                returnList.AddRange(searchResponse.ResponseItems);

                totalProductCount = searchResponse.TotalItemCount;
                totalPageCount = searchResponse.TotalPageCount;
                facets = searchResponse.Facets;
            }

            var results = new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
            return results;
        }

        public SearchResponse GetCategoryProducts(Item categoryItem, CommerceSearchOptions searchOptions)
        {
            if (categoryItem.IsDerived(Templates.Commerce.DynamicCategory.ID))
            {
                return FindCatalogItems(categoryItem, searchOptions);
            }

            var searchIndex = CommerceSearchManager.GetIndex();

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceProductSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Product)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(item => item.CommerceAncestorIds.Contains(categoryItem.ID))
                    .Select(p => new CommerceProductSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    });

                searchResults = CommerceSearchManager.AddSearchOptionsToQuery(searchResults, searchOptions);

                var results = searchResults.GetResults();
                var response = SearchResponse.CreateFromSearchResultsItems(searchOptions, results);

                return response;
            }
        }


        protected string CleanLanguageFromFilter(string filter)
        {
            if (filter.IndexOf("language:", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return filter;
            }

            var newFilter = new StringBuilder();

            var statementList = filter.Split(';');
            foreach (var statement in statementList)
            {
                if (statement.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (newFilter.Length > 0)
                {
                    newFilter.Append(';');
                }

                newFilter.Append(statement);
            }

            return newFilter.ToString();
        }

        public Item GetCategory(string categoryName, string catalogName)
        {
            Assert.ArgumentNotNullOrEmpty(catalogName, nameof(catalogName));

            Item result = null;
            var searchIndex = CommerceSearchManager.GetIndex(catalogName);

            using (var context = searchIndex.CreateSearchContext())
            {
                var categoryQuery = context.GetQueryable<CommerceBaseCatalogSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(item => item.Name == categoryName && item.CatalogName == catalogName || item.Name == categoryName)
                    .Select(p => new CommerceBaseCatalogSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    })
                    .Take(1);

                var foundSearchItem = categoryQuery.FirstOrDefault();
                if (foundSearchItem != null)
                {
                    result = foundSearchItem.GetItem();
                }
            }

            return result;
        }

        public Item GetProduct(string productId, string catalogName)
        {
            Assert.ArgumentNotNullOrEmpty(catalogName, nameof(catalogName));

            Item result = null;
            var searchIndex = CommerceSearchManager.GetIndex(catalogName);

            using (var context = searchIndex.CreateSearchContext())
            {
                var productQuery = context.GetQueryable<CommerceProductSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Product)
                    .Where(item => item.CatalogName == catalogName)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(item => item.CatalogItemId == productId.ToLowerInvariant())
                    .Select(p => new CommerceProductSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    })
                    .Take(1);

                var foundSearchItem = productQuery.FirstOrDefault();
                if (foundSearchItem != null)
                {
                    result = foundSearchItem.GetItem();
                }
            }

            return result;
        }

        public CategorySearchResults GetCategoryChildCategories(ID categoryId, CommerceSearchOptions searchOptions)
        {
            var childCategoryList = new List<Item>();

            var searchIndex = CommerceSearchManager.GetIndex();

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceBaseCatalogSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(item => item.ItemId == categoryId)
                    .Select(p => p);

                var list = searchResults.ToList();
                if (!list.Any())
                {
                    return new CategorySearchResults(childCategoryList, childCategoryList.Count, 1, 1, new List<FacetCategory>());
                }
                if (!list[0].Fields.ContainsKey(Constants.CommerceIndex.Fields.ChildCategoriesSequence))
                {
                    return new CategorySearchResults(childCategoryList, childCategoryList.Count, 1, 1, new List<FacetCategory>());
                }

                var childCategoryDelimitedString = list[0][Constants.CommerceIndex.Fields.ChildCategoriesSequence];

                var categoryIdArray = childCategoryDelimitedString.Split('|');

                var categoryItems = categoryIdArray.Select(childCategoryId => Context.Database.GetItem(ID.Parse(childCategoryId))).Where(childCategoryItem => childCategoryItem != null);
                childCategoryList.AddRange(categoryItems);
            }

            return new CategorySearchResults(childCategoryList, childCategoryList.Count, 1, 1, new List<FacetCategory>());
        }
    }
}