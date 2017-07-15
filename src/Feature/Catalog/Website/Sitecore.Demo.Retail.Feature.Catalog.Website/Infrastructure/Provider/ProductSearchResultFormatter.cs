using System.Collections.Generic;
using System.Configuration.Provider;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Demo.Retail.Feature.Catalog.Website.Factories;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Foundation.Indexing.Models;
using ISearchResult = Sitecore.Foundation.Indexing.Models.ISearchResult;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Infrastructure.Provider
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
            Demo.Retail.Foundation.Commerce.Website.Templates.Commerce.Product.Id
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