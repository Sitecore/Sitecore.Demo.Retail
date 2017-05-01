using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq.Expressions;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Feature.Commerce.Catalog.Factories;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Foundation.Indexing.Infrastructure;
using Sitecore.Foundation.Indexing.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Web.UI.WebControls;
using ISearchResult = Sitecore.Foundation.Indexing.Models.ISearchResult;

namespace Sitecore.Feature.Commerce.Catalog.Infrastructure.Provider
{
    public class IndexingProvider : ProviderBase, ISearchResultFormatter, IQueryPredicateProvider, IQueryRoot
    {
        public CatalogManager CatalogManager { get; }
        public ProductViewModelFactory ProductViewModelFactory { get; }
        public IndexingProvider(CatalogManager catalogManager, ProductViewModelFactory productViewModelFactory)
        {
            CatalogManager = catalogManager;
            ProductViewModelFactory = productViewModelFactory;
        }

        public string ContentType => DictionaryPhraseRepository.Current.Get("/Catalog/Search/Content Type", "Product Catalog");

        public IEnumerable<ID> SupportedTemplates => new[]
        {
            Foundation.Commerce.Templates.Commerce.Product.Id,
            Foundation.Commerce.Templates.Commerce.Category.Id
        };

        public Expression<Func<SearchResultItem, bool>> GetQueryPredicate(IQuery query)
        {
            if (this.CatalogManager.CatalogContext?.CurrentCatalog == null)
                return PredicateBuilder.False<SearchResultItem>();
            var catalogName = this.CatalogManager.CatalogContext.CurrentCatalog.Name;

            var fieldNames = new[]
            {
                Sitecore.ContentSearch.BuiltinFields.Name,
                Sitecore.ContentSearch.BuiltinFields.DisplayName,
                Sitecore.ContentSearch.BuiltinFields.Content
            };
            var predicate = GetFreeTextPredicateService.GetFreeTextPredicate(fieldNames, query);
            return predicate.And(i => i[Constants.IndexFields.CatalogName] == catalogName);
        }

        public void FormatResult(SearchResultItem item, ISearchResult formattedResult)
        {
            var contentItem = item.GetItem();
            if (contentItem.IsDerived(Foundation.Commerce.Templates.Commerce.Product.Id))
            {
                var product = ProductViewModelFactory.Create(contentItem);
                formattedResult.Title = product.ProductName;
                formattedResult.Description = product.Description;
                formattedResult.ViewName = "~/Views/Catalog/_ProductSearchResult.cshtml";
            }
            if (contentItem.IsDerived(Foundation.Commerce.Templates.Commerce.Category.Id))
            {
                formattedResult.Title = contentItem.DisplayName;
                formattedResult.Description = FieldRenderer.Render(contentItem, Templates.Generated.Category.Fields.Description);
                formattedResult.ViewName = "~/Views/Catalog/_CategorySearchResult.cshtml";
            }
        }

        public Item Root
        {
            get
            {
                var catalogRootItem = CatalogManager.CatalogContext?.CatalogRootItem;
                return catalogRootItem?.TargetItem(Foundation.Commerce.Templates.Commerce.NavigationItem.Fields.CategoryDatasource);
            }
            set { throw new NotImplementedException(); }
        }
    }
}