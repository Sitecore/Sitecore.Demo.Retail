using System;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Catalog.Website.Models;
using Sitecore.Feature.Commerce.Catalog.Website.Services;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Feature.Commerce.Catalog.Website.Factories
{
    [Service]
    public class CategoryViewModelFactory
    {
        public CategoryViewModelFactory(GetChildProductsService getChildProductsService, CatalogManager catalogManager, InventoryManager inventoryManager)
        {
            GetChildProductsService = getChildProductsService;
            CatalogManager = catalogManager;
            InventoryManager = inventoryManager;
        }
        public CategoryViewModel Create(Item categoryItem)
        {
            if (categoryItem == null)
            {
                throw new ArgumentNullException(nameof(categoryItem));
            }

            if (!IsValid(categoryItem))
            {
                throw new ArgumentException("Invalid item type. Must be a category.", nameof(categoryItem));
            }

            var category = CatalogManager.GetCategory(categoryItem);
            return category == null ? null : Create(category);
        }

        private bool IsValid(Item categoryItem)
        {
            return categoryItem != null && categoryItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Category.Id);
        }

        public CategoryViewModel Create(Category category, SearchOptions productSearchOptions = null)
        {
            SearchResults childProducts = null;
            if (productSearchOptions != null)
            {
                childProducts = this.GetChildProductsService.GetChildProducts(category, productSearchOptions);
            }

            var categoryViewModel = new CategoryViewModel(category.InnerItem, childProducts, category.SortFields, productSearchOptions);

            if (childProducts != null && childProducts.SearchResultItems.Count > 0)
            {
                CatalogManager.GetProductBulkPrices(categoryViewModel.ChildProducts);
                this.InventoryManager.GetProductsStockStatusForList(categoryViewModel.ChildProducts);

                foreach (var productViewModel in categoryViewModel.ChildProducts)
                {
                    var productItem = childProducts.SearchResultItems.Single(item => item.Name == productViewModel.ProductId);
                    productViewModel.CustomerAverageRating = this.CatalogManager.GetProductRating(productItem);
                }
            }
            return categoryViewModel;
        }

        public InventoryManager InventoryManager { get; }

        public CatalogManager CatalogManager { get; }

        public GetChildProductsService GetChildProductsService { get; }
    }
}