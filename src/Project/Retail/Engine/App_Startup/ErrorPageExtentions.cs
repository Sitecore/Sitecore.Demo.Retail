using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Sitecore.Demo.Retail.Project.Engine.App_Startup
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
