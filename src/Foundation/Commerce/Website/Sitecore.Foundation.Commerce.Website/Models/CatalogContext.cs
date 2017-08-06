using System.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Website.Models
{
    public class CatalogContext
    {
        private Catalog _currentCatalog;

        private CatalogContext(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Item = item;
        }

        public static CatalogContext CreateFromContext()
        {
            var contextItem = Sitecore.Context.Item?.GetAncestorOrSelfOfTemplate(Templates.CatalogContext.Id);
            if (contextItem == null)
            {
                contextItem = Sitecore.Context.Site?.GetStartItem()?.GetAncestorOrSelfOfTemplate(Templates.CatalogContext.Id);
            }

            if (contextItem == null)
                return null;

            return new CatalogContext(contextItem);
        }

        public Item Item { get; private set; }

        private Catalog GetCurrentCatalog()
        {
            var catalogItem = ((ReferenceField) Item.Fields[Templates.CatalogContext.Fields.Catalog]).TargetItem;
            if (catalogItem == null || !catalogItem.IsDerived(Templates.Commerce.Catalog.Id))
            {
                throw new ConfigurationErrorsException($"No catalog is assigned or the assigned item is not a catalog on '{Item.Paths.FullPath}'");
            }
            return new Catalog(catalogItem);
        }

        public Catalog CurrentCatalog
        {
            get
            {
                if (_currentCatalog != null)
                {
                    return _currentCatalog;
                }

                return _currentCatalog = GetCurrentCatalog();
            }
        }

        public Item CatalogRootItem
        {
            get
            {
                var catalogRootItem = ((ReferenceField)Item.Fields[Templates.CatalogContext.Fields.CatalogRoot]).TargetItem;
                if (catalogRootItem == null || !catalogRootItem.IsDerived(Templates.Commerce.NavigationItem.Id))
                {
                    throw new ConfigurationErrorsException($"No catalog root is assigned or the assigned root is not a valid type '{Item.Paths.FullPath}'");
                }
                return catalogRootItem;
            }
        }
    }
}