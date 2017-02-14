//-----------------------------------------------------------------------
// <copyright file="WishListManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>The manager class responsible for encapsulating the wishlist business logic for the site.</summary>
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
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Commerce.Services.WishLists;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class WishListManager : BaseManager
    {
        public WishListManager([NotNull] WishListServiceProvider wishListServiceProvider, [NotNull] AccountManager accountManager, [NotNull] InventoryManager inventoryManager)
        {
            Assert.ArgumentNotNull(wishListServiceProvider, nameof(wishListServiceProvider));
            Assert.ArgumentNotNull(accountManager, nameof(accountManager));
            Assert.ArgumentNotNull(inventoryManager, nameof(inventoryManager));

            WishListServiceProvider = wishListServiceProvider;
            AccountManager = accountManager;
            InventoryManager = inventoryManager;
        }

        public WishListServiceProvider WishListServiceProvider { get; protected set; }

        public AccountManager AccountManager { get; protected set; }

        public InventoryManager InventoryManager { get; protected set; }

        public virtual ManagerResponse<CreateWishListResult, WishList> CreateWishList([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] string wishListName)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(wishListName, nameof(wishListName));

            var request = new CreateWishListRequest(visitorContext.UserId, wishListName, storefront.ShopName);
            var result = WishListServiceProvider.CreateWishList(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<CreateWishListResult, WishList>(result, result.WishList);
        }

        public virtual ManagerResponse<GetWishListResult, WishList> GetWishList([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string wishListId)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(wishListId, nameof(wishListId));

            var request = new GetWishListRequest(visitorContext.UserId, wishListId, storefront.ShopName);
            var result = WishListServiceProvider.GetWishList(request);
            if (result.Success && result.WishList != null)
            {
                PopulateStockInformation(storefront, result.WishList);
            }

            result.WriteToSitecoreLog();

            return new ManagerResponse<GetWishListResult, WishList>(result, result.WishList);
        }

        public virtual ManagerResponse<GetWishListsResult, IEnumerable<WishListHeader>> GetWishLists([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));

            var request = new GetWishListsRequest(visitorContext.UserId, storefront.ShopName);
            var result = WishListServiceProvider.GetWishLists(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<GetWishListsResult, IEnumerable<WishListHeader>>(result, result.WishLists.ToList());
        }

        public virtual ManagerResponse<DeleteWishListResult, WishList> DeleteWishList([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string wishListId)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNullOrEmpty(wishListId, nameof(wishListId));

            var request = new DeleteWishListRequest(new WishList {UserId = visitorContext.UserId, CustomerId = visitorContext.UserId, ExternalId = wishListId, ShopName = storefront.ShopName});
            var result = WishListServiceProvider.DeleteWishList(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<DeleteWishListResult, WishList>(result, result.WishList);
        }

        public virtual ManagerResponse<RemoveWishListLinesResult, WishList> RemoveWishListLines([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string wishListId, IEnumerable<WishListLineInputModel> models)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(models, nameof(models));
            Assert.ArgumentNotNullOrEmpty(wishListId, nameof(wishListId));

            var lineIds = models.Select(model => model.ExternalLineId).ToList();
            var request = new RemoveWishListLinesRequest(new WishList {UserId = visitorContext.UserId, CustomerId = visitorContext.UserId, ExternalId = wishListId, ShopName = storefront.ShopName}, lineIds);
            var result = WishListServiceProvider.RemoveWishListLines(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<RemoveWishListLinesResult, WishList>(result, result.WishList);
        }

        public virtual ManagerResponse<UpdateWishListLinesResult, WishList> UpdateWishListLines([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string wishListId, IEnumerable<WishListLine> lines)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(lines, nameof(lines));
            Assert.ArgumentNotNullOrEmpty(wishListId, nameof(wishListId));

            var request = new UpdateWishListLinesRequest(new WishList {UserId = visitorContext.UserId, CustomerId = visitorContext.UserId, ExternalId = wishListId, ShopName = storefront.ShopName}, lines);
            var result = WishListServiceProvider.UpdateWishListLines(request);
            if (result.Success && result.WishList != null)
            {
                PopulateStockInformation(storefront, result.WishList);
            }
            result.WriteToSitecoreLog();

            return new ManagerResponse<UpdateWishListLinesResult, WishList>(result, result.WishList);
        }

        public virtual ManagerResponse<UpdateWishListLinesResult, WishList> UpdateWishListLine([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, [NotNull] WishListLineInputModel model)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(model, nameof(model));
            Assert.ArgumentNotNullOrEmpty(model.WishListId, nameof(model.WishListId));
            Assert.ArgumentNotNullOrEmpty(model.ProductId, nameof(model.ProductId));

            var wishListLine = new WishListLine
            {
                Product = new CommerceCartProduct {ProductId = model.ProductId, ProductVariantId = model.VariantId},
                Quantity = model.Quantity
            };

            return UpdateWishListLines(storefront, visitorContext, model.WishListId, new List<WishListLine> {wishListLine});
        }

        public virtual ManagerResponse<AddLinesToWishListResult, WishList> AddLinesToWishList([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, string wishListId, IEnumerable<WishListLine> lines)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(lines, nameof(lines));
            Assert.ArgumentNotNullOrEmpty(wishListId, nameof(wishListId));

            var request = new AddLinesToWishListRequest(new WishList {UserId = visitorContext.UserId, CustomerId = visitorContext.UserId, ExternalId = wishListId, ShopName = storefront.ShopName}, lines);
            var result = WishListServiceProvider.AddLinesToWishList(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<AddLinesToWishListResult, WishList>(result, result.WishList);
        }

        public virtual ManagerResponse<AddLinesToWishListResult, WishList> AddLinesToWishList([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, AddToWishListInputModel model)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(model, nameof(model));

            var product = new CommerceCartProduct
            {
                ProductCatalog = model.ProductCatalog,
                ProductId = model.ProductId,
                ProductVariantId = model.VariantId
            };

            var line = new WishListLine
            {
                Product = product,
                Quantity = model.Quantity == null ? 1 : (uint) model.Quantity
            };

            if (line.Product.ProductId.Equals(storefront.GiftCardProductId, StringComparison.OrdinalIgnoreCase))
            {
                line.Properties.Add("GiftCardAmount", model.GiftCardAmount);
            }

            // create wish list
            if (model.WishListId == null && !string.IsNullOrEmpty(model.WishListName))
            {
                var newList = CreateWishList(storefront, visitorContext, model.WishListName).Result;
                if (newList == null)
                {
                    return new ManagerResponse<AddLinesToWishListResult, WishList>(new AddLinesToWishListResult {Success = false}, null);
                }

                model.WishListId = newList.ExternalId;
            }

            var result = AddLinesToWishList(storefront, visitorContext, model.WishListId, new List<WishListLine> {line});

            return new ManagerResponse<AddLinesToWishListResult, WishList>(result.ServiceProviderResult, result.ServiceProviderResult.WishList);
        }

        public virtual ManagerResponse<UpdateWishListResult, WishList> UpdateWishList([NotNull] CommerceStorefront storefront, [NotNull] VisitorContext visitorContext, UpdateWishListInputModel model)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));
            Assert.ArgumentNotNull(model, nameof(model));

            var request = new UpdateWishListRequest(new WishList {UserId = visitorContext.UserId, CustomerId = visitorContext.UserId, ExternalId = model.ExternalId, Name = model.Name, IsFavorite = model.IsFavorite, ShopName = storefront.ShopName});
            var result = WishListServiceProvider.UpdateWishList(request);
            result.WriteToSitecoreLog();

            return new ManagerResponse<UpdateWishListResult, WishList>(result, result.WishList);
        }

        protected virtual void PopulateStockInformation(CommerceStorefront storefront, WishList wishList)
        {
            var productList = wishList.Lines.Select(line => new CommerceInventoryProduct {ProductId = line.Product.ProductId, CatalogName = ((CommerceCartProduct) line.Product).ProductCatalog}).ToList();

            var stockInfos = InventoryManager.GetStockInformation(storefront, productList, StockDetailsLevel.Status).Result;
            if (stockInfos == null)
            {
                return;
            }

            foreach (var line in wishList.Lines)
            {
                var stockInfo = stockInfos.ToList().FirstOrDefault(si => si.Product.ProductId.Equals(line.Product.ProductId, StringComparison.OrdinalIgnoreCase));
                if (stockInfo == null)
                {
                    continue;
                }

                line.Product.StockStatus = new StockStatus(System.Convert.ToInt32((decimal) stockInfo.Properties["OnHandQuantity"]), stockInfo.Status.Name);
                InventoryManager.VisitedProductStockStatus(storefront, stockInfo, string.Empty);
            }
        }
    }
}