using System.Collections.Generic;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    internal class SearchInfo
    {
        public string SearchKeyword { get; set; }

        public IEnumerable<CommerceQueryFacet> RequiredFacets { get; set; }

        public IEnumerable<CommerceQuerySort> SortFields { get; set; }

        public int ItemsPerPage { get; set; }

        public Foundation.Commerce.Models.Catalog Catalog { get; set; }

        public CommerceSearchOptions SearchOptions { get; set; }
    }
}