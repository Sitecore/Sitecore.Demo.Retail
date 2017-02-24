using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Catalog;
using Sitecore.Commerce.Services.Globalization;
using Sitecore.DependencyInjection;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.Commerce.Util;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Foundation.Commerce.Infrastructure
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTypesImplementingInCurrentAssembly<BaseManager>(Lifetime.Transient);
            serviceCollection.AddTransient<CountryRepository>();
            serviceCollection.AddTransient<SiteContextRepository>();
            serviceCollection.AddTransient<VisitorContextRepository>();
            serviceCollection.AddSingleton<CartCacheHelper>();
            serviceCollection.AddTypesImplementing<object>(Lifetime.Transient, "Sitecore.Commerce.Connect.CommerceServer");
            serviceCollection.AddTypesImplementing<object>(Lifetime.Transient, "Sitecore.Commerce");
            serviceCollection.AddByWildcard(Lifetime.Transient, "*Pipelines.*");
            serviceCollection.Add(new ServiceDescriptor(typeof(ICommerceSearchManager), provider => CommerceTypeLoader.CreateInstance<ICommerceSearchManager>(), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(ICatalogRepository), provider => CommerceTypeLoader.CreateInstance<ICatalogRepository>(), ServiceLifetime.Singleton));
        }
    }
}