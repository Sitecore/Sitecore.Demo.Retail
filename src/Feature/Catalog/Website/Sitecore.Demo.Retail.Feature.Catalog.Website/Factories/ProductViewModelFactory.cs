using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Connect.CommerceServer.Inventory.Models;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Feature.Catalog.Website.Models;
using Sitecore.Demo.Retail.Feature.Catalog.Website.Services;
using Sitecore.Demo.Retail.Foundation.Commerce.Website;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Factories
{
    [Service]
    public class ProductViewModelFactory
    {
        public ProductViewModelFactory(CatalogItemContext catalogItemContext, CatalogManager catalogManager, InventoryManager inventoryManager, CommerceUserContext commerceUserContext, StorefrontContext storefrontContext, ProductOverlayImageService productOverlayImageService)
        {
            CatalogItemContext = catalogItemContext;
            CatalogManager = catalogManager;
            InventoryManager = inventoryManager;
            CommerceUserContext = commerceUserContext;
            StorefrontContext = storefrontContext;
            ProductOverlayImageService = productOverlayImageService;
        }

        public CatalogItemContext CatalogItemContext { get; }
        private CatalogManager CatalogManager { get; }
        private InventoryManager InventoryManager { get; }
        private CommerceUserContext CommerceUserContext { get; }
        private StorefrontContext StorefrontContext { get; }
        private ProductOverlayImageService ProductOverlayImageService { get; }

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

            PopulateCategoryInformation(productViewModel);
            PopulateStockInformation(productViewModel);
            PopulatePriceInformation(productViewModel);
            PopulateRatings(productViewModel);
            PopulateImages(productViewModel);

            return AddToCache(cacheKey, productViewModel);
        }

        private void PopulateImages(ProductViewModel productViewModel)
        {
            productViewModel.OverlayImage = this.ProductOverlayImageService.GetProductOverlayImage(productViewModel);
        }

        private void PopulateRatings(ProductViewModel productViewModel)
        {
            productViewModel.CustomerAverageRating = CatalogManager.GetProductRating(productViewModel.Item);
        }

        private void PopulatePriceInformation(ProductViewModel productViewModel)
        {
            CatalogManager.GetProductPrice(productViewModel);
        }

        private void PopulateCategoryInformation(ProductViewModel productViewModel)
        {
            if (CatalogItemContext.Current == null)
            {
                return;
            }

            productViewModel.ParentCategoryId = CatalogItemContext.Current.CategoryId;
            var category = CatalogManager.GetCategory(productViewModel.ParentCategoryId);
            if (category != null)
            {
                productViewModel.ParentCategoryName = category.Title;
            }
        }

        public bool IsValid(Item item)
        {
            if (item == null)
                return false;
            if (item.IsDerived(Foundation.Commerce.Website.Templates.Commerce.Product.Id))
                return true;
            return item.IsWildcardItem() && IsValid(CatalogItemContext.Current?.Item);
        }

        private void PopulateStockInformation(ProductViewModel model)
        {
            if (StorefrontContext.Current == null)
                return;

            var inventoryProducts = new List<CommerceInventoryProduct> { new CommerceInventoryProduct { ProductId = model.ProductId, CatalogName = model.CatalogName } };
            var response = InventoryManager.GetStockInformation(inventoryProducts, StockDetailsLevel.StatusAndAvailability);
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
            model.StockStatusName = InventoryManager.GetStockStatusName(model.StockStatus);
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