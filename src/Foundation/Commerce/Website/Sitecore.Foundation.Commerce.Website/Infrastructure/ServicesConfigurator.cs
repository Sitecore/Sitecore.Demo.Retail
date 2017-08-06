using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.DependencyInjection;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Website.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddClassesWithServiceAttribute();
            serviceCollection.AddTypesImplementingInCurrentAssembly<IManager>(Lifetime.Transient);
            serviceCollection.AddTypesImplementing<object>(Lifetime.Transient, "Sitecore.Commerce.Connect.CommerceServer");
            serviceCollection.AddTypesImplementing<object>(Lifetime.Transient, "Sitecore.Commerce");
            serviceCollection.Add(new ServiceDescriptor(typeof(ICommerceSearchManager), provider => CommerceTypeLoader.CreateInstance<ICommerceSearchManager>(), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(ICatalogRepository), provider => CommerceTypeLoader.CreateInstance<ICatalogRepository>(), ServiceLifetime.Singleton));
        }
    }
}