using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Factories
{
    public class ProductViewModelFactory
    {
        public ProductViewModelFactory(CatalogItemContext catalogItemContext, CatalogManager catalogManager, InventoryManager inventoryManager, VisitorContextRepository visitorContextRepository)
        {
            CatalogItemContext = catalogItemContext;
            CatalogManager = catalogManager;
            InventoryManager = inventoryManager;
            VisitorContextRepository = visitorContextRepository;
        }

        private CatalogItemContext CatalogItemContext { get; }
        public CatalogManager CatalogManager { get; }
        public InventoryManager InventoryManager { get; }
        public VisitorContextRepository VisitorContextRepository { get; }

        public ProductViewModel CreateFromCatalogItemContext()
        {
            if (CatalogItemContext.IsCategory)
                return null;

            var productItem = CatalogItemContext.Current?.Item;
            if (productItem == null)
            {
                return null;
            }

            RenderingContext.Current.Rendering.Item = productItem;
            return Create(RenderingContext.Current.Rendering.Item);
        }

        public ProductViewModel Create(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!IsValid(item))
            {
                throw new ArgumentException("Invalid item type. Must be a product.", nameof(item));
            }

            if (item.IsWildcardItem())
            {
                return CreateFromCatalogItemContext();
            }

            var cacheKey = $"CurrentProductViewModel{item.ID}";
            var productViewModel = GetFromCache(cacheKey);
            if (productViewModel != null)
            {
                return productViewModel;
            }

            var variants = item.Children.Select(c => new VariantViewModel(c)).ToList();

            productViewModel = new ProductViewModel(item, variants);
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
            productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(item);

            return AddToCache(cacheKey, productViewModel);
        }

        public bool IsValid(Item item)
        {
            if (item == null)
                return false;
            if (item.IsDerived(Foundation.Commerce.Templates.Commerce.Product.Id))
                return true;
            return item.IsWildcardItem() && IsValid(CatalogItemContext.Current?.Item);
        }

        private void PopulateStockInformation(ProductViewModel model)
        {
            var inventoryProducts = new List<CommerceInventoryProduct> { new CommerceInventoryProduct { ProductId = model.ProductId, CatalogName = model.CatalogName } };
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


        public static ProductViewModel GetFromCache(string cacheKey)
        {
            return HttpContext.Current?.Items[cacheKey] as ProductViewModel;
        }

        public static ProductViewModel AddToCache(string cacheKey, ProductViewModel value)
        {
            if (HttpContext.Current == null)
                return value;
            if (HttpContext.Current.Items.Contains(cacheKey))
            {
                HttpContext.Current.Items[cacheKey] = value;
            }
            else
            {
                HttpContext.Current.Items.Add(cacheKey, value);
            }
            return value;
        }
    }
}