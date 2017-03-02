using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Feature.Commerce.Catalog.Factories;
using Sitecore.Feature.Commerce.Catalog.Infrastructure.Pipelines;
using Sitecore.Feature.Commerce.Catalog.Services;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Feature.Commerce.Catalog.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CatalogUrlService>();
            serviceCollection.AddSingleton<CatalogItemContextFactory>();
            serviceCollection.AddByWildcard(Lifetime.Transient, "*Pipelines.*");
        }
    }
}