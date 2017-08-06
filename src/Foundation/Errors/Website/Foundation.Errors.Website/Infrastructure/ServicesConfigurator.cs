using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Errors.Website.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcControllersInCurrentAssembly();
        }
    }
}