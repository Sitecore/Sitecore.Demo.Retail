using Sitecore.Data.Items;

namespace Sitecore.Foundation.Commerce.Models
{
    public interface ICatalogContext
    {
        CatalogItemType? ItemType { get; set; }
        string Catalog { get; set; }
        string Id { get; set; }
        string CategoryId { get; set; }
        Item Item { get; }
    }
}