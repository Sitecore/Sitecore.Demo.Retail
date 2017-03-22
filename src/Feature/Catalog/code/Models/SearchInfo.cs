using System.Collections.Generic;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    internal class SearchInfo
    {
        public string SearchKeyword { get; set; }

        public IEnumerable<QueryFacet> RequiredFacets { get; set; }

        public IEnumerable<QuerySortField> SortFields { get; set; }

        public int ItemsPerPage { get; set; }

        public Foundation.Commerce.Models.Catalog Catalog { get; set; }

        public SearchOptions SearchOptions { get; set; }
    }
}