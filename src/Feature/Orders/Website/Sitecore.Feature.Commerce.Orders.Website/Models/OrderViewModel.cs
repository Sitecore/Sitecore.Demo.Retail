using System;
using System.Collections.Generic;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            Lines = new List<OrderLineViewModel>();
        }

        public bool IsItemShipping { get; set; }
        public string OrderId { get; set; }
        public string ExternalId { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
        public string StatusText { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public List<OrderLineViewModel> Lines { get; set; }
        public decimal? TotalSavings { get; set; }
        public decimal Subtotal { get; set; }
        public string Currency { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal Total { get; set; }
        public IEnumerable<IParty> ShippingAddresses { get; set; }
        public IEnumerable<IParty> BillingAddresses { get; set; }
        public IEnumerable<PaymentInfoViewModel> PaymentInfo { get; set; }
        public IEnumerable<string> Adjustments { get; set; }
    }
}