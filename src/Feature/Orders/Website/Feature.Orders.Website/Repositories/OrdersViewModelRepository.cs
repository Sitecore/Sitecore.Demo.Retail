using System;
using System.Collections.Generic;
using System.Linq;
using Feature.Orders.Website.Models;
using Foundation.Commerce.Website;
using Foundation.Commerce.Website.Managers;
using Foundation.Commerce.Website.Models;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Foundation.DependencyInjection;

namespace Feature.Orders.Website.Repositories
{
    [Service]
    public class OrdersViewModelRepository
    {
        public OrdersViewModelRepository(OrderManager orderManager, AccountManager accountManager, StorefrontContext storefrontContext, OrderViewModelRepository orderViewModelRepository)
        {
            OrderManager = orderManager;
            AccountManager = accountManager;
            StorefrontContext = storefrontContext;
            OrderViewModelRepository = orderViewModelRepository;
        }

        public OrderManager OrderManager { get; }
        public AccountManager AccountManager { get; }
        public StorefrontContext StorefrontContext { get; }
        public OrderViewModelRepository OrderViewModelRepository { get; }

        public OrdersViewModel Get(IUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var commerceOrders = OrderManager.GetUserOrders(user.UserName).Result;
            return new OrdersViewModel
            {
                Orders = CreateOrders(commerceOrders)
            };
        }

        private List<OrderViewModel> CreateOrders(IEnumerable<OrderHeader> commerceOrders)
        {
            return commerceOrders?.Select(o => OrderViewModelRepository.Get(o)).ToList() ?? new List<OrderViewModel>();
        }
    }
}