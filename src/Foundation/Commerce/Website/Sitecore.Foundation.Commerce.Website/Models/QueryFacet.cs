using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch.Linq;

namespace Sitecore.Foundation.Commerce.Website.Models
{
    public class QueryFacet
    {
        public List<FacetValue> FoundValues { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<object> Values { get; set; }

        public void Clean()
        {
            if (FoundValues == null)
            {
                return;
            }

            var items = FoundValues.Where(v => string.IsNullOrEmpty(v.Name) || v.AggregateCount == 0);
            items.ToList().ForEach(v => FoundValues.Remove(v));
        }

        public bool IsValid()
        {
            Clean();
            return FoundValues != null && FoundValues.Count > 0;
        }

        public override string ToString()
        {
            return base.ToString() + $"_{Name}";
        }
    }
}