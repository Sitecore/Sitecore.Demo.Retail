using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;

namespace Sitecore.Foundation.Commerce.Website.Models
{
    public class SearchOptions
    {
        public SearchOptions(int numberOfItemsToReturn = 20, int startPageIndex = 0)
        {
            NumberOfItemsToReturn = numberOfItemsToReturn;
            StartPageIndex = startPageIndex;
        }

        public int NumberOfItemsToReturn { get; set; }
        public int StartPageIndex { get; set; }
        public string SortField { get; set; }
        public List<QueryFacet> FacetFields { get; set; }
        public SortDirection SortDirection { get; set; }

        public override string ToString()
        {
            var fields = FacetFields?.Select(f => f.ToString()) ?? new string[0];
            var facets = string.Join("|", fields);
            return base.ToString() + $"{NumberOfItemsToReturn}{StartPageIndex}{SortField}{facets}{SortDirection}";
        }
    }
}