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
        public Item Item { get; set; }
        public HtmlString DescriptionRenderer => Item == null ? new HtmlString(Description) : PageContext.Current.HtmlHelper.Sitecore().Field("Value", Item);
    }
}