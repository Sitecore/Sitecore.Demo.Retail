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
    public class ProductSearchResultFormatter : ProviderBase, ISearchResultFormatter
    {
        private ProductViewModelFactory ProductViewModelFactory { get; }
        public ProductSearchResultFormatter(ProductViewModelFactory productViewModelFactory)
        {
            ProductViewModelFactory = productViewModelFactory;
        }

        public string ContentType => DictionaryPhraseRepository.Current.Get("/Catalog/Search/Product Content Type", "Product");

        public IEnumerable<ID> SupportedTemplates => new[]
        {
            Foundation.Commerce.Templates.Commerce.Product.Id
        };

        public void FormatResult(SearchResultItem item, ISearchResult formattedResult)
        {
            var product = ProductViewModelFactory.Create(item.GetItem());
            formattedResult.Title = product.ProductName;
            formattedResult.Description = product.Description;
            formattedResult.Media = product.DefaultImage;
            formattedResult.ViewName = "~/Views/Catalog/_ProductSearchResult.cshtml";
        }
    }
}