using System.Collections.Generic;
using System.Configuration.Provider;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Foundation.Indexing.Models;
using Sitecore.Web.UI.WebControls;
using ISearchResult = Sitecore.Foundation.Indexing.Models.ISearchResult;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Infrastructure.Provider
{
    public class CategorySearchResultFormatter : ProviderBase, ISearchResultFormatter
    {
        public string ContentType => DictionaryPhraseRepository.Current.Get("/Catalog/Search/Category Content Type", "Product Category");

        public IEnumerable<ID> SupportedTemplates => new[]
        {
            Foundation.Commerce.Website.Templates.Commerce.Category.Id
        };

        public void FormatResult(SearchResultItem item, ISearchResult formattedResult)
        {
            var contentItem = item.GetItem();
            formattedResult.Title = contentItem.DisplayName;
            formattedResult.Description = FieldRenderer.Render(contentItem, Templates.Generated.Category.Fields.Description);
            formattedResult.ViewName = "~/Views/Catalog/_CategorySearchResult.cshtml";
        }
    }
}