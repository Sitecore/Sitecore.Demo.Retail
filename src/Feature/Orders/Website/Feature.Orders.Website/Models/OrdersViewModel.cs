using System.Collections.Generic;

namespace Feature.Orders.Website.Models
{
    public class OrdersViewModel
    {
        public OrdersViewModel()
        {
            Orders = new List<OrderViewModel>();
        }

        public List<OrderViewModel> Orders { get; set; }
    }
}