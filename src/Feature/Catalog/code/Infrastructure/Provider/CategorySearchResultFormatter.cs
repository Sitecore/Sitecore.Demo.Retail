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
    public class CategorySearchResultFormatter : ProviderBase, ISearchResultFormatter
    {
        public string ContentType => DictionaryPhraseRepository.Current.Get("/Catalog/Search/Category Content Type", "Product Category");

        public IEnumerable<ID> SupportedTemplates => new[]
        {
            Foundation.Commerce.Templates.Commerce.Category.Id
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