using System.Collections.Generic;
using Sitecore.Commerce.Entities.Inventory;
using Sitecore.Foundation.Commerce.Managers;

namespace Sitecore.Foundation.Commerce.Models
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