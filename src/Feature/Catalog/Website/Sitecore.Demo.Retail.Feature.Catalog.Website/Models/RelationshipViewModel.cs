using System.Collections.Generic;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public class RelationshipViewModel
    {
        public RelationshipViewModel()
        {
            ChildProducts = new List<ProductViewModel>();
        }

        public List<ProductViewModel> ChildProducts { get; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}