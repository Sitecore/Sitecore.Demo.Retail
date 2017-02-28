using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Reference.Storefront.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcControllersInCurrentAssembly();
        }
    }
}