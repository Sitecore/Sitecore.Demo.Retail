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
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog.Fields;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Feature.Commerce.Catalog.Models.JsonResults;
using Sitecore.Feature.Commerce.Catalog.Services;
using Sitecore.Foundation.Alerts;
using Sitecore.Foundation.Alerts.Extensions;
using Sitecore.Foundation.Alerts.Models;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Models.Search;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Controllers
{
    public class CatalogController : SitecoreController
    {
        public CatalogController(InventoryManager inventoryManager, ContactFactory contactFactory, AccountManager accountManager, CatalogManager catalogManager, GiftCardManager giftCardManager, PricingManager pricingManager, [NotNull] CartManager cartManager, VisitorContextRepository visitorContextRepository, CatalogItemContext catalogItemContext, CatalogUrlService catalogUrlRepository)
        {
            InventoryManager = inventoryManager;
            CatalogManager = catalogManager;
            GiftCardManager = giftCardManager;
            VisitorContextRepository = visitorContextRepository;
            CatalogItemContext = catalogItemContext;
        }

        private VisitorContextRepository VisitorContextRepository { get; }
        public CatalogItemContext CatalogItemContext { get; }
        private InventoryManager InventoryManager { get; }
        private CatalogManager CatalogManager { get; }
        private GiftCardManager GiftCardManager { get; }

        public ActionResult CategoryList()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var datasource = RenderingContext.Current.Rendering.DataSource;

            if (string.IsNullOrEmpty(datasource) || !RenderingContext.Current.Rendering.Item.IsDerived(Foundation.Commerce.Templates.Commerce.Category.ID))
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var categoryViewModel = GetCategoryViewModel(CatalogManager.GetCategory(RenderingContext.Current.Rendering.Item));

            return View(categoryViewModel);
        }

        public ActionResult RelatedProducts([Bind(Prefix = Constants.QueryString.Sort)] string sortField)
        {
            if (CatalogManager.CatalogContext == null)
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
            CatalogManager.GetProductBulkPrices(VisitorContextRepository.GetCurrent(), products);
            CatalogManager.InventoryManager.GetProductsStockStatusForList(StorefrontManager.CurrentStorefront, products);

            foreach (var productViewModel in products)
            {
                productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productViewModel.Item);
            }
            return viewModel;
        }

        public MultipleProductSearchResults GetMultipleProductSearchResults(Item dataSource, string sortField)
        {
            if (!dataSource.IsDerived(Templates.HasNamedSearches.ID))
                return null;
            MultilistField searchesField = dataSource.Fields[Templates.HasNamedSearches.Fields.NamedSearches];
            var searches = searchesField.GetItems();
            var productsSearchResults = new List<SearchResults>();
            foreach (var search in searches)
            {
                if (search.IsDerived(Templates.NamedSearch.ID))
                {
                    var productSearchOptions = new CommerceSearchOptions
                    {
                        NumberOfItemsToReturn = 12,
                        StartPageIndex = 0,
                        SortField = sortField
                    };

                    var productsSearchResult = CatalogManager.GetProductSearchResults(search, productSearchOptions);
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
                        if (!productItem.IsDerived(Foundation.Commerce.Templates.Commerce.Category.ID) && !productItem.IsDerived(Foundation.Commerce.Templates.Commerce.Category.ID))
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


        public ActionResult ProductRecommendation([Bind(Prefix = Constants.QueryString.Sort)] string sortField)
        {
            if (CatalogManager.CatalogContext == null)
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

        public ActionResult ProductList([Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber, 
                                        [Bind(Prefix = Constants.QueryString.Facets)] string facetValues, 
                                        [Bind(Prefix = Constants.QueryString.Sort)] string sortField, 
                                        [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize, 
                                        [Bind(Prefix = Constants.QueryString.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            Category currentCategory;

            if (RenderingContext.Current.Rendering.Item.IsWildcardItem())
            {
                //Supported option - pass in a categoryid
                currentCategory = GetCurrentCategory();
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

            var searchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize, sortField, sortDirection);

            var viewModel = GetCategoryViewModel(currentCategory, searchOptions);

            return View(viewModel);
        }

        private CommerceSearchOptions GetCategorySearchOptions(Category currentCategory, int? pageNumber, string facetValues, int? pageSize, string sortField = null, CommerceConstants.SortDirection? sortDirection = null)
        {
            var searchOptions = new CommerceSearchOptions(pageSize ?? currentCategory.ItemsPerPage, pageNumber ?? 0);
            UpdateOptionsWithFacets(currentCategory.RequiredFacets, facetValues, searchOptions);
            if (sortField != null) { 
                SetSortParameters(currentCategory, ref sortField, ref sortDirection);
                UpdateOptionsWithSorting(sortField, sortDirection, searchOptions);
            }
            return searchOptions;
        }

        public ActionResult Navigation()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var item = RenderingContext.Current.Rendering.Item;
            var dataSource = item.IsDerived(Foundation.Commerce.Templates.Commerce.NavigationItem.ID) ? item?.TargetItem(Foundation.Commerce.Templates.Commerce.NavigationItem.Fields.CategoryDatasource) : null;
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
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            var productSearchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize, sortField, sortDirection);

            var viewModel = GetProductListHeaderViewModel(productSearchOptions, currentCategory.SortFields, currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult Pagination(
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();
            var productSearchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize);
            var viewModel = GetPaginationViewModel(productSearchOptions, currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult ProductFacets(
            [Bind(Prefix = Constants.QueryString.Paging)] int? pageNumber,
            [Bind(Prefix = Constants.QueryString.PageSize)] int? pageSize,
            [Bind(Prefix = Constants.QueryString.Facets)] string facetValues,
            [Bind(Prefix = Constants.QueryString.Sort)] string sortField,
            [Bind(Prefix = Constants.QueryString.SortDirection)] CommerceConstants.SortDirection? sortDirection)
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = GetCurrentCategory();

            var searchOptions = GetCategorySearchOptions(currentCategory, pageNumber, facetValues, pageSize, sortField, sortDirection);

            var viewModel = GetProductFacetsViewModel(searchOptions, currentCategory.InnerItem, RenderingContext.Current.Rendering);

            return View(viewModel);
        }

        public ActionResult CategoryPageHeader()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var currentCategory = RenderingContext.Current.Rendering.Item.IsWildcardItem() ? GetCurrentCategory() : CatalogManager.GetCategory(Context.Item);
            if (currentCategory == null)
            {
                return this.InfoMessage(InfoMessage.Error(AlertTexts.InvalidDataSourceTemplateFriendlyMessage));
            }

            var model = new CategoryViewModel(currentCategory.InnerItem);
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
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult ProductImages()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var model = GetProductViewModel();
            if (model.Images == null || !model.Images.Any())
                return new EmptyResult();

            return View(model);
        }

        public ActionResult ProductInformation()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult ProductRating()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var model = GetProductViewModel();
            return View(model);
        }

        public ActionResult VisitedProductDetailsPage()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var model = GetProductViewModel();

            if (model != null)
            {
                CatalogManager.VisitedProductDetailsPage(StorefrontManager.CurrentStorefront, model.ProductId, model.ProductName, model.ParentCategoryId, model.ParentCategoryName);
            }

            return new EmptyResult();
        }

        public ActionResult VisitedCategoryPage()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            var lastCategoryId = CategoryCookieHelper.GetLastVisitedCategory(VisitorContextRepository.GetCurrent().GetCustomerId());

            var currentCategory = RenderingContext.Current.Rendering.Item.IsWildcardItem() ? GetCurrentCategory() : CatalogManager.GetCategory(Context.Item);

            if (currentCategory == null)
            {
                return new EmptyResult();
            }

            if (!string.IsNullOrWhiteSpace(lastCategoryId) && lastCategoryId.Equals(currentCategory.Name))
            {
                return new EmptyResult();
            }

            CatalogManager.VisitedCategoryPage(StorefrontManager.CurrentStorefront, currentCategory.Name, currentCategory.Title);
            CategoryCookieHelper.SetLastVisitedCategory(VisitorContextRepository.GetCurrent().GetCustomerId(), currentCategory.Name);

            return new EmptyResult();
        }

        public ActionResult RelatedCatalogItems()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

            if (RenderingContext.Current.Rendering.Item.IsWildcardItem())
            {
                // This is a Wild Card
                var productViewModel = GetWildCardProductViewModel();
                var relatedCatalogItemsModel = GetRelationshipsFromItem(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), productViewModel.Item, RenderingContext.Current.Rendering);
                return View(relatedCatalogItemsModel);
            }
            else
            {
                var relatedCatalogItemsModel = GetRelationshipsFromItem(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), RenderingContext.Current.Rendering.Item, RenderingContext.Current.Rendering);
                return View(relatedCatalogItemsModel);
            }
        }

        [HttpGet]
        public ActionResult CheckGiftCardBalance()
        {
            if (CatalogManager.CatalogContext == null)
            {
                return this.InfoMessage(InfoMessage.Error("This rendering cannot be shown without a valid catalog context."));
            }

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

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var currentProductItem = CatalogManager.GetProduct(model.ProductId);
                var productId = currentProductItem.Name;
                var catalogName = currentProductItem[Foundation.Commerce.Templates.Commerce.CatalogItem.Fields.CatalogName];
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

                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = GiftCardManager.GetGiftCardBalance(StorefrontManager.CurrentStorefront, VisitorContextRepository.GetCurrent(), inputModel.GiftCardId);
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

                var result = this.CreateJsonResult();
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

        private IEnumerable<Item> GetChildCategories(Category category)
        {
            return RenderingContext.Current.Rendering.Item != null ? CatalogManager.GetCategoryChildCategories(category.InnerItem).CategoryItems : Enumerable.Empty<Item>();
        }

        private ProductViewModel GetProductViewModel(Item productItem)
        {
            const string cacheKey = "CurrentProductViewModel";
            var productViewModel = this.GetFromCache<ProductViewModel>(cacheKey);
            if (productViewModel != null)
            {
                return productViewModel;
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

            productViewModel = new ProductViewModel(productItem, variants);
            productViewModel.ProductName = productViewModel.Title;

            if (CatalogItemContext.Current != null)
            {
                productViewModel.ParentCategoryId = CatalogItemContext.Current.CategoryId;

                var category = CatalogManager.GetCategory(productViewModel.ParentCategoryId);
                if (category != null)
                {
                    productViewModel.ParentCategoryName = category.Title;
                }
            }
            PopulateStockInformation(productViewModel);

            CatalogManager.GetProductPrice(VisitorContextRepository.GetCurrent(), productViewModel);
            productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);

            return this.AddToCache(cacheKey, productViewModel);
        }

        private void PopulateStockInformation(ProductViewModel model)
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
            model.StockStatusName = LookupManager.GetProductStockStatusName(model.StockStatus);
            if (stockInfo.AvailabilityDate != null)
            {
                model.StockAvailabilityDate = stockInfo.AvailabilityDate.Value.ToDisplayedDate();
            }
        }

        private CategoryViewModel GetCategoryViewModel(Category category, CommerceSearchOptions productSearchOptions = null)
        {
            Assert.IsNotNull(category, nameof(category));

            var cacheKey = $"Category/{category.InnerItem.ID}/{productSearchOptions}";

            var categoryViewModel = this.GetFromCache<CategoryViewModel>(cacheKey);
            if (categoryViewModel != null)
            {
                return categoryViewModel;
            }

            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = GetChildProducts(productSearchOptions, category.InnerItem);
            }

            categoryViewModel = new CategoryViewModel(category.InnerItem, childProducts, category.SortFields, productSearchOptions);

            if (childProducts != null && childProducts.SearchResultItems.Count > 0)
            {
                CatalogManager.GetProductBulkPrices(VisitorContextRepository.GetCurrent(), categoryViewModel.ChildProducts);
                InventoryManager.GetProductsStockStatusForList(StorefrontManager.CurrentStorefront, categoryViewModel.ChildProducts);

                foreach (var productViewModel in categoryViewModel.ChildProducts)
                {
                    var productItem = childProducts.SearchResultItems.Single(item => item.Name == productViewModel.ProductId);
                    productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productItem);
                }
            }
            return this.AddToCache(cacheKey, categoryViewModel);
        }

        private NavigationViewModel GetNavigationViewModel(Category category)
        {
            var cacheKey = "Navigation/" + category.Name;

            var navigationViewModel = this.GetFromCache<NavigationViewModel>(cacheKey);
            if (navigationViewModel != null)
            {
                return navigationViewModel;
            }

            navigationViewModel = new NavigationViewModel(GetCategoryViewModel(category));
            var childCategories = GetChildCategories(category);
            navigationViewModel.ChildCategories.AddRange(childCategories.Select(i => GetCategoryViewModel(CatalogManager.GetCategory(i))));

            return this.AddToCache(cacheKey, navigationViewModel);
        }

        private SearchResults GetChildProducts(CommerceSearchOptions searchOptions, Item categoryItem)
        {
            var cacheKey = $"ChildProductSearch_{categoryItem.ID}";

            var results = this.GetFromCache<SearchResults>(cacheKey);
            if (results != null)
            {
                return results;
            }
            results = CatalogManager.GetChildProducts(searchOptions, categoryItem);

            return this.AddToCache(cacheKey, results);
        }

        protected void UpdateOptionsWithFacets(List<CommerceQueryFacet> facets, string valueQueryString, CommerceSearchOptions productSearchOptions)
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
                    var values = facetValues[1].Split(Constants.FacetsSeparator);
                    foreach (var value in values)
                    {
                        existingFacet.Values.Add(value);
                    }
                }
            }

            productSearchOptions.FacetFields = facets;
        }

        protected void UpdateOptionsWithSorting(string sortField, CommerceConstants.SortDirection? sortDirection, CommerceSearchOptions productSearchOptions)
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

        private ProductViewModel GetProductViewModel()
        {
            if (RenderingContext.Current.Rendering.Item.IsWildcardItem())
            {
                return GetWildCardProductViewModel();
            }

            return GetProductViewModel(RenderingContext.Current.Rendering.Item);
        }

        private ProductViewModel GetWildCardProductViewModel()
        {
            if (CatalogItemContext.IsCategory)
                return null;

            var productItem = CatalogItemContext.Current?.Item;
            if (productItem == null)
            {
                return null;
            }

            RenderingContext.Current.Rendering.Item = productItem;
            return GetProductViewModel(RenderingContext.Current.Rendering.Item);
        }

        public RelatedCatalogItemsViewModel GetRelationshipsFromItem([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, Item catalogItem, Rendering rendering)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            if (catalogItem == null || !catalogItem.IsDerived(Foundation.Commerce.Templates.Commerce.CatalogItem.ID) || catalogItem.FieldHasValue(Foundation.Commerce.Templates.Commerce.CatalogItem.Fields.RelationshipList))
            {
                return null;
            }

            RelationshipField field = catalogItem.Fields[Foundation.Commerce.Templates.Commerce.CatalogItem.Fields.RelationshipList];

            return GetRelationshipsFromField(storefront, visitorContext, field, rendering);
        }

        private RelatedCatalogItemsViewModel GetRelationshipsFromField([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, RelationshipField field, Rendering rendering)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));

            var model = new RelatedCatalogItemsViewModel();

            if (field != null)
            {
                var productRelationshipInfoList = field.GetRelationships();
                productRelationshipInfoList = productRelationshipInfoList.OrderBy(x => x.Rank);
                var productModelList = GroupRelationshipsByDescription(storefront, visitorContext, field, productRelationshipInfoList);
                model.RelatedProducts.AddRange(productModelList);
            }

            model.Initialize(rendering);

            return model;
        }


        private IEnumerable<RelationshipViewModel> GroupRelationshipsByDescription([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, RelationshipField field, IEnumerable<CatalogRelationshipInformation> productRelationshipInfoList)
        {
            var relationshipGroups = new Dictionary<string, RelationshipViewModel>(StringComparer.OrdinalIgnoreCase);

            if (field != null && productRelationshipInfoList != null)
            {
                foreach (var relationshipInfo in productRelationshipInfoList)
                {
                    Item lookupItem = null;
                    var usingRelationshipName = string.IsNullOrWhiteSpace(relationshipInfo.RelationshipDescription);
                    var relationshipDescription = string.IsNullOrWhiteSpace(relationshipInfo.RelationshipDescription) ? LookupManager.GetRelationshipName(relationshipInfo.RelationshipName, out lookupItem) : relationshipInfo.RelationshipDescription;
                    RelationshipViewModel relationshipModel;
                    if (!relationshipGroups.TryGetValue(relationshipDescription, out relationshipModel))
                    {
                        relationshipModel = new RelationshipViewModel()
                        {
                            Name = relationshipInfo.RelationshipName,
                            Description = relationshipDescription,
                            Item = usingRelationshipName ? lookupItem : null
                        };

                        relationshipGroups[relationshipDescription] = relationshipModel;
                    }

                    var targetItemId = ID.Parse(relationshipInfo.ToItemExternalId);
                    var targetItem = field.InnerField.Database.GetItem(targetItemId);
                    var productModel = new ProductViewModel(targetItem);

                    CatalogManager.GetProductRating(targetItem);

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

            CatalogManager.GetProductBulkPrices(visitorContext, productViewModelList);
            InventoryManager.GetProductsStockStatusForList(storefront, productViewModelList);

            return relationshipGroups.Values;
        }

        private Category GetCurrentCategory()
        {
            var categoryId = CatalogItemContext.Current.CategoryId;
            var virtualCategoryCacheKey = $"VirtualCategory_{categoryId}";
            var currentCategory = this.GetFromCache<Category>(virtualCategoryCacheKey);
            if (currentCategory != null)
            {
                return currentCategory;
            }
            currentCategory = CatalogManager.GetCategory(categoryId, CatalogItemContext.Current.Catalog);
            return this.AddToCache(virtualCategoryCacheKey, currentCategory);
        }
    }
}