using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Models
{

    public class CheckoutViewModel
    {
        public IDictionary<ShippingOption, IList<CartLine>> CartLinesMap { get; set; } =
            new Dictionary<ShippingOption, IList<CartLine>>();
    }

}