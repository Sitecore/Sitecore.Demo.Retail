using Microsoft.Extensions.DependencyInjection;
using Sitecore.Framework.Configuration;

namespace Sitecore.Foundation.Commerce.Engine
{
    public static class DiagnosticsExtensions
    {
        /// <summary>
        /// Adds the services diagnostics.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>A <see cref="ISitecoreServicesConfiguration"/></returns>
        public static ISitecoreServicesConfiguration AddServicesDiagnostics(this ISitecoreServicesConfiguration builder)
        {
            builder.Services.AddSitecoreServicesDiagnosticsPage();
            return builder;
        }
    }
}
