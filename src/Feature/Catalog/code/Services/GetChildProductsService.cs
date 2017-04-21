using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Feature.Commerce.Catalog.Services
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