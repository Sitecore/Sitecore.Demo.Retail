using System.Collections.Generic;
using System.Web.Helpers;

namespace Sitecore.Foundation.Commerce.Models
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
    }
}