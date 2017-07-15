using System.Collections.Generic;
using Sitecore.Commerce.Entities.Inventory;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public interface IInventoryProduct
    {
        string ProductId { get; }
        string CatalogName { get; }
        StockStatus StockStatus { get; set; }
        string StockStatusName { get; set; }
        IEnumerable<IProductVariant> Variants { get; }
    }
}