using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine.App_Start
{
    public static class ConfigErrorPage
    {
        public static void Register(IHostingEnvironment hostEnv, IApplicationBuilder app)
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
