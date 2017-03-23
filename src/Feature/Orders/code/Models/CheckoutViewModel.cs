using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Feature.Commerce.Orders.Models
{

    public class CheckoutViewModel
    {
        public CommerceCart Cart { get; set; }

        public IDictionary<string, string> LineHrefs { get; set; } =
            new Dictionary<string, string>();

        public IDictionary<string, string> LineImgSrcs { get; set; } =
            new Dictionary<string, string>();

        public IDictionary<string, ShippingOption> LineShippingOptions { get; set; } =
            new Dictionary<string, ShippingOption>();
    }

}