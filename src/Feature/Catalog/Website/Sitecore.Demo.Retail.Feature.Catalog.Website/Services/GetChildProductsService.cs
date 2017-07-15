using System.Web;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Services
{
    [Service]
    public class GetChildProductsService
    {
        public GetChildProductsService(CatalogManager catalogManager)
        {
            CatalogManager = catalogManager;
        }

        public SearchResults GetChildProducts(Category category, SearchOptions searchOptions)
        {
            var cacheKey = $"ChildProducts/{category.InnerItem.ID}/{searchOptions}";

            var results = HttpContext.Current?.Items[cacheKey] as SearchResults;
            if (results != null)
            {
                return results;
            }
            results = this.CatalogManager.GetChildProducts(category.InnerItem, searchOptions);

            HttpContext.Current?.Items.Add(cacheKey, results);
            return results;
        }

        public CatalogManager CatalogManager { get; }
    }
}