using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.DynamicProxy.Generators.Emitters;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Feature.Commerce.Catalog.Services
{
    [Service]
    public class ProductOverlayImageService
    {
        public MediaItem GetProductOverlayImage(ProductViewModel product)
        {
            var productOverlaysItem = GetProductOverlayImagesItem(product.Item);
            if (productOverlaysItem == null)
                return null;

            if (product.IsOnSale)
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.OnSaleOverlayImage);
            if (product.StockStatus == StockStatus.OutOfStock)
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.OutOfStockOverlayImage);
            if (product.StockCount > 0 && product.StockCount < GetAlmostOutOfStockLimit(product.Item))
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.AlmostOutOfStockLimit);
            if (product.StockStatus == StockStatus.PreOrderable)
                return GetProductOverlayImage(productOverlaysItem, Templates.ProductOverlayImages.Fields.PreorderOverlayImage);
            return null;
        }

        private double GetAlmostOutOfStockLimit(Item productOverlaysItem)
        {
            if (!productOverlaysItem.FieldHasValue(Templates.ProductOverlayImages.Fields.AlmostOutOfStockLimit))
                return double.MaxValue;
            var returnValue = productOverlaysItem.GetInteger(Templates.ProductOverlayImages.Fields.AlmostOutOfStockLimit);
            return returnValue ?? double.MaxValue;
        }

        private MediaItem GetProductOverlayImage(Item productOverlaysItem, ID overlayFieldId)
        {
            return !productOverlaysItem.FieldHasValue(overlayFieldId) ? null : productOverlaysItem.Database.GetItem(new ID(productOverlaysItem[overlayFieldId]));
        }

        private Item GetProductOverlayImagesItem(Item contextItem)
        {
            var overlayImages = contextItem.GetAncestorOrSelfOfTemplate(Templates.ProductOverlayImages.ID);
            if (overlayImages != null)
                return overlayImages;
            return GetProductOverlayImagesItem(Sitecore.Context.Item) ?? GetProductOverlayImagesItem(Sitecore.Context.Site.GetStartItem());
        }
    }
}