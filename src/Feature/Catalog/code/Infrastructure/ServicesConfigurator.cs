using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Feature.Commerce.Catalog.Infrastructure.Pipelines;
using Sitecore.Feature.Commerce.Catalog.Repositories;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Feature.Commerce.Catalog.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<CatalogRepository>();
            serviceCollection.Add(new ServiceDescriptor(typeof(IProductResolver), typeof(ProductItemResolver), ServiceLifetime.Singleton));
            serviceCollection.AddByWildcard(Lifetime.Transient, "*Pipelines.*");
        }
    }
}