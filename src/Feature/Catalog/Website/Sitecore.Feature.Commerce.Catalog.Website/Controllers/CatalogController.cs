//-----------------------------------------------------------------------
// <copyright file="CatalogController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CheckoutController class.</summary>
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
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer.Catalog.Fields;
using Sitecore.Commerce.Contacts;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Website.Factories;
using Sitecore.Feature.Commerce.Catalog.Website.Models;
using Sitecore.Feature.Commerce.Catalog.Website.Services;
using Sitecore.Foundation.Alerts;
using Sitecore.Foundation.Alerts.Extensions;
using Sitecore.Foundation.Alerts.Models;
using Sitecore.Foundation.Commerce.Website;
using Sitecore.Foundation.Commerce.Website.Extensions;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Foundation.Commerce.Website.Models.Search;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Foundation.SitecoreExtensions.Attributes;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Website.Controllers
{
    public class CatalogController : SitecoreController
    {
        private const string CookieName = "_lastVisitedCategory";
        private const string CustomerIdKey = "CustomerId";
        private const string CategoryIdKey = "CategoryId";

        public CatalogController(InventoryManager inventoryManager, ContactFactory contactFactory, ProductViewModelFactory productViewModelFactory, AccountManager accountManager, CatalogManager catalogManager, GiftCardManager giftCardManager, PricingManager pricingManager, CartManager cartManager, CommerceUserContext commerceUserContext, CatalogItemContext catalogItemContext, CatalogUrlService catalogUrlRepository, StorefrontContext storefrontContext, CategoryViewModelFactory categoryViewModelFactory, GetChildProductsService getChildProductsService)
        {
            InventoryManager = inventoryManager;
            ProductViewModelFactory = productViewModelFactory;
            CatalogManager = catalogManager;
            GiftCardManager = giftCardManager;
            CommerceUserContext = commerceUserContext;
            CatalogItemContext = catalogItemContext;
            StorefrontContext = storefrontContext;
            CategoryViewModelFactory = categoryViewModelFactory;
            GetChildProductsService = getChildProductsService;
        }

        private CommerceUserContext CommerceUserContext { get; }
        private CatalogItemContext CatalogItemContext { get; }
        private StorefrontContext StorefrontContext { get; }
        private InventoryManager InventoryManager { get; }
        private ProductViewModelFactory ProductViewModelFactory { get; }
        private CatalogManager CatalogManager { get; }
        private GiftCardManager GiftCardManager { get; }

        public ActionResult RelatedProducts([Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Sort)] string sortField)
        {
            if (StorefrontContext.Current == null || CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var viewModel = GetProductSearchViewModel(RenderingContext.Current.Rendering.Item, sortField);
            if (viewModel == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            return View(viewModel);
        }

        private MultipleSearchResultsViewModel GetProductSearchViewModel(Item item, string sortField)
        {
            var multipleProductSearchResults = GetMultipleProductSearchResults(item, sortField);
            if (multipleProductSearchResults == null)
            {
                return null;
            }

            var viewModel = new MultipleSearchResultsViewModel(multipleProductSearchResults);

            var products = viewModel.ProductSearchResults.SelectMany(productSearchResult => productSearchResult.Items.Where(i => i is ProductViewModel).Cast<ProductViewModel>()).ToList();
            CatalogManager.GetProductBulkPrices(products);
            CatalogManager.InventoryManager.GetProductsStockStatusForList(products);

            foreach (var productViewModel in products)
            {
                productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productViewModel.Item);
            }
            return viewModel;
        }

        public MultipleProductSearchResults GetMultipleProductSearchResults(Item dataSource, string sortField)
        {
            if (!dataSource.IsDerived(Templates.HasNamedSearches.ID))
            {
                return null;
            }
            MultilistField searchesField = dataSource.Fields[Templates.HasNamedSearches.Fields.NamedSearches];
            var searches = searchesField.GetItems();
            var productsSearchResults = new List<SearchResults>();
            foreach (var search in searches)
            {
                if (search.IsDerived(Templates.NamedSearch.ID))
                {
                    var searchOptions = new SearchOptions
                    {
                        NumberOfItemsToReturn = 12,
                        StartPageIndex = 0,
                        SortField = sortField
                    };

                    var productsSearchResult = CatalogManager.GetProductSearchResults(search, searchOptions);
                    if (productsSearchResult == null)
                    {
                        continue;
                    }
                    productsSearchResult.Title = search[Templates.NamedSearch.Fields.Title];
                    productsSearchResults.Add(productsSearchResult);
                }
                else if (search.IsDerived(Templates.SelectedProducts.ID))
                {
                    var staticSearchList = new SearchResults
                    {
                        Title = search[Templates.SelectedProducts.Fields.Title]
                    };

                    MultilistField productListField = search.Fields[Templates.SelectedProducts.Fields.ProductList];
                    var productList = productListField.GetItems();
                    foreach (var productItem in productList)
                    {
                        if (!productItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Category.Id) && !productItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Category.Id))
                        {
                            continue;
                        }
                        staticSearchList.SearchResultItems.Add(productItem);
                    }

                    staticSearchList.TotalItemCount = staticSearchList.SearchResultItems.Count;
                    staticSearchList.TotalPageCount = staticSearchList.SearchResultItems.Count;
                    productsSearchResults.Add(staticSearchList);
                }
            }

            return new MultipleProductSearchResults(productsSearchResults);
        }


        public ActionResult ProductRecommendation([Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Sort)] string sortField)
        {
            if (StorefrontContext.Current == null || CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var viewModel = GetProductSearchViewModel(RenderingContext.Current.Rendering.Item, sortField);
            if (viewModel == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            return View(viewModel);
        }

        public ActionResult ProductList([Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var searchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize, sortField, sortDirection);

            var viewModel = GetCategoryViewModel(currentCategory, searchOptions);

            return View(viewModel);
        }

        private SearchOptions GetCategorySearchOptions(Category currentCategory, int? pageNumber, string facetValues, int? pageSize, string sortField = null, SortDirection? sortDirection = null)
        {
            var searchOptions = new SearchOptions(pageSize ?? currentCategory.ItemsPerPage, pageNumber ?? 0);
            UpdateOptionsWithFacets(currentCategory.RequiredFacets, facetValues, searchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, searchOptions);
            return searchOptions;
        }

        public ActionResult Navigation()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var item = RenderingContext.Current.Rendering.Item;
            var dataSource = item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.NavigationItem.Id) ? item?.TargetItem(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.NavigationItem.Fields.CategoryDatasource) : null;
            if (dataSource == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var currentCategory = CatalogManager.GetCategory(dataSource);
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var viewModel = GetNavigationViewModel(currentCategory);

            return View(viewModel);
        }

        public ActionResult ChildCategoryNavigation()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            Assert.IsNotNull(CatalogItemContext.Current, "The Current Catalog Item must be set");
            Assert.IsTrue(CatalogItemContext.IsCategory, "Current item must be a Category.");

            var category = CatalogManager.GetCategory(CatalogItemContext.Current.Item);
            var viewModel = GetNavigationViewModel(category);
            if (viewModel.ChildCategories.Count == 0)
            {
                category = CatalogManager.GetCategory(CatalogItemContext.Current.Item.Parent);
                viewModel = GetNavigationViewModel(category);
            }

            return View(viewModel);
        }

        public ActionResult ProductListHeader(
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var productSearchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize, sortField, sortDirection);

            var viewModel = GetProductListHeaderViewModel(productSearchOptions, currentCategory.SortFields, currentCategory, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult Pagination(
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets)] string facetValues)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var productSearchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize);
            var viewModel = GetPaginationViewModel(productSearchOptions, currentCategory, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult ProductFacets(
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = global::Sitecore.Feature.Commerce.Catalog.Website.Constants.QueryString.SortDirection)] SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var searchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize, sortField, sortDirection);

            var viewModel = GetProductFacetsViewModel(searchOptions, currentCategory, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult CategoryPageHeader()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var model = CategoryViewModelFactory.Create(currentCategory);
            return View(model);
        }

        [HttpPost]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult FacetApplied(string facetValue, bool? isApplied)
        {
            if (!string.IsNullOrWhiteSpace(facetValue) && isApplied.HasValue && StorefrontContext.Current != null)
            {
                CatalogManager.FacetApplied(facetValue, isApplied.Value);
            }

            return new BaseApiModel();
        }

        [HttpPost]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult SortOrderApplied(string sortField, SortDirection? sortDirection)
        {
            if (!string.IsNullOrWhiteSpace(sortField) && StorefrontContext.Current != null)
            {
                CatalogManager.SortOrderApplied(sortField, sortDirection);
            }

            return new BaseApiModel();
        }

        public ActionResult AddToCart()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context.")) : new EmptyResult();
            }
            if (!ProductViewModelFactory.IsValid(RenderingContext.Current.Rendering.Item))
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("Invalid datasource. Please pick a product.")) : new EmptyResult();
            }

            var model = ProductViewModelFactory.Create(RenderingContext.Current.Rendering.Item);
            return View(model);
        }

        public ActionResult ProductImages()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context.")) : new EmptyResult();
            }
            if (!ProductViewModelFactory.IsValid(RenderingContext.Current.Rendering.Item))
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("Invalid datasource. Please pick a product.")) : new EmptyResult();
            }

            var model = ProductViewModelFactory.Create(RenderingContext.Current.Rendering.Item);
            if (model.Images == null || !model.Images.Any())
            {
                return new EmptyResult();
            }

            return View(model);
        }

        public ActionResult ProductInformation()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context.")) : new EmptyResult();
            }
            if (!ProductViewModelFactory.IsValid(RenderingContext.Current.Rendering.Item))
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("Invalid datasource. Please pick a product.")) : new EmptyResult();
            }

            var model = ProductViewModelFactory.Create(RenderingContext.Current.Rendering.Item);
            return View(model);
        }

        public ActionResult ProductTeaser()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context.")) : new EmptyResult();
            }
            if (!ProductViewModelFactory.IsValid(RenderingContext.Current.Rendering.Item))
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("Invalid datasource. Please pick a product.")) : new EmptyResult();
            }

            var model = ProductViewModelFactory.Create(RenderingContext.Current.Rendering.Item);
            return View(model);
        }

        public ActionResult ProductRating()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context.")) : new EmptyResult();
            }
            if (!ProductViewModelFactory.IsValid(RenderingContext.Current.Rendering.Item))
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("Invalid datasource. Please pick a product.")) : new EmptyResult();
            }

            var model = ProductViewModelFactory.Create(RenderingContext.Current.Rendering.Item);
            return View(model);
        }

        public ActionResult VisitedProductDetailsPage()
        {
            if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context.")) : new EmptyResult();
            }
            if (!ProductViewModelFactory.IsValid(RenderingContext.Current.Rendering.Item))
            {
                return Context.PageMode.IsExperienceEditor ? (ActionResult) this.InfoMessage(InfoMessage.Error("Invalid datasource. Please pick a product.")) : new EmptyResult();
            }

            var model = ProductViewModelFactory.Create(RenderingContext.Current.Rendering.Item);

            if (model != null)
            {
                CatalogManager.VisitedProductDetailsPage(model.ProductId, model.ProductName, model.ParentCategoryId, model.ParentCategoryName);
            }

            return new EmptyResult();
        }

        public ActionResult VisitedCategoryPage()
        {
            if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var lastCategoryId = GetLastVisitedCategory(CommerceUserContext.Current.UserId);

            var currentCategory = GetCurrentCategory();
            if (currentCategory == null)
            {
                return new EmptyResult();
            }

            if (!string.IsNullOrWhiteSpace(lastCategoryId) && lastCategoryId.Equals(currentCategory.Name))
            {
                return new EmptyResult();
            }

            CatalogManager.VisitedCategoryPage(currentCategory.Name, currentCategory.Title);
            SetLastVisitedCategory(CommerceUserContext.Current.UserId, currentCategory.Name);

            return new EmptyResult();
        }

        public ActionResult RelatedCatalogItems()
        {
            if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var isCatalogItem = RenderingContext.Current.Rendering.Item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Id);

            var catalogItem = isCatalogItem ? RenderingContext.Current.Rendering.Item : CatalogItemContext.Current?.Item;
            var relatedCatalogItemsModel = GetRelationshipsFromItem(catalogItem, RenderingContext.Current.Rendering);
            return View(relatedCatalogItemsModel);
        }

        [HttpGet]
        public ActionResult CheckGiftCardBalance()
        {
            if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            return View();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult GetCurrentProductStockInfo(ProductStockInfoInputModel model)
        {
            try
            {
                if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
                {
                    throw new InvalidOperationException("Cannot be called without a valid catalog context.");
                }

                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var productId = model.ProductId;
                var response = CatalogManager.GetProductInventory(productId);
                var result = new StockInfoListApiModel(response.ServiceProviderResult);
                if (response.Result == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(response.Result);
                var stockInfo = response.Result.FirstOrDefault();
                if (stockInfo != null)
                {
                    InventoryManager.VisitedProductStockStatus(stockInfo, string.Empty);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetCurrentProductStockInfo", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult CheckGiftCardBalance(GetGiftCardBalanceInputModel inputModel)
        {
            try
            {
                if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
                {
                    throw new InvalidOperationException("Cannot be called without a valid catalog context.");
                }

                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = GiftCardManager.GetGiftCardBalance(inputModel.GiftCardId);
                var result = new GiftCardApiModel(response.ServiceProviderResult);
                if (!response.ServiceProviderResult.Success || response.ServiceProviderResult.GiftCard == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var giftCard = response.ServiceProviderResult.GiftCard;
                result.Initialize(giftCard);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("CheckGiftCardBalance", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        [SkipAnalyticsTracking]
        public JsonResult SignUpForBackInStockNotification(SignUpForNotificationInputModel model)
        {
            try
            {
                if (CatalogManager.CatalogContext == null || StorefrontContext.Current == null)
                {
                    throw new InvalidOperationException("Cannot be called without a valid catalog context.");
                }

                Assert.ArgumentNotNull(model, nameof(model));

                var result = this.CreateJsonResult();
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var response = InventoryManager.VisitorSignupForStockNotification(model, string.Empty);
                result = new BaseApiModel(response.ServiceProviderResult);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("GetCurrentUser", e), JsonRequestBehavior.AllowGet);
            }
        }

        public ProductListHeaderViewModel GetProductListHeaderViewModel(SearchOptions productSearchOptions, IEnumerable<QuerySortField> sortFields, Category category, Rendering rendering)
        {
            var viewModel = new ProductListHeaderViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProductsService.GetChildProducts(category, productSearchOptions);
            }

            viewModel.Initialize(rendering, childProducts, sortFields, productSearchOptions);

            return viewModel;
        }

        public PaginationViewModel GetPaginationViewModel(SearchOptions productSearchOptions, Category category, Rendering rendering)
        {
            var viewModel = new PaginationViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProductsService.GetChildProducts(category, productSearchOptions);
            }

            viewModel.Initialize(rendering, childProducts, productSearchOptions);

            return viewModel;
        }

        public ProductFacetsViewModel GetProductFacetsViewModel(SearchOptions productSearchOptions, Category category, Rendering rendering)
        {
            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProductsService.GetChildProducts(category, productSearchOptions);
            }

            var viewModel = new ProductFacetsViewModel(childProducts?.Facets ?? productSearchOptions?.FacetFields);

            return viewModel;
        }

        public GetChildProductsService GetChildProductsService { get; }

        private IEnumerable<Item> GetChildCategories(Category category)
        {
            return RenderingContext.Current.Rendering.Item != null ? CatalogManager.GetCategoryChildCategories(category.InnerItem).CategoryItems : Enumerable.Empty<Item>();
        }

        private CategoryViewModel GetCategoryViewModel(Category category, SearchOptions productSearchOptions = null)
        {
            Assert.IsNotNull(category, nameof(category));

            var cacheKey = CreateCacheKey("Category", category, productSearchOptions);

            var categoryViewModel = this.GetFromCache<CategoryViewModel>(cacheKey);
            if (categoryViewModel != null)
            {
                return categoryViewModel;
            }
            categoryViewModel = CategoryViewModelFactory.Create(category, productSearchOptions);

            return this.AddToCache(cacheKey, categoryViewModel);
        }

        public CategoryViewModelFactory CategoryViewModelFactory { get; }

        private static string CreateCacheKey(string cacheId, Category category, SearchOptions productSearchOptions)
        {
            return $"{cacheId}/{category.InnerItem.ID}/{productSearchOptions}";
        }

        private NavigationViewModel GetNavigationViewModel(Category category)
        {
            var cacheKey = CreateCacheKey("Navigation", category, null);

            var navigationViewModel = this.GetFromCache<NavigationViewModel>(cacheKey);
            if (navigationViewModel != null)
            {
                return navigationViewModel;
            }

            navigationViewModel = new NavigationViewModel(GetCategoryViewModel(category));
            var childCategories = GetChildCategories(category);
            navigationViewModel.ChildCategories.AddRange(childCategories.Select(i => GetCategoryViewModel(CatalogManager.GetCategory(i))));

            if (CatalogItemContext.Current != null)
            {
                if (CatalogItemContext.IsCategory)
                {
                    navigationViewModel.ActiveCategoryID = CatalogItemContext.Current?.Item?.ID;
                }
                else
                {
                    var categoryItem = CatalogManager.GetCategory(CatalogItemContext.Current.CategoryId);
                    navigationViewModel.ActiveCategoryID = categoryItem?.InnerItem.ID;
                }
            }

            return this.AddToCache(cacheKey, navigationViewModel);
        }

        protected void UpdateOptionsWithFacets(List<QueryFacet> facets, string valueQueryString, SearchOptions productSearchOptions)
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
                    var values = facetValues[1].Split(global::Sitecore.Feature.Commerce.Catalog.Website.Constants.FacetsSeparator);
                    foreach (var value in values)
                    {
                        existingFacet.Values.Add(value);
                    }
                }
            }

            productSearchOptions.FacetFields = facets;
        }

        protected void UpdateOptionsWithSorting(string sortField, SortDirection? sortDirection, SearchOptions productSearchOptions)
        {
            if (string.IsNullOrEmpty(sortField))
            {
                return;
            }
            productSearchOptions.SortField = sortField;

            if (sortDirection.HasValue)
            {
                productSearchOptions.SortDirection = sortDirection.Value;
            }

            ViewBag.SortField = sortField;
            ViewBag.SortDirection = sortDirection;
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

        public RelatedCatalogItemsViewModel GetRelationshipsFromItem(Item catalogItem, Rendering rendering)
        {
            if (catalogItem == null || !catalogItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Id) || !catalogItem.FieldHasValue(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Fields.RelationshipList))
            {
                return null;
            }

            RelationshipField field = catalogItem.Fields[global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Fields.RelationshipList];

            var model = new RelatedCatalogItemsViewModel();
            var productRelationshipInfoList = field.GetRelationships();
            productRelationshipInfoList = productRelationshipInfoList.OrderBy(x => x.Rank);
            var productModelList = GroupRelationshipsByDescription(productRelationshipInfoList);
            model.RelatedProducts.AddRange(productModelList);

            return model;
        }


        private IEnumerable<RelationshipViewModel> GroupRelationshipsByDescription(IEnumerable<CatalogRelationshipInformation> productRelationshipInfoList)
        {
            var relationshipGroups = new Dictionary<string, RelationshipViewModel>(StringComparer.OrdinalIgnoreCase);

            if (productRelationshipInfoList != null)
            {
                foreach (var relationshipInfo in productRelationshipInfoList)
                {
                    var relationshipDescription = string.IsNullOrWhiteSpace(relationshipInfo.RelationshipDescription) ? GetRelationshipName(relationshipInfo.RelationshipName) : relationshipInfo.RelationshipDescription;
                    RelationshipViewModel relationshipModel;
                    if (!relationshipGroups.TryGetValue(relationshipDescription, out relationshipModel))
                    {
                        relationshipModel = new RelationshipViewModel
                        {
                            Name = relationshipInfo.RelationshipName,
                            Description = relationshipDescription
                        };

                        relationshipGroups[relationshipDescription] = relationshipModel;
                    }

                    var targetItemId = new ID(relationshipInfo.ToItemExternalId);
                    var targetItem = Context.Database.GetItem(targetItemId);
                    var productModel = ProductViewModelFactory.Create(targetItem);

                    relationshipModel.ChildProducts.Add(productModel);
                }
            }

            if (relationshipGroups.Count <= 0)
            {
                return relationshipGroups.Values;
            }

            var productViewModelList = relationshipGroups.Values.SelectMany(v => v.ChildProducts).ToArray();
            if (!productViewModelList.Any())
            {
                return relationshipGroups.Values;
            }

            CatalogManager.GetProductBulkPrices(productViewModelList);
            InventoryManager.GetProductsStockStatusForList(productViewModelList);

            return relationshipGroups.Values;
        }

        private string GetRelationshipName(string relationshipName)
        {
            return DictionaryPhraseRepository.Current.Get($"/Catalog/Relationships/{relationshipName}", relationshipName);
        }

        private Category GetCurrentCategory()
        {
            if (!RenderingContext.Current.Rendering.Item.IsWildcardItem() && RenderingContext.Current.Rendering.Item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Category.Id))
            {
                return CatalogManager.GetCategory(Context.Item);
            }

            var categoryId = CatalogItemContext.Current?.CategoryId;
            if (categoryId == null)
            {
                return null;
            }
            var virtualCategoryCacheKey = $"VirtualCategory_{categoryId}";
            var currentCategory = this.GetFromCache<Category>(virtualCategoryCacheKey);
            if (currentCategory != null)
            {
                return currentCategory;
            }
            currentCategory = CatalogManager.GetCategory(categoryId, CatalogItemContext.Current.Catalog);
            return this.AddToCache(virtualCategoryCacheKey, currentCategory);
        }

        private string GetLastVisitedCategory(string customerId)
        {
            var categoryCookie = Request.Cookies[CookieName];

            return categoryCookie?[CustomerIdKey] != null && categoryCookie[CustomerIdKey].Equals(customerId, StringComparison.OrdinalIgnoreCase) ? categoryCookie[CategoryIdKey] : string.Empty;
        }

        private void SetLastVisitedCategory(string customerId, string categoryId)
        {
            // The cookie does not defined an expiry date therefore the browser will not persist it.
            var categoryCookie = Request.Cookies[CookieName] ?? new HttpCookie(CookieName);
            categoryCookie.Values[CustomerIdKey] = customerId;
            categoryCookie.Values[CategoryIdKey] = categoryId;
            Response.Cookies.Add(categoryCookie);
        }
    }
}