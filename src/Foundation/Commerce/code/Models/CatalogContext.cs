using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Jobs.AsyncUI;

namespace Sitecore.Foundation.Commerce.Models
{
    public class CatalogContext
    {
        private Catalog _currentCatalog;

        public CatalogContext(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            AssignItemFromContext(item);
        }

        public CatalogContext()
        {
            AssignItemFromContext();
        }

        private void AssignItemFromContext(Item item = null)
        {
            var contextItem = item?.GetAncestorOrSelfOfTemplate(Templates.CatalogContext.ID);
            if (contextItem == null)
            {
                contextItem = Sitecore.Context.Item?.GetAncestorOrSelfOfTemplate(Templates.CatalogContext.ID);
                if (contextItem == null)
                {
                    contextItem = Sitecore.Context.Site?.GetStartItem()?.GetAncestorOrSelfOfTemplate(Templates.CatalogContext.ID);
                }
            }

            if (contextItem == null)
                throw new InvalidContextException($"Could not determing the catalog context from the item '{Sitecore.Context.Item}' or site '{Sitecore.Context.Site?.Name}'");

            Item = contextItem;
        }

        public Item Item { get; private set; }

        public Catalog[] GetCatalogs()
        {
            return ((MultilistField)Item.Fields[Templates.CatalogContext.Fields.Catalogs]).GetItems().Select(c => new Catalog(c)).ToArray();
        }

        private Catalog GetCurrentCatalog()
        {
            var catalogs = GetCatalogs();
            if (catalogs.Any())
            {
                return catalogs.First();
            }
            throw new ConfigurationErrorsException($"No catalogs is assigned to {Item.Paths.FullPath}");
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
    }
}