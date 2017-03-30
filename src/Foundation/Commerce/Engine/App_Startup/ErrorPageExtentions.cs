using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine.App_Startup
{
    public static class ErrorPageExtentions
    {
        public static void ConfigureErrorPage(this IApplicationBuilder app, IHostingEnvironment hostEnv)
        {
            if (hostEnv.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages();
            }
        }
    }
}
