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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Catalog.Fields;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Alerts;
using Sitecore.Foundation.Alerts.Extensions;
using Sitecore.Foundation.Alerts.Models;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Models.Search;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models;
using Sitecore.Reference.Storefront.Models.JsonResults;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class CatalogController : BaseController
    {
        private const string CurrentProductViewModelKeyName = "CurrentProductViewModel";
        public static ID CommerceNamedSearchTemplate = new ID("{9F7D719A-3A05-4A64-AA74-3C46D8D0D20D}");
        private Catalog _currentCatalog;

        public CatalogController([NotNull] InventoryManager inventoryManager, [NotNull] ContactFactory contactFactory, [NotNull] AccountManager accountManager, [NotNull] CatalogManager catalogManager, [NotNull] GiftCardManager giftCardManager, [NotNull] PricingManager pricingManager, [NotNull] CartManager cartManager) : base(accountManager, contactFactory)
        {
            Assert.ArgumentNotNull(inventoryManager, nameof(inventoryManager));
            Assert.ArgumentNotNull(catalogManager, nameof(catalogManager));
            Assert.ArgumentNotNull(giftCardManager, nameof(giftCardManager));
            Assert.ArgumentNotNull(pricingManager, nameof(pricingManager));

            InventoryManager = inventoryManager;
            CatalogManager = catalogManager;
            GiftCardManager = giftCardManager;
            PricingManager = pricingManager;
            CartManager = cartManager;
        }

        public CartManager CartManager { get; }

        public PricingManager PricingManager { get; }

        public InventoryManager InventoryManager { get; }

        public CatalogManager CatalogManager { get; }

        public GiftCardManager GiftCardManager { get; }

        public Catalog CurrentCatalog => CatalogManager.CurrentCatalog;

        [HttpPost]
        public JsonResult SwitchCurrency(string currency)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(currency))
                {
                    PricingManager.CurrencyChosenPageEvent(StorefrontManager.CurrentStorefront, StorefrontManager.CurrentStorefront.DefaultCurrency);
                    CartManager.UpdateCartCurrency(StorefrontManager.CurrentStorefront, CurrentVisitorContext, StorefrontManager.CurrentStorefront.DefaultCurrency);
                }
            }
            catch (Exception e)
            {
                return Json(new BaseJsonResult("SwitchCurrency", e), JsonRequestBehavior.AllowGet);
            }

            return new JsonResult();
        }

        public ActionResult CategoryList()
        {
            var datasource = RenderingContext.Current.Rendering.DataSource;

            if (string.IsNullOrEmpty(datasource))
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var datasourceItem = Context.Database.GetItem(ID.Parse(datasource));
            var categoryViewModel = GetCategoryViewModel(null, null, datasourceItem, RenderingContext.Current.Rendering, string.Empty);

            return View(categoryViewModel);
        }

        public ActionResult RelatedProducts([Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField)
        {
            var viewModel = GetProductSearchViewModel(sortField, RenderingContext.Current.Rendering);
            if (viewModel == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            return View(viewModel);
        }

        private MultipleProductSearchResultsViewModel GetProductSearchViewModel(string sortField, Rendering currentRendering)
        {
            var datasource = currentRendering.Item;
            if (datasource == null)
                return null;

            var productSearchOptions = new CommerceSearchOptions
            {
                NumberOfItemsToReturn = 12,
                StartPageIndex = 0,
                SortField = sortField
            };

            var multipleProductSearchResults = CatalogManager.GetMultipleProductSearchResults(datasource, productSearchOptions);
            if (multipleProductSearchResults == null)
            {
                return null;
            }

            var viewModel = new MultipleProductSearchResultsViewModel(multipleProductSearchResults);
            viewModel.Initialize(RenderingContext.Current.Rendering);
            viewModel.DisplayName = datasource.DisplayName;

            var products = viewModel.ProductSearchResults.SelectMany(productSearchResult => productSearchResult.Products).ToList();
            CatalogManager.GetProductBulkPrices(CurrentVisitorContext, products);
            CatalogManager.InventoryManager.GetProductsStockStatusForList(StorefrontManager.CurrentStorefront, products);

            foreach (var productViewModel in products)
            {
                var productItem = multipleProductSearchResults.SearchResults.SelectMany(productSearchResult => productSearchResult.SearchResultItems).FirstOrDefault(item => item.Name == productViewModel.ProductId);
                productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);
            }
            return viewModel;
        }

        public ActionResult ProductRecommendation([Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField)
        {
            var viewModel = GetProductSearchViewModel(sortField, RenderingContext.Current.Rendering);
            if (viewModel == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            return View(viewModel);
        }

        public ActionResult ProductList(
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            Category currentCategory;

            //This is a Wild Card - Wild card pages are named "*"
            if (RenderingContext.Current.Rendering.Item.Name == "*")
            {
                //Supported option - pass in a categoryid
                currentCategory = CatalogManager.GetCurrentCategoryByUrl();
                ViewBag.Title = currentCategory.Name;
            }
            else
            {
                currentCategory = CatalogManager.GetCategory(Context.Item);
            }

            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var productSearchOptions = new CommerceSearchOptions(pageSize.GetValueOrDefault(currentCategory.ItemsPerPage), pageNumber.GetValueOrDefault(0));

            UpdateOptionsWithFacets(currentCategory.RequiredFacets, facetValues, productSearchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, productSearchOptions);

            var viewModel = GetCategoryViewModel(productSearchOptions, currentCategory.SortFields, currentCategory.InnerItem, RenderingContext.Current.Rendering, currentCategory.InnerItem.DisplayName);

            return View(viewModel);
        }

        public ActionResult Navigation()
        {
            var dataSourcePath = RenderingContext.Current.Rendering.Item["CategoryDatasource"];

            if (string.IsNullOrEmpty(dataSourcePath))
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var dataSource = RenderingContext.Current.Rendering.Item.Database.GetItem(dataSourcePath);

            if (dataSource == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var currentCategory = CatalogManager.GetCategory(dataSource);

            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var viewModel = GetNavigationViewModel(currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult ChildCategoryNavigation()
        {
            var categoryItem = CurrentSiteContext.CurrentCatalogItem;
            Assert.IsTrue(categoryItem.IsDerived(CommerceConstants.KnownTemplateIds.CommerceCategoryTemplate), "Current item must be a Category.");

            var viewModel = GetNavigationViewModel(categoryItem, RenderingContext.Current.Rendering);
            if (viewModel.ChildCategories.Count == 0)
            {
                viewModel = GetNavigationViewModel(categoryItem.Parent, RenderingContext.Current.Rendering);
            }

            return View(viewModel);
        }

        public ActionResult ProductListHeader(
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var currentCategory = CatalogManager.GetCurrentCategoryByUrl();
            var productSearchOptions = new CommerceSearchOptions(pageSize.GetValueOrDefault(currentCategory.ItemsPerPage), pageNumber.GetValueOrDefault(0));

            SetSortParameters(currentCategory, ref sortField, ref sortDirection);

            UpdateOptionsWithFacets(currentCategory.RequiredFacets, facetValues, productSearchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, productSearchOptions);

            var viewModel = GetProductListHeaderViewModel(productSearchOptions, currentCategory.SortFields, currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult Pagination(
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues)
        {
            var currentCategory = CatalogManager.GetCurrentCategoryByUrl();
            var productSearchOptions = new CommerceSearchOptions(pageSize.GetValueOrDefault(currentCategory.ItemsPerPage), pageNumber.GetValueOrDefault(0));

            UpdateOptionsWithFacets(currentCategory.RequiredFacets, facetValues, productSearchOptions);
            var viewModel = GetPaginationViewModel(productSearchOptions, currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult ProductFacets(
            [Bind(Prefix = StorefrontConstants.QueryStrings.Paging)] int? pageNumber,
            [Bind(Prefix = StorefrontConstants.QueryStrings.PageSize)] int? pageSize,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Facets)] string facetValues,
            [Bind(Prefix = StorefrontConstants.QueryStrings.Sort)] string sortField,
            [Bind(Prefix = StorefrontConstants.QueryStrings.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            var currentCategory = CatalogManager.GetCurrentCategoryByUrl();
            var productSearchOptions = new CommerceSearchOptions(pageSize.GetValueOrDefault(currentCategory.ItemsPerPage), pageNumber.GetValueOrDefault(0));

            SetSortParameters(currentCategory, ref sortField, ref sortDirection);

            UpdateOptionsWithFacets(currentCategory.RequiredFacets, facetValues, productSearchOptions);
            UpdateOptionsWithSorting(sortField, sortDirection, productSearchOptions);
            var viewModel = GetProductFacetsViewModel(productSearchOptions, currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult CuratedProductImages()
        {
            var images = new List<MediaItem>();

            MultilistField field = RenderingContext.Current.Rendering.Item.Fields["ProductImages"];

            if (field != null)
            {
                images.AddRange(field.TargetIDs.Select(id => RenderingContext.Current.Rendering.Item.Database.GetItem(id)).Select(mediaItem => (MediaItem) mediaItem));
            }

            return View(images);
        }

        public ActionResult CategoryPageHeader()
        {
            //This is a Wild Card - Wild card pages are named "*"
            var currentCategory = RenderingContext.Current.Rendering.Item.Name == "*" ? CatalogManager.GetCurrentCategoryByUrl() : CatalogManager.GetCategory(Context.Item);
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var model = new CategoryViewModel(currentCategory.InnerItem);
            model.Initialize(RenderingContext.Current.Rendering);

            return View(model);
        }

        [HttpPost]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult FacetApplied(string facetValue, bool? isApplied)
        {
            if (!string.IsNullOrWhiteSpace(facetValue) && isApplied.HasValue)
            {
                CatalogManager.FacetApplied(StorefrontManager.CurrentStorefront, facetValue, isApplied.Value);
            }

            return new BaseJsonResult();
        }

        [HttpPost]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult SortOrderApplied(string sortField, CommerceConstants.SortDirection? sortDirection)
        {
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                CatalogManager.SortOrderApplied(StorefrontManager.CurrentStorefront, sortField, sortDirection);
            }

            return new BaseJsonResult();
        }

        public ActionResult AddToCart()
        {
            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult ProductImages()
        {
            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult ProductInformation()
        {
            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult ProductRating()
        {
            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult VisitedProductDetailsPage()
        {
            var model = GetProductViewModel();

            if (model != null)
            {
                CatalogManager.VisitedProductDetailsPage(StorefrontManager.CurrentStorefront, model.ProductId, model.ProductName, model.ParentCategoryId, model.ParentCategoryName);
            }

            return new EmptyResult();
        }

        public ActionResult VisitedCategoryPage()
        {
            var lastCategoryId = CategoryCookieHelper.GetLastVisitedCategory(CurrentVisitorContext.GetCustomerId());

            //This is a Wild Card - Wild card pages are named "*"
            var currentCategory = RenderingContext.Current.Rendering.Item.Name == "*" ? CatalogManager.GetCurrentCategoryByUrl() : CatalogManager.GetCategory(Context.Item);

            if (currentCategory == null)
            {
                return new EmptyResult();
            }

            if (!string.IsNullOrWhiteSpace(lastCategoryId) && lastCategoryId.Equals(currentCategory.Name))
            {
                return new EmptyResult();
            }

            CatalogManager.VisitedCategoryPage(StorefrontManager.CurrentStorefront, currentCategory.Name, currentCategory.DisplayName);
            CategoryCookieHelper.SetLastVisitedCategory(CurrentVisitorContext.GetCustomerId(), currentCategory.Name);

            return new EmptyResult();
        }

        public ActionResult RelatedCatalogItems()
        {
            //Wild card pages are named "*"
            if (RenderingContext.Current.Rendering.Item.Name == "*")
            {
                // This is a Wild Card
                var productViewModel = GetWildCardProductViewModel();
                var relatedCatalogItemsModel = GetRelationshipsFromItem(StorefrontManager.CurrentStorefront, CurrentVisitorContext, productViewModel.Item, RenderingContext.Current.Rendering);
                return View(relatedCatalogItemsModel);
            }
            else
            {
                var relatedCatalogItemsModel = GetRelationshipsFromItem(StorefrontManager.CurrentStorefront, CurrentVisitorContext, RenderingContext.Current.Rendering.Item, RenderingContext.Current.Rendering);
                return View(relatedCatalogItemsModel);
            }
        }

        [HttpGet]
        public ActionResult CheckGiftCardBalance()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ProductPresentation()
        {
            return View();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult GetCurrentProductStockInfo(ProductStockInfoInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var currentProductItem = CatalogManager.GetProduct(model.ProductId, CurrentCatalog.Name);
                var productId = currentProductItem.Name;
                var catalogName = currentProductItem["CatalogName"];
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

                var response = InventoryManager.GetStockInformation(StorefrontManager.CurrentStorefront, products, StockDetailsLevel.All);
                var result = new StockInfoListBaseJsonResult(response.ServiceProviderResult);
                if (response.Result == null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result.Initialize(response.Result);
                var stockInfo = response.Result.FirstOrDefault();
                if (stockInfo != null)
                {
                    InventoryManager.VisitedProductStockStatus(StorefrontManager.CurrentStorefront, stockInfo, string.Empty);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("GetCurrentProductStockInfo", this, e);
                return Json(new BaseJsonResult("GetCurrentProductStockInfo", e), JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult CheckGiftCardBalance(GetGiftCardBalanceInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));

                var validationResult = new BaseJsonResult();
                ValidateModel(validationResult);
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = GiftCardManager.GetGiftCardBalance(StorefrontManager.CurrentStorefront, CurrentVisitorContext, inputModel.GiftCardId);
                var result = new GiftCardBaseJsonResult(response.ServiceProviderResult);
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
                CommerceLog.Current.Error("CheckGiftCardBalance", this, e);
                return Json(new BaseJsonResult("CheckGiftCardBalance", e), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult SignUpForBackInStockNotification(SignUpForNotificationInputModel model)
        {
            try
            {
                Assert.ArgumentNotNull(model, nameof(model));

                var result = new BaseJsonResult();
                ValidateModel(result);
                if (result.HasErrors)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var response = InventoryManager.VisitorSignupForStockNotification(StorefrontManager.CurrentStorefront, model, string.Empty);
                result = new BaseJsonResult(response.ServiceProviderResult);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                CommerceLog.Current.Error("GetCurrentUser", this, e);
                return Json(new BaseJsonResult("GetCurrentUser", e), JsonRequestBehavior.AllowGet);
            }
        }

        public ProductListHeaderViewModel GetProductListHeaderViewModel(CommerceSearchOptions productSearchOptions, IEnumerable<CommerceQuerySort> sortFields, Item categoryItem, Rendering rendering)
        {
            var viewModel = new ProductListHeaderViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, categoryItem);
            }

            viewModel.Initialize(rendering, childProducts, sortFields, productSearchOptions);

            return viewModel;
        }

        public PaginationViewModel GetPaginationViewModel(CommerceSearchOptions productSearchOptions, Item categoryItem, Rendering rendering)
        {
            var viewModel = new PaginationViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, categoryItem);
            }

            viewModel.Initialize(rendering, childProducts, productSearchOptions);

            return viewModel;
        }

        public ProductFacetsViewModel GetProductFacetsViewModel(CommerceSearchOptions productSearchOptions, Item categoryItem, Rendering rendering)
        {
            var viewModel = new ProductFacetsViewModel();

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, categoryItem);
            }

            viewModel.Initialize(rendering, childProducts, productSearchOptions);

            return viewModel;
        }

        protected CategorySearchResults GetChildCategories(CommerceSearchOptions searchOptions, Item categoryItem)
        {
            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalCategoryCount = 0;

            if (RenderingContext.Current.Rendering.Item != null)
            {
                return CatalogManager.GetCategoryChildCategories(categoryItem.ID, searchOptions);
            }

            return new CategorySearchResults(returnList, totalCategoryCount, totalPageCount, searchOptions.StartPageIndex, new List<FacetCategory>());
        }

        protected ProductViewModel GetProductViewModel(Item productItem, Rendering rendering)
        {
            if (CurrentSiteContext.Items[CurrentProductViewModelKeyName] != null)
            {
                return (ProductViewModel) CurrentSiteContext.Items[CurrentProductViewModelKeyName];
            }

            var variants = new List<VariantViewModel>();
            if (productItem != null && productItem.HasChildren)
            {
                foreach (Item item in productItem.Children)
                {
                    var v = new VariantViewModel(item);
                    variants.Add(v);
                }
            }

            var productViewModel = new ProductViewModel(productItem);
            productViewModel.Initialize(rendering, variants);

            productViewModel.ProductName = productViewModel.DisplayName;

            if (CurrentSiteContext.UrlContainsCategory)
            {
                productViewModel.ParentCategoryId = CatalogUrlManager.ExtractCategoryNameFromCurrentUrl();

                var category = CatalogManager.GetCategory(productViewModel.ParentCategoryId);
                if (category != null)
                {
                    productViewModel.ParentCategoryName = category.DisplayName;
                }
            }

            CatalogManager.GetProductPrice(CurrentVisitorContext, productViewModel);
            productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);

            CurrentSiteContext.Items[CurrentProductViewModelKeyName] = productViewModel;
            return productViewModel;
        }

        protected void PopulateStockInformation(ProductViewModel model)
        {
            var inventoryProducts = new List<CommerceInventoryProduct> {new CommerceInventoryProduct {ProductId = model.ProductId, CatalogName = model.CatalogName}};
            var response = InventoryManager.GetStockInformation(StorefrontManager.CurrentStorefront, inventoryProducts, StockDetailsLevel.StatusAndAvailability);
            if (!response.ServiceProviderResult.Success || response.Result == null)
            {
                return;
            }

            var stockInfos = response.Result;
            var stockInfo = stockInfos.FirstOrDefault();
            if (stockInfo == null || stockInfo.Status == null)
            {
                return;
            }

            model.StockStatus = stockInfo.Status;
            model.StockStatusName = StorefrontManager.GetProductStockStatusName(model.StockStatus);
            if (stockInfo.AvailabilityDate != null)
            {
                model.StockAvailabilityDate = stockInfo.AvailabilityDate.Value.ToDisplayedDate();
            }
        }

        protected virtual CategoryViewModel GetCategoryViewModel(CommerceSearchOptions productSearchOptions, IEnumerable<CommerceQuerySort> sortFields, Item categoryItem, Rendering rendering, string cacheName)
        {
            var cacheKey = "Category/" + cacheName;
            var noCache = string.IsNullOrEmpty(cacheName);

            if (CurrentSiteContext.Items[cacheKey] != null && !noCache)
            {
                return (CategoryViewModel) CurrentSiteContext.Items[cacheKey];
            }

            var categoryViewModel = new CategoryViewModel(categoryItem);
            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, categoryItem);
            }

            categoryViewModel.Initialize(rendering, childProducts, sortFields, productSearchOptions);
            if (childProducts != null && childProducts.SearchResultItems.Count > 0)
            {
                CatalogManager.GetProductBulkPrices(CurrentVisitorContext, categoryViewModel.ChildProducts);
                InventoryManager.GetProductsStockStatusForList(StorefrontManager.CurrentStorefront, categoryViewModel.ChildProducts);

                foreach (var productViewModel in categoryViewModel.ChildProducts)
                {
                    var productItem = childProducts.SearchResultItems.Where(item => item.Name == productViewModel.ProductId).Single();
                    productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);
                }
            }

            if (!noCache)
            {
                CurrentSiteContext.Items[cacheKey] = categoryViewModel;
            }

            return categoryViewModel;
        }

        protected virtual NavigationViewModel GetNavigationViewModel(Item categoryItem, Rendering rendering)
        {
            var cacheKey = "Navigation/" + categoryItem.Name;
            var noCache = string.IsNullOrEmpty(categoryItem.Name);

            if (CurrentSiteContext.Items[cacheKey] != null && !noCache)
            {
                return (NavigationViewModel) CurrentSiteContext.Items[cacheKey];
            }

            var navigationViewModel = new NavigationViewModel();
            var childCategories = GetChildCategories(new CommerceSearchOptions(), categoryItem);
            navigationViewModel.Initialize(rendering, childCategories);
            if (noCache)
            {
                return navigationViewModel;
            }

            CurrentSiteContext.Items[cacheKey] = navigationViewModel;
            return navigationViewModel;
        }

        protected SearchResults GetChildProducts(CommerceSearchOptions searchOptions, Item categoryItem)
        {
            IEnumerable<CommerceQueryFacet> facets = null;
            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;

            var childProductsCacheKey = $"ChildProductSearch_{categoryItem.ID.ToString()}";

            if (!CurrentSiteContext.Items.Contains(childProductsCacheKey))
            {
                if (RenderingContext.Current.Rendering.Item != null)
                {
                    SearchResponse searchResponse;
                    if (categoryItem.IsDerived(CommerceConstants.KnownTemplateIds.CommerceDynamicCategoryTemplate) || categoryItem.IsDerived(Templates.NamedSearch.ID))
                    {
                        searchResponse = CatalogManager.FindCatalogItems(categoryItem, searchOptions);
                    }
                    else
                    {
                        searchResponse = CatalogManager.GetCategoryProducts(categoryItem.ID, searchOptions);
                    }

                    if (searchResponse != null)
                    {
                        returnList.AddRange(searchResponse.ResponseItems);

                        totalProductCount = searchResponse.TotalItemCount;
                        totalPageCount = searchResponse.TotalPageCount;
                        facets = searchResponse.Facets;
                    }
                }

                var results = new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
                CurrentSiteContext.Items[childProductsCacheKey] = results;
            }

            return (SearchResults) CurrentSiteContext.Items[childProductsCacheKey];
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

        protected ProductViewModel GetProductViewModel()
        {
            //Wild card pages are named "*"
            if (RenderingContext.Current.Rendering.Item.Name == "*")
            {
                // This is a Wild Card
                return GetWildCardProductViewModel();
            }

            return GetProductViewModel(RenderingContext.Current.Rendering.Item, RenderingContext.Current.Rendering);
        }

        protected void SetSortParameters(Category category, ref string sortField, ref CommerceConstants.SortDirection? sortOrder)
        {
            if (string.IsNullOrWhiteSpace(sortField))
            {
                var sortfieldList = category.SortFields;
                if (sortfieldList != null && sortfieldList.Count > 0)
                {
                    sortField = sortfieldList[0].Name;
                    sortOrder = CommerceConstants.SortDirection.Asc;
                }
            }
        }

        private ProductViewModel GetWildCardProductViewModel()
        {
            ProductViewModel productViewModel;
            var productId = CatalogUrlManager.ExtractItemIdFromCurrentUrl();
            var virtualProductCacheKey = $"VirtualProduct_{productId}";
            if (CurrentSiteContext.Items.Contains(virtualProductCacheKey))
            {
                productViewModel = CurrentSiteContext.Items[virtualProductCacheKey] as ProductViewModel;
            }
            else
            {
                if (string.IsNullOrEmpty(productId))
                {
                    //No ProductId passed in on the URL
                    //Use to Storefront DefaultProductId
                    productId = StorefrontManager.CurrentStorefront.DefaultProductId;
                }

                var productItem = CatalogManager.GetProduct(productId, CurrentCatalog.Name);
                if (productItem == null)
                {
                    Log.Error($"The requested product '{productId}' does not exist in the catalog '{CurrentCatalog.Name}' or cannot be displayed in the language '{Context.Language}'", this);
                    throw new InvalidOperationException($"The requested product '{productId}' does not exist in the catalog '{CurrentCatalog.Name}' or cannot be displayed in the language '{Context.Language}'");
                }

                RenderingContext.Current.Rendering.Item = productItem;
                productViewModel = GetProductViewModel(RenderingContext.Current.Rendering.Item, RenderingContext.Current.Rendering);
                CurrentSiteContext.Items.Add(virtualProductCacheKey, productViewModel);
            }

            return productViewModel;
        }

        public RelatedCatalogItemsViewModel GetRelationshipsFromItem([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, Item catalogItem, Rendering rendering)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            if (catalogItem != null &&
                catalogItem.Fields.Contains(CommerceConstants.KnownFieldIds.RelationshipList) &&
                !string.IsNullOrEmpty(catalogItem[CommerceConstants.KnownFieldIds.RelationshipList]))
            {
                var field = new RelationshipField(catalogItem.Fields[CommerceConstants.KnownFieldIds.RelationshipList]);
                if (rendering != null &&
                    !string.IsNullOrWhiteSpace(rendering.RenderingItem.InnerItem["RelationshipsToDisplay"]))
                {
                    var relationshipsToDisplay =
                        rendering.RenderingItem.InnerItem["RelationshipsToDisplay"].Split(new[] {"|"},
                            StringSplitOptions.RemoveEmptyEntries);
                    return GetRelationshipsFromField(storefront, visitorContext, field, rendering,
                        relationshipsToDisplay);
                }
                return GetRelationshipsFromField(storefront, visitorContext, field, rendering);
            }

            return null;
        }

        public RelatedCatalogItemsViewModel GetRelationshipsFromField([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, RelationshipField field, Rendering rendering)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            return GetRelationshipsFromField(storefront, visitorContext, field, rendering, null);
        }

        public RelatedCatalogItemsViewModel GetRelationshipsFromField([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, RelationshipField field, Rendering rendering, IEnumerable<string> relationshipNames)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            relationshipNames = relationshipNames ?? Enumerable.Empty<string>();
            relationshipNames = relationshipNames.Select(s => s.Trim());
            var model = new RelatedCatalogItemsViewModel();

            if (field != null)
            {
                var productRelationshipInfoList = field.GetRelationships();
                productRelationshipInfoList = productRelationshipInfoList.OrderBy(x => x.Rank);
                var productModelList = GroupRelationshipsByDescription(storefront, visitorContext, field, relationshipNames, productRelationshipInfoList, rendering);
                model.RelatedProducts.AddRange(productModelList);
            }

            model.Initialize(rendering);

            return model;
        }


        protected IEnumerable<CategoryViewModel> GroupRelationshipsByDescription([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, RelationshipField field, IEnumerable<string> relationshipNames, IEnumerable<CatalogRelationshipInformation> productRelationshipInfoList, Rendering rendering)
        {
            var relationshipGroups = new Dictionary<string, CategoryViewModel>(StringComparer.OrdinalIgnoreCase);

            if (field != null && productRelationshipInfoList != null)
            {
                foreach (var relationshipInfo in productRelationshipInfoList)
                {
                    if (relationshipNames.Contains(relationshipInfo.RelationshipName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Item lookupItem = null;
                    var usingRelationshipName = string.IsNullOrWhiteSpace(relationshipInfo.RelationshipDescription);
                    var relationshipDescription = string.IsNullOrWhiteSpace(relationshipInfo.RelationshipDescription)
                        ? StorefrontManager.GetRelationshipName(relationshipInfo.RelationshipName, out lookupItem)
                        : relationshipInfo.RelationshipDescription;
                    CategoryViewModel categoryModel = null;
                    if (!relationshipGroups.TryGetValue(relationshipDescription, out categoryModel))
                    {
                        categoryModel = new CategoryViewModel
                        {
                            ChildProducts = new List<ProductViewModel>(),
                            RelationshipName = relationshipInfo.RelationshipName,
                            RelationshipDescription = relationshipDescription,
                            LookupRelationshipItem = usingRelationshipName ? lookupItem : null
                        };

                        relationshipGroups[relationshipDescription] = categoryModel;
                    }

                    var targetItemId = ID.Parse(relationshipInfo.ToItemExternalId);
                    var targetItem = field.InnerField.Database.GetItem(targetItemId);
                    var productModel = new ProductViewModel(targetItem);
                    productModel.Initialize(rendering);

                    CatalogManager.GetProductRating(targetItem);

                    categoryModel.ChildProducts.Add(productModel);
                }
            }

            if (relationshipGroups.Count > 0)
            {
                var productViewModelList = new List<ProductViewModel>();

                foreach (var key in relationshipGroups.Keys)
                {
                    var viewModel = relationshipGroups[key];
                    var childProducts = viewModel.ChildProducts;
                    if (childProducts != null && childProducts.Count > 0)
                    {
                        productViewModelList.AddRange(childProducts);
                    }
                }

                if (productViewModelList.Count > 0)
                {
                    CatalogManager.GetProductBulkPrices(visitorContext, productViewModelList);
                    InventoryManager.GetProductsStockStatusForList(storefront, productViewModelList);
                }
            }

            return relationshipGroups.Values;
        }
    }
}