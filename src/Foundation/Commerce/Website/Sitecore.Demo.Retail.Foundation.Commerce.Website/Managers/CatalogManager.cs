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
using System.Web;
using System.Web.Helpers;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Engine.Connect.Entities.Prices;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Commerce.Entities.Prices;
using Sitecore.Commerce.Services.Catalog;
using Sitecore.Commerce.Services.Globalization;
using Sitecore.Commerce.Services.Inventory;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.Search;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
    public class CatalogManager : IManager
    {
        public CatalogManager(CatalogServiceProvider catalogServiceProvider, GlobalizationServiceProvider globalizationServiceProvider, PricingManager pricingManager, InventoryManager inventoryManager, CatalogItemContext catalogItemContext, ICommerceSearchManager commerceSearchManager, StorefrontContext storefrontContext)
        {
            CatalogServiceProvider = catalogServiceProvider;
            GlobalizationServiceProvider = globalizationServiceProvider;
            PricingManager = pricingManager;
            InventoryManager = inventoryManager;
            CatalogItemContext = catalogItemContext;
            CommerceSearchManager = commerceSearchManager;
            StorefrontContext = storefrontContext;
        }

        public ICommerceSearchManager CommerceSearchManager { get; set; }
        public StorefrontContext StorefrontContext { get; }
        public CatalogServiceProvider CatalogServiceProvider { get; protected set; }
        public GlobalizationServiceProvider GlobalizationServiceProvider { get; protected set; }
        public InventoryManager InventoryManager { get; protected set; }
        public CatalogItemContext CatalogItemContext { get; }
        public PricingManager PricingManager { get; protected set; }

        [CanBeNull]
        public CatalogContext CatalogContext
        {
            get
            {
                var context = (CatalogContext) HttpContext.Current.Items["_catalogContext"];
                if (context != null)
                {
                    return context;
                }
                if (StorefrontContext.Current == null)
                {
                    return null;
                }
                context = CatalogContext.CreateFromContext();
                HttpContext.Current.Items["_catalogContext"] = context;
                return context;
            }
        }

        public CatalogResult VisitedCategoryPage(string categoryId, string categoryName)
        {
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new VisitedCategoryPageRequest(StorefrontContext.Current.ShopName, categoryId, categoryName);
            return CatalogServiceProvider.VisitedCategoryPage(request);
        }

        public CatalogResult VisitedProductDetailsPage(string productId, string productName, string parentCategoryId, string parentCategoryName)
        {
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new VisitedProductDetailsPageRequest(StorefrontContext.Current.ShopName, productId, productName, parentCategoryId, parentCategoryName);
            return CatalogServiceProvider.VisitedProductDetailsPage(request);
        }

        public SearchResults GetProductSearchResults(Item dataSource, SearchOptions productSearchOptions)
        {
            Assert.ArgumentNotNull(productSearchOptions, nameof(productSearchOptions));
            Assert.ArgumentCondition(dataSource.IsDerived(Templates.Commerce.SearchSettings.Id), nameof(dataSource), "Item must derive from the CommerceSearchSettings template");

            if (dataSource == null)
            {
                return null;
            }

            var totalProductCount = 0;
            var totalPageCount = 0;

            var returnList = new List<Item>();
            List<QueryFacet> facets = null;
            var searchOptions = new SearchOptions(-1, 0);
            var searchResponse = FindCatalogItems(dataSource, searchOptions);
            if (searchResponse != null)
            {
                returnList.AddRange(searchResponse.ResponseItems);

                totalProductCount = searchResponse.TotalItemCount;
                totalPageCount = searchResponse.TotalPageCount;
                facets = searchResponse.Facets.Select(f => f.ToQueryFacet()).ToList();
            }

            return new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
        }

        public Category GetCategory(string categoryId)
        {
            return GetCategory(categoryId, CatalogContext.CurrentCatalog.Name);
        }

        public Category GetCategory(string categoryId, string catalog)
        {
            var categoryItem = GetCategoryItem(categoryId, catalog);
            return GetCategory(categoryItem);
        }

        public Category GetCategory(Item item)
        {
            var category = new Category(item)
            {
                RequiredFacets = CommerceSearchManager.GetFacetFieldsForItem(item).Select(f => f.ToQueryFacet()).ToList(),
                SortFields = CommerceSearchManager.GetSortFieldsForItem(item).Select(f => f.ToQuerySortField()).ToList(),
                ItemsPerPage = CommerceSearchManager.GetItemsPerPageForItem(item)
            };

            return category;
        }

        public void GetProductPrice(ICatalogProduct productViewModel)
        {
            if (productViewModel == null)
            {
                return;
            }

            var includeVariants = productViewModel.Variants != null && productViewModel.Variants.Any();
            var pricesResponse = PricingManager.GetProductPrices(productViewModel.CatalogName, productViewModel.ProductId, includeVariants, null);
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

        public void GetProductBulkPrices(IEnumerable<ICatalogProduct> catalogProducts)
        {
            var catalogProductsArray = catalogProducts as ICatalogProduct[] ?? catalogProducts?.ToArray();
            if (catalogProductsArray == null || !catalogProductsArray.Any())
            {
                return;
            }

            var catalogName = catalogProductsArray.Select(p => p.CatalogName).First();
            var productIds = catalogProductsArray.Select(p => p.ProductId).ToList();

            var pricesResponse = PricingManager.GetProductBulkPrices(catalogName, productIds, null);
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
            var ratingString = productItem[Templates.Commerce.Product.FieldNames.Rating];
            decimal rating;
            return decimal.TryParse(ratingString, out rating) ? rating : 0;
        }

        public ManagerResponse<CatalogResult, bool> FacetApplied(string facet, bool isApplied)
        {
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new FacetAppliedRequest(StorefrontContext.Current.ShopName, facet, isApplied);
            var result = CatalogServiceProvider.FacetApplied(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CatalogResult, bool>(result, result.Success);
        }

        public ManagerResponse<CatalogResult, bool> SortOrderApplied(string sortKey, SortDirection? sortDirection)
        {
            var connectSortDirection = SortDirection.Ascending;
            if (sortDirection.HasValue)
            {
                switch (sortDirection.Value)
                {
                    case SortDirection.Ascending:
                        connectSortDirection = SortDirection.Ascending;
                        break;
                    case SortDirection.Descending:
                        connectSortDirection = SortDirection.Descending;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new ProductSortingRequest(StorefrontContext.Current.ShopName, sortKey, connectSortDirection == SortDirection.Ascending ? Sitecore.Commerce.Entities.Catalog.SortDirection.Ascending : Sitecore.Commerce.Entities.Catalog.SortDirection.Descending);
            var result = CatalogServiceProvider.ProductSorting(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CatalogResult, bool>(result, result.Success);
        }

        public ManagerResponse<CatalogResult, bool> RegisterSearchEvent(string searchKeyword, int numberOfHits)
        {
            Assert.ArgumentNotNullOrEmpty(searchKeyword, nameof(searchKeyword));

            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new SearchInitiatedRequest(StorefrontContext.Current.ShopName, searchKeyword, numberOfHits);
            var result = CatalogServiceProvider.SearchInitiated(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CatalogResult, bool>(result, result.Success);
        }

        public ManagerResponse<GlobalizationResult, bool> RaiseCultureChosenPageEvent(string culture)
        {
            Assert.ArgumentNotNullOrEmpty(culture, nameof(culture));

            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var result = GlobalizationServiceProvider.CultureChosen(new CultureChosenRequest(StorefrontContext.Current.ShopName, culture));

            return new ManagerResponse<GlobalizationResult, bool>(result, result.Success);
        }

        public SearchResponse FindCatalogItems(Item queryItem, SearchOptions searchOptions)
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

                    var commerceSearchOptions = searchOptions.ToCommerceSearchOptions();
                    query = CommerceSearchManager.AddSearchOptionsToQuery(query, commerceSearchOptions);

                    var results = query.GetResults();
                    var response = SearchResponse.CreateFromUISearchResultsItems(commerceSearchOptions, results);
                    searchOptions = commerceSearchOptions.ToSearchOptions();

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


        public SearchResults GetChildProducts(Item categoryItem, SearchOptions searchOptions)
        {
            List<QueryFacet> facets = null;
            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;

            var searchResponse = GetCategoryProducts(categoryItem, searchOptions);

            if (searchResponse != null)
            {
                returnList.AddRange(searchResponse.ResponseItems);

                totalProductCount = searchResponse.TotalItemCount;
                totalPageCount = searchResponse.TotalPageCount;
                facets = searchResponse.Facets.Select(f => f.ToQueryFacet()).ToList();
            }

            var results = new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
            return results;
        }

        public SearchResponse GetCategoryProducts(Item categoryItem, SearchOptions searchOptions)
        {
            if (categoryItem.IsDerived(Templates.Commerce.DynamicCategory.Id))
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

                var commerceSearchOptions = searchOptions.ToCommerceSearchOptions();
                searchResults = CommerceSearchManager.AddSearchOptionsToQuery(searchResults, commerceSearchOptions);

                var results = searchResults.GetResults();
                var response = SearchResponse.CreateFromSearchResultsItems(commerceSearchOptions, results);

                searchOptions = commerceSearchOptions.ToSearchOptions();

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

        public Item GetCategoryItem(string categoryName, string catalogName)
        {
            Assert.ArgumentNotNullOrEmpty(catalogName, nameof(catalogName));
            Assert.ArgumentNotNullOrEmpty(categoryName, nameof(categoryName));

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

        public Item GetProduct(string productId)
        {
            return GetProduct(productId, CatalogContext.CurrentCatalog.Name);
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

        public CategorySearchResults GetCategoryChildCategories(Item category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            var childCategoryList = new List<Item>();

            var searchIndex = CommerceSearchManager.GetIndex();

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceBaseCatalogSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(item => item.ItemId == category.ID)
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

        public ManagerResponse<GetStockInformationResult, IEnumerable<StockInformation>> GetProductInventory(string productId)
        {
            var currentProductItem = GetProduct(productId);
            if (currentProductItem == null)
            {
                return null;
            }

            var catalogName = currentProductItem[Templates.Commerce.CatalogItem.Fields.CatalogName];
            var products = new List<CommerceInventoryProduct>();
            if (currentProductItem.HasChildren)
            {
                foreach (Item item in currentProductItem.Children)
                {
                    products.Add(new CommerceInventoryProduct
                    {
                        ProductId = productId,
                        CatalogName = catalogName,
                        VariantId = item.Name
                    });
                }
            }
            else
            {
                products.Add(new CommerceInventoryProduct {ProductId = productId, CatalogName = catalogName});
            }

            var response = InventoryManager.GetStockInformation(products, StockDetailsLevel.All);
            return response;
        }
    }
}