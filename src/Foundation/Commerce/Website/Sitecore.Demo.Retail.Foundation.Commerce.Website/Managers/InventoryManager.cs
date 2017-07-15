//-----------------------------------------------------------------------
// <copyright file="InventoryManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the inventory business logic for the site.</summary>
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
using System.Linq.Expressions;
using Sitecore.Commerce.Connect.CommerceServer.Inventory;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Commerce.Multishop;
using Sitecore.Commerce.Services.Inventory;
using Sitecore.Configuration;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.Search;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Dictionary.Repositories;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
    public class InventoryManager : IManager
    {
        private readonly CommerceContextBase _obecContext;

        public InventoryManager(InventoryServiceProvider inventoryServiceProvider, ContactFactory contactFactory, ICommerceSearchManager commerceSearchManager, StorefrontContext storefrontContext)
        {
            Assert.ArgumentNotNull(inventoryServiceProvider, nameof(inventoryServiceProvider));
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            InventoryServiceProvider = inventoryServiceProvider;
            ContactFactory = contactFactory;
            _obecContext = (CommerceContextBase) Factory.CreateObject("commerceContext", true);
            CommerceSearchManager = commerceSearchManager;
            StorefrontContext = storefrontContext;
        }

        private StorefrontContext StorefrontContext { get; }

        private ICommerceSearchManager CommerceSearchManager { get; }

        private InventoryServiceProvider InventoryServiceProvider { get; }

        private ContactFactory ContactFactory { get; }

        private bool UseIndexFileForProductStatusInLists => Settings.GetBoolSetting("Storefront.UseIndexFileForProductStatusInLists", false);

        public void GetProductsStockStatusForList(IEnumerable<IInventoryProduct> productViewModels)
        {
            GetProductsStockStatus(productViewModels);
        }

        private void GetProductStockStatusFromIndex(IEnumerable<IInventoryProduct> viewModelList)
        {
            var products = viewModelList as IInventoryProduct[] ?? viewModelList.ToArray();
            if (!products.Any())
            {
                return;
            }

            var searchIndex = CommerceSearchManager.GetIndex();

            using (var context = searchIndex.CreateSearchContext())
            {
                var predicate = PredicateBuilder.Create<InventorySearchResultItem>(item => item[Constants.CommerceIndex.Fields.InStockLocations].Contains("Default"));
                predicate = predicate.Or(item => item[Constants.CommerceIndex.Fields.OutOfStockLocations].Contains("Default"));
                predicate = predicate.Or(item => item[Constants.CommerceIndex.Fields.OrderableLocations].Contains("Default"));
                predicate = predicate.Or(item => item[Constants.CommerceIndex.Fields.PreOrderable].Contains("0"));

                var searchResults = context.GetQueryable<InventorySearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Product)
                    .Where(item => item.Language == Context.Language.Name)
                    .Where(BuildProductIdListPredicate(products))
                    .Where(predicate)
                    .Select(x => new {x.OutOfStockLocations, x.OrderableLocations, x.PreOrderable, x.InStockLocations, x.Fields, x.Name});

                var results = searchResults.GetResults();
                if (results.TotalSearchResults == 0)
                {
                    return;
                }

                foreach (var result in results)
                {
                    var resultDocument = result.Document;
                    if (resultDocument == null)
                    {
                        continue;
                    }

                    StockStatus status;

                    var isInStock = resultDocument.Fields.ContainsKey(Constants.CommerceIndex.Fields.InStockLocations)
                                    && resultDocument.Fields[Constants.CommerceIndex.Fields.InStockLocations] != null;
                    if (isInStock)
                    {
                        status = StockStatus.InStock;
                    }
                    else
                    {
                        var isPreOrderable = resultDocument.Fields.ContainsKey(Constants.CommerceIndex.Fields.PreOrderable)
                                             && result.Document.PreOrderable != null
                                             && (result.Document.PreOrderable.Equals("1", StringComparison.OrdinalIgnoreCase)
                                                 || result.Document.PreOrderable.Equals("true", StringComparison.OrdinalIgnoreCase));
                        if (isPreOrderable)
                        {
                            status = StockStatus.PreOrderable;
                        }
                        else
                        {
                            var isOutOfStock = resultDocument.Fields.ContainsKey(Constants.CommerceIndex.Fields.OutOfStockLocations)
                                               && result.Document.OutOfStockLocations != null;
                            var isBackOrderable = resultDocument.Fields.ContainsKey(Constants.CommerceIndex.Fields.OrderableLocations)
                                                  && result.Document.OrderableLocations != null;
                            if (isOutOfStock && isBackOrderable)
                            {
                                status = StockStatus.BackOrderable;
                            }
                            else
                            {
                                status = isOutOfStock ? StockStatus.OutOfStock : null;
                            }
                        }
                    }

                    var foundModel = products.FirstOrDefault(x => x.ProductId == result.Document.Name);
                    if (foundModel == null)
                    {
                        continue;
                    }

                    foundModel.StockStatus = status;
                    foundModel.StockStatusName = GetStockStatusName(foundModel.StockStatus);
                }
            }
        }

        private static Expression<Func<InventorySearchResultItem, bool>> BuildProductIdListPredicate(IEnumerable<IInventoryProduct> viewModelList)
        {
            Expression<Func<InventorySearchResultItem, bool>> predicate = null;

            var isFirst = true;
            foreach (var viewModel in viewModelList)
            {
                if (isFirst)
                {
                    predicate = PredicateBuilder.Create<InventorySearchResultItem>(p => p.CatalogItemId == viewModel.ProductId.ToLowerInvariant());
                }
                else
                {
                    predicate = predicate.Or(p => p.CatalogItemId == viewModel.ProductId.ToLowerInvariant());
                }

                isFirst = false;
            }

            return predicate;
        }


        public void GetProductsStockStatus(IEnumerable<IInventoryProduct> productViewModels)
        {
            if (productViewModels == null || !productViewModels.Any())
            {
                return;
            }

            var products = new List<CommerceInventoryProduct>();
            foreach (var viewModel in productViewModels)
            {
                if (viewModel.Variants != null && viewModel.Variants.Any())
                {
                    foreach (var variant in viewModel.Variants)
                    {
                        products.Add(new CommerceInventoryProduct
                        {
                            ProductId = viewModel.ProductId,
                            CatalogName = viewModel.CatalogName,
                            VariantId = variant.VariantId
                        });
                    }
                }
                else
                {
                    products.Add(new CommerceInventoryProduct {ProductId = viewModel.ProductId, CatalogName = viewModel.CatalogName});
                }
            }

            if (products.Any())
            {
                var response = GetStockInformation(products, StockDetailsLevel.All);
                if (response.Result != null)
                {
                    var stockInfoList = response.Result.ToList();

                    foreach (var viewModel in productViewModels)
                    {
                        StockInformation foundItem = null;
                        if (viewModel.Variants != null && viewModel.Variants.Any())
                        {
                            foreach (var variant in viewModel.Variants)
                            {
                                foundItem = stockInfoList.Find(p => p.Product.ProductId == viewModel.ProductId && ((CommerceInventoryProduct) p.Product).VariantId == variant.VariantId);
                            }
                        }
                        else
                        {
                            foundItem = stockInfoList.Find(p => p.Product.ProductId == viewModel.ProductId);
                        }

                        if (foundItem != null)
                        {
                            viewModel.StockStatus = foundItem.Status;
                            viewModel.StockStatusName = GetStockStatusName(foundItem.Status);
                        }
                    }
                }
            }
        }

        public ManagerResponse<GetStockInformationResult, IEnumerable<StockInformation>> GetStockInformation(IEnumerable<InventoryProduct> products, StockDetailsLevel detailsLevel)
        {
            Assert.ArgumentNotNull(products, nameof(products));
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new GetStockInformationRequest(StorefrontContext.Current.ShopName, products, detailsLevel) {Location = _obecContext.InventoryLocation, VisitorId = ContactFactory.GetContact()};
            var result = InventoryServiceProvider.GetStockInformation(request);

            // Currently, both Categories and Products are passed in and are waiting for a fix to filter the categories out.  Until then, this code is commented
            // out as it generates an unecessary Error event indicating the product cannot be found.
            // result.WriteToSitecoreLog();
            return new ManagerResponse<GetStockInformationResult, IEnumerable<StockInformation>>(result, result.StockInformation ?? new List<StockInformation>());
        }

        public ManagerResponse<GetPreOrderableInformationResult, IEnumerable<OrderableInformation>> GetPreOrderableInformation(IEnumerable<InventoryProduct> products)
        {
            Assert.ArgumentNotNull(products, nameof(products));

            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new GetPreOrderableInformationRequest(StorefrontContext.Current.ShopName, products);
            var result = InventoryServiceProvider.GetPreOrderableInformation(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetPreOrderableInformationResult, IEnumerable<OrderableInformation>>(result, !result.Success || result.OrderableInformation == null ? new List<OrderableInformation>() : result.OrderableInformation);
        }

        public ManagerResponse<GetBackOrderableInformationResult, IEnumerable<OrderableInformation>> GetBackOrderableInformation(IEnumerable<InventoryProduct> products)
        {
            Assert.ArgumentNotNull(products, nameof(products));
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new GetBackOrderableInformationRequest(StorefrontContext.Current.ShopName, products);
            var result = InventoryServiceProvider.GetBackOrderableInformation(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetBackOrderableInformationResult, IEnumerable<OrderableInformation>>(result, !result.Success || result.OrderableInformation == null ? new List<OrderableInformation>() : result.OrderableInformation);
        }

        public ManagerResponse<VisitedProductStockStatusResult, bool> VisitedProductStockStatus(StockInformation stockInformation, string location)
        {
            Assert.ArgumentNotNull(stockInformation, nameof(stockInformation));

            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var request = new VisitedProductStockStatusRequest(StorefrontContext.Current.ShopName, stockInformation) {Location = location};
            var result = InventoryServiceProvider.VisitedProductStockStatus(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<VisitedProductStockStatusResult, bool>(result, result.Success);
        }

        public ManagerResponse<VisitorSignUpForStockNotificationResult, bool> VisitorSignupForStockNotification(SignUpForNotificationInputModel model, string location)
        {
            Assert.ArgumentNotNull(model, nameof(model));
            Assert.ArgumentNotNullOrEmpty(model.ProductId, nameof(model.ProductId));
            Assert.ArgumentNotNullOrEmpty(model.Email, nameof(model.Email));
            if (StorefrontContext.Current == null)
            {
                throw new InvalidOperationException("Cannot be called without a valid storefront context.");
            }

            var visitorId = ContactFactory.GetContact();
            var builder = new CommerceInventoryProductBuilder();
            var inventoryProduct = (CommerceInventoryProduct) builder.CreateInventoryProduct(model.ProductId);
            if (string.IsNullOrEmpty(model.VariantId))
            {
                inventoryProduct.VariantId = model.VariantId;
            }

            if (string.IsNullOrEmpty(inventoryProduct.CatalogName))
            {
                inventoryProduct.CatalogName = model.CatalogName;
            }

            DateTime interestDate;
            var isDate = DateTime.TryParse(model.InterestDate, out interestDate);
            var request = new VisitorSignUpForStockNotificationRequest(StorefrontContext.Current.ShopName, visitorId, model.Email, inventoryProduct) {Location = location};
            if (isDate)
            {
                request.InterestDate = interestDate;
            }

            var result = InventoryServiceProvider.VisitorSignUpForStockNotification(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<VisitorSignUpForStockNotificationResult, bool>(result, result.Success);
        }

        public static string GetStockStatusName(StockStatus modelStockStatus)
        {
            return DictionaryPhraseRepository.Current.Get($"/Commerce/Stock Status/{modelStockStatus.Name}", $"[{modelStockStatus.Name}]");
        }
    }
}