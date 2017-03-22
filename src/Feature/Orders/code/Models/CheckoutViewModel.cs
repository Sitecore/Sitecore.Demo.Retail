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
        public IDictionary<string, IList<CommerceCartLineWithImages>> CartLinesMap { get; set; } =
            new Dictionary<string, IList<CommerceCartLineWithImages>>();

        public CommerceCart Cart { get; set; }
        public CommerceTotal Total => Cart.Total as CommerceTotal;

        public string GetProductLink(CartLine line)
        {
            var productItem = GetProductItem(line);
            return LinkManager.GetDynamicUrl(productItem).TrimEnd('/');
        }

        public Item GetProductItem(CartLine line)
        {
            // The SitecoreProductItemId is the ID to variant under a product.
            // Get the item and then return its parent.
            var productVariantItemId = line.Product.SitecoreProductItemId;
            var productVariantItem = Context.Database.GetItem(productVariantItemId);
            return productVariantItem.Parent;
        }

        public decimal GetSubTotal()
        {
            decimal subTotal = 0;
            if (Cart != null && Cart.Lines != null && Cart.Lines.Any())
            {
                foreach (var line in Cart.Lines)
                {
                    subTotal += line.Total.Amount;
                }
            }
            return subTotal;
        }

    }

}