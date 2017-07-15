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
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Orders.Models
{

    public class CheckoutViewModel
    {
        public CommerceCart Cart { get; set; }

        /// <summary>
        ///   This collection holds all countries and their provinces (if any).
        ///   Each country and province is expressed as "code|name".  For example,
        ///   the US is "US|United States" and New York is "NY|New York".
        /// </summary>
        public IDictionary<string, IList<string>> CountriesRegions { get; set; } =
            new Dictionary<string, IList<string>>();

        public IDictionary<string, string> LineHrefs { get; set; } =
            new Dictionary<string, string>();

        public IDictionary<string, string> LineImgSrcs { get; set; } =
            new Dictionary<string, string>();

        public IDictionary<string, ShippingOption> LineShippingOptions { get; set; } =
            new Dictionary<string, ShippingOption>();

        public string PaymentClientToken { get; set; }

        public IDictionary<string, string> ShippingOptions { get; set; } =
            new Dictionary<string, string>();

        public bool HasLines => Cart?.Lines != null && Cart.Lines.Any();

        public IParty DefaultAddress { get; set; }

        public string UserName { get; set; }
    }
}