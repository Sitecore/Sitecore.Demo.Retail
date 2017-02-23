using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Contacts;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Catalog;
using Sitecore.Commerce.Services.Globalization;
using Sitecore.DependencyInjection;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
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
            serviceCollection.AddTypesImplementing<object>(Lifetime.Transient, "Sitecore.Commerce.Connect.CommerceServer");
            serviceCollection.AddTypesImplementing<object>(Lifetime.Transient, "Sitecore.Commerce");
            serviceCollection.AddByWildcard(Lifetime.Transient, "*Pipelines.*");
        }
    }
}