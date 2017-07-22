using Foundation.Commerce.Website.Models;
using Sitecore.Data.Items;

namespace Feature.Catalog.Website.Models
{
    public class CatalogRouteData : ICatalogItemContext
    {
        public CatalogItemType? ItemType { get; set; }
        public string Id { get; set; }
        public string Catalog { get; set; }
        public string CategoryId { get; set; }
        public Item Item { get; set; }
    }
}