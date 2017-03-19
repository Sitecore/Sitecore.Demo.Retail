using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Lucene.Exceptions;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Foundation.Commerce.Managers
{
    public class ProductSearchManager : IManager
    {
        public ProductSearchManager(CommerceSearchManager commerceSearchManager)
        {
            CommerceSearchManager = commerceSearchManager;
        }

        private CommerceSearchManager CommerceSearchManager { get; }

        public SearchResults GetSearchResults(SearchOptions searchOptions, string searchKeyword, string catalogName)
        {
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));
            Assert.ArgumentNotNull(searchKeyword, nameof(searchKeyword));

            var returnList = new List<Item>();
            var totalPageCount = 0;
            var totalProductCount = 0;
            var facets = Enumerable.Empty<QueryFacet>();

            if (RenderingContext.Current.Rendering.Item != null && !string.IsNullOrEmpty(searchKeyword.Trim()))
            {
                var searchResponse = SearchCatalogItemsByKeyword(searchKeyword, catalogName, searchOptions);

                if (searchResponse != null)
                {
                    returnList.AddRange(searchResponse.ResponseItems);
                    totalProductCount = searchResponse.TotalItemCount;
                    totalPageCount = searchResponse.TotalPageCount;
                    facets = searchResponse.Facets.Select(f => f.ToQueryFacet());
                }
            }

            return new SearchResults(returnList, totalProductCount, totalPageCount, searchOptions.StartPageIndex, facets);
        }


        private SearchResponse SearchCatalogItemsByKeyword(string keyword, string catalogName, SearchOptions searchOptions)
        {
            Assert.ArgumentNotNullOrEmpty(catalogName, nameof(catalogName));
            var searchIndex = CommerceSearchManager.GetIndex(catalogName);

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceProductSearchResultItem>()
                    .Where(item => item.Name.Equals(keyword) || item[BuiltinFields.DisplayName].Equals(keyword) || item.Content.Contains(keyword))
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Product || item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.CatalogName == catalogName)
                    .Where(item => item.Language == Context.Language.Name)
                    .Select(p => new CommerceProductSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    });

                var options = searchOptions.ToCommerceSearchOptions();
                searchResults = CommerceSearchManager.AddSearchOptionsToQuery(searchResults, options);

                try
                {
                    var results = searchResults.GetResults();
                    var response = SearchResponse.CreateFromSearchResultsItems(options, results);
                    return response;
                }
                catch (TooManyClausesException e)
                {
                    //In some cases a very broad keyword may cause the query to be too big.
                    Log.Warn($"Could not search for '{keyword}'. Results in too many clauses.", e, this);
                    return null;
                }
            }
        }

        public IEnumerable<QueryFacet> GetFacetFieldsForItem(Item item)
        {
            return CommerceSearchManager.GetFacetFieldsForItem(item).Select(f => f.ToQueryFacet());
        }

        public IEnumerable<QuerySortField> GetSortFieldsForItem(Item item)
        {
            return CommerceSearchManager.GetSortFieldsForItem(item).Select(f => f.ToQuerySortField());
        }

        public int GetItemsPerPageForItem(Item item)
        {
            return CommerceSearchManager.GetItemsPerPageForItem(item);
        }
    }
}