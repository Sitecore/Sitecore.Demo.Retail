using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Sitecore.Reflection;
using Sitecore.Xml;

namespace Sitecore.Foundation.DependencyInjection.Infrastructure
{
    public class MvcControllerServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcControllers("*.Feature.*");
        }
    }
}