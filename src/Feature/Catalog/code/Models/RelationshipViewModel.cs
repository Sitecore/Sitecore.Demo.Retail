using System.Collections.Generic;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Mvc;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Feature.Commerce.Catalog.Models
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