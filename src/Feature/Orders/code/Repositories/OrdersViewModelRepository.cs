using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Feature.Commerce.Orders.Models;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Security.Accounts;

namespace Sitecore.Feature.Commerce.Orders.Repositories
{
    [Service]
    public class OrdersViewModelRepository
    {
        public OrdersViewModelRepository(OrderManager orderManager, AccountManager accountManager, StorefrontManager storefrontManager, OrderViewModelRepository orderViewModelRepository)
        {
            OrderManager = orderManager;
            AccountManager = accountManager;
            StorefrontManager = storefrontManager;
            OrderViewModelRepository = orderViewModelRepository;
        }
        public OrderManager OrderManager { get; }
        public AccountManager AccountManager { get; }
        public StorefrontManager StorefrontManager { get; }
        public OrderViewModelRepository OrderViewModelRepository { get; }

        public OrdersViewModel Get(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var commerceUser = AccountManager.GetUser(user.Name).Result;
            if (commerceUser == null)
                return null;
            var commerceOrders = OrderManager.GetOrders(commerceUser.ExternalId, StorefrontManager.Current.ShopName).Result;
            return new OrdersViewModel()
            {
                Orders = CreateOrders(commerceOrders)
            };
        }

        private List<OrderViewModel> CreateOrders(IEnumerable<OrderHeader> commerceOrders)
        {
            return commerceOrders.Select(o => OrderViewModelRepository.Get(o)).ToList();
        }
    }
}