using System;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Feature.Commerce.Catalog.Website.Services
{
    [Service]
    public class ProductOverlayImageService
    {
        public MediaItem GetProductOverlayImage(ProductViewModel product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            Item productOverlaysItem = GetProductOverlayImagesItemFromProductOrContext(product);
            if (productOverlaysItem == null)
            {
                Log.Warn($"Could not find ProductOverlayImages content for product {product.ProductId} and site {Context.Site.Name}", this);
                return null;
            }

            if (product.IsOnSale)
            {
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.OnSaleOverlayImage);
            }
            if (product.StockStatus == StockStatus.OutOfStock)
            {
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.OutOfStockOverlayImage);
            }
            if (product.StockCount > 0 && product.StockCount < GetAlmostOutOfStockLimit(product.Item))
            {
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.AlmostOutOfStockLimit);
            }
            if (product.StockStatus == StockStatus.PreOrderable)
            {
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.PreorderOverlayImage);
            }
            return null;
        }

        private Item GetProductOverlayImagesItemFromProductOrContext(ProductViewModel product)
        {
            var productOverlaysItem = GetProductOverlayImagesItem(product.Item);
            if (productOverlaysItem != null)
            {
                return productOverlaysItem;
            }

            productOverlaysItem = GetProductOverlayImagesItemFromContext(product, Context.Item);
            return productOverlaysItem ?? GetProductOverlayImagesItemFromContext(product, Context.Site.GetStartItem());
        }

        private Item GetProductOverlayImagesItemFromContext(ProductViewModel product, Item contextItem)
        {
            if (contextItem == null || product.Item.Database != contextItem.Database)
                return null;
            if (product.Item.ID != contextItem.ID && !contextItem.Axes.IsAncestorOf(product.Item))
            {
                return GetProductOverlayImagesItem(contextItem);
            }
            return null;
        }

        private double GetAlmostOutOfStockLimit(Item productOverlaysItem)
        {
            if (!productOverlaysItem.FieldHasValue(Templates.ProductOverlayImages.Fields.AlmostOutOfStockLimit))
            {
                return double.MaxValue;
            }
            var returnValue = productOverlaysItem.GetInteger(Templates.ProductOverlayImages.Fields.AlmostOutOfStockLimit);
            return returnValue ?? double.MaxValue;
        }

        private MediaItem GetProductOverlayImage(Item productOverlaysItem, ID overlayFieldId)
        {
            return !productOverlaysItem.FieldHasValue(overlayFieldId) ? null : productOverlaysItem.Database.GetItem(new ID(productOverlaysItem[overlayFieldId]));
        }

        private Item GetProductOverlayImagesItem(Item contextItem)
        {
            return contextItem.GetAncestorOrSelfOfTemplate(Templates.ProductOverlayImages.ID);
        }
    }
}