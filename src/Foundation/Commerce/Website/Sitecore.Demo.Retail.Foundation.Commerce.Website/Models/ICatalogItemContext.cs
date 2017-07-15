using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public interface ICatalogItemContext
    {
        CatalogItemType? ItemType { get; set; }
        string Id { get; set; }
        string Catalog { get; set; }
        string CategoryId { get; set; }
        Item Item { get; }
    }
}