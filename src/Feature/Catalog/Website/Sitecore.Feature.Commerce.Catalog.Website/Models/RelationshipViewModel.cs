using System.Collections.Generic;

namespace Sitecore.Feature.Commerce.Catalog.Website.Models
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