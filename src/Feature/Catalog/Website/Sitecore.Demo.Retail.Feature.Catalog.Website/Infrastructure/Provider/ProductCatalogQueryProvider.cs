using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq.Expressions;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Indexing.Infrastructure;
using Sitecore.Foundation.Indexing.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Infrastructure.Provider
{
    public class ProductCatalogQueryProvider : ProviderBase, IQueryPredicateProvider, IQueryRoot
    {
        private CatalogManager CatalogManager { get; }
        public ProductCatalogQueryProvider(CatalogManager catalogManager)
        {
            CatalogManager = catalogManager;
        }

        public IEnumerable<ID> SupportedTemplates => new[]
        {
            Foundation.Commerce.Website.Templates.Commerce.Product.Id,
            Foundation.Commerce.Website.Templates.Commerce.Category.Id
        };

        public Expression<Func<SearchResultItem, bool>> GetQueryPredicate(IQuery query)
        {
            if (this.CatalogManager.CatalogContext?.CurrentCatalog == null)
                return PredicateBuilder.False<SearchResultItem>();
            var catalogName = this.CatalogManager.CatalogContext.CurrentCatalog.Name;

            var fieldNames = new[]
            {
                ContentSearch.BuiltinFields.Name,
                ContentSearch.BuiltinFields.DisplayName,
                ContentSearch.BuiltinFields.Content
            };
            var predicate = GetFreeTextPredicateService.GetFreeTextPredicate(fieldNames, query);
            return predicate.And(i => i[Constants.IndexFields.CatalogName] == catalogName);
        }

        public Item Root
        {
            get
            {
                var catalogRootItem = CatalogManager.CatalogContext?.CatalogRootItem;
                return catalogRootItem?.TargetItem(Foundation.Commerce.Website.Templates.Commerce.NavigationItem.Fields.CategoryDatasource);
            }
            set { throw new NotImplementedException(); }
        }

    }
}