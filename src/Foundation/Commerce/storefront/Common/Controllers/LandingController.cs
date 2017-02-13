﻿//-----------------------------------------------------------------------
// <copyright file="LandingController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LandingController class.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Catalog.Fields;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.Search;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Models;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class LandingController : CSBaseController
    {
        private const string CurrentCategoryViewModelKeyName = "CurrentCategoryViewModel";
        private const string CurrentProductViewModelKeyName = "CurrentProductViewModel";

        public LandingController([NotNull] AccountManager accountManager, [NotNull] InventoryManager inventoryManager, [NotNull] CatalogManager catalogManager, [NotNull] ContactFactory contactFactory) : base(accountManager, contactFactory)
        {
            Assert.ArgumentNotNull(inventoryManager, "inventoryManager");
            Assert.ArgumentNotNull(catalogManager, "catalogManager");

            CatalogManager = catalogManager;
            InventoryManager = inventoryManager;
        }

        public InventoryManager InventoryManager { get; protected set; }
        public CatalogManager CatalogManager { get; }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult PromoRotator()
        {
            var datasource = CurrentRendering.DataSource;
            var datasourceItem = Context.Database.GetItem(ID.Parse(datasource));

            var item = Context.Item;
            var associatedItemIds = datasourceItem["Promotions"];
            var associatedItemIdArray = associatedItemIds.Split("|".ToCharArray());

            var viewModel = new PromoRotator(item);

            foreach (var associatedItemId in associatedItemIdArray)
            {
                if (!string.IsNullOrEmpty(associatedItemId))
                {
                    var associatedItem = Context.Database.GetItem(ID.Parse(associatedItemId));
                    var commercePromotion = new CommercePromotion(associatedItem);
                    viewModel.Promotions.Add(commercePromotion);
                }
            }

            return View(CurrentRenderingView, viewModel);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult PromoList()
        {
            var datasource = CurrentRendering.DataSource;
            var datasourceItem = Context.Database.GetItem(ID.Parse(datasource));

            if (datasourceItem == null)
            {
                return View(CurrentRenderingView, new PromoList());
            }

            var associatedItemIds = datasourceItem["Promotions"];
            var associatedItemIdArray = associatedItemIds.Split("|".ToCharArray());

            var viewModel = new PromoList(datasourceItem);

            foreach (var associatedItemId in associatedItemIdArray)
            {
                var commercePromotion = CreateCommercePromotionItem(associatedItemId);
                viewModel.Promotions.Add(commercePromotion);
            }

            return View(CurrentRenderingView, viewModel);
        }

        private static CommercePromotion CreateCommercePromotionItem(string associatedItemId)
        {
            var item = Context.Database.GetItem(associatedItemId);
            var sitecoreItem = (CommercePromotion) Activator.CreateInstance(typeof(CommercePromotion), item);
            var commercePromotion = sitecoreItem;
            return commercePromotion;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Promotion()
        {
            var datasource = CurrentRendering.DataSource;
            var datasourceItem = Context.Database.GetItem(ID.Parse(datasource));
            var commercePromotion = new CommercePromotion(datasourceItem);

            return View(CurrentRenderingView, commercePromotion);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Carousel()
        {
            return View(CurrentRenderingView);
        }

        protected CategorySearchResults GetChildCategories(CommerceSearchOptions searchOptions, Item categoryItem)
        {
            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalCategoryCount = 0;

            if (Item != null)
            {
                return CatalogManager.GetCategoryChildCategories(categoryItem.ID, searchOptions);
            }

            return new CategorySearchResults(returnList, totalCategoryCount, totalPageCount,
                searchOptions.StartPageIndex, new List<FacetCategory>());
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

            var productViewModel = new ProductViewModel();
            productViewModel.Initialize(rendering, variants);
            PopulateStockInformation(productViewModel);

            CurrentSiteContext.Items[CurrentProductViewModelKeyName] = productViewModel;

            return (ProductViewModel) CurrentSiteContext.Items[CurrentProductViewModelKeyName];
        }

        protected void PopulateStockInformation(ProductViewModel model)
        {
            var stockInfos =
                InventoryManager.GetStockInformation(CurrentStorefront,
                    new List<CommerceInventoryProduct>
                    {
                        new CommerceInventoryProduct {ProductId = model.ProductId, CatalogName = model.CatalogName}
                    },
                    StockDetailsLevel.Status).Result;
            var stockInfo = stockInfos.FirstOrDefault();
            if (stockInfo == null || stockInfo.Status == null)
            {
                return;
            }

            model.StockStatus = stockInfo.Status;
            InventoryManager.VisitedProductStockStatus(CurrentStorefront, stockInfo, string.Empty);
        }

        protected List<Item> GetRelationshipsFromField(RelationshipField field, string relationshipName)
        {
            List<Item> items = null;

            var relationships = field.GetRelationshipsTargets(relationshipName);

            if (relationships != null)
            {
                items = new List<Item>(relationships);
            }

            return items;
        }

        protected virtual LandingPageViewModel GetLandingPageViewModel(CommerceSearchOptions productSearchOptions,
            CommerceSearchOptions categorySearchOptions, IEnumerable<CommerceQuerySort> sortFields, Item item,
            Rendering rendering)
        {
            if (CurrentSiteContext.Items[CurrentCategoryViewModelKeyName] == null)
            {
                var categoryViewModel = new LandingPageViewModel(item);
                CurrentSiteContext.Items[CurrentCategoryViewModelKeyName] = categoryViewModel;
            }

            var viewModel = (LandingPageViewModel) CurrentSiteContext.Items[CurrentCategoryViewModelKeyName];
            return viewModel;
        }

        protected SearchResults GetChildProducts(CommerceSearchOptions searchOptions, Item categoryItem)
        {
            IEnumerable<CommerceQueryFacet> facets = null;
            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;

            if (Item != null)
            {
                SearchResponse searchResponse = null;
                if (CatalogUtility.IsItemDerivedFromCommerceTemplate(categoryItem,
                    CommerceConstants.KnownTemplateIds.CommerceDynamicCategoryTemplate))
                {
                    var defaultBucketQuery = categoryItem[CommerceConstants.KnownSitecoreFieldNames.DefaultBucketQuery];
                    var persistendBucketFilter =
                        categoryItem[CommerceConstants.KnownSitecoreFieldNames.PersistentBucketFilter];
                    persistendBucketFilter = CleanLanguageFromFilter(persistendBucketFilter);
                    searchResponse = CatalogManager.FindCatalogItems(defaultBucketQuery, persistendBucketFilter,
                        searchOptions);
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

            var results = new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex,
                facets);
            return results;
        }

        protected void UpdateOptionsWithFacets(IEnumerable<CommerceQueryFacet> facets, string valueQueryString,
            CommerceSearchOptions productSearchOptions)
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

                        var existingFacet =
                            facets.FirstOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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

        protected void UpdateOptionsWithSorting(string sortField, CommerceConstants.SortDirection? sortDirection,
            CommerceSearchOptions productSearchOptions)
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
    }
}