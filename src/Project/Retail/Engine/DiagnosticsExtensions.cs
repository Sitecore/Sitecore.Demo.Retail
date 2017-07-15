using Microsoft.Extensions.DependencyInjection;
using Sitecore.Framework.Configuration;

namespace Sitecore.Demo.Retail.Project.Engine
{
    public static class DiagnosticsExtensions
    {
        public static ISitecoreServicesConfiguration AddServicesDiagnostics(this ISitecoreServicesConfiguration builder)
        {
            builder.Services.AddSitecoreServicesDiagnosticsPage();
            return builder;
        }
    }
}
    