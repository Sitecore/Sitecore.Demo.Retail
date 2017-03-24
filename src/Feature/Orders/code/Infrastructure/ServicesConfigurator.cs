using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Feature.Commerce.Orders.Repositories;

namespace Sitecore.Feature.Commerce.Orders.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<OrdersViewModelRepository>();
            serviceCollection.AddSingleton<OrderViewModelRepository>();
        }
    }
}