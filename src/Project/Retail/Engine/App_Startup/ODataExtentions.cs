using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Sitecore.Commerce.Core;

namespace Sitecore.Demo.Retail.Project.Engine.App_Startup
{
    public static class ODataExtentions
    {
        public static void ConfigureOData(this IApplicationBuilder app, IConfigureServiceApiPipeline contextPipeline, IConfigureOpsServiceApiPipeline contextOpsServiceApiPipeline, NodeContext node)
        {
            // Initialize plugins OData contexts
            app.InitializeODataBuilder();
            var modelBuilder = new ODataConventionModelBuilder();

            // Run the pipeline to configure the plugin's OData context
            var contextResult = contextPipeline.Run(modelBuilder, node.GetPipelineContextOptions()).Result;
            contextResult.Namespace = "Sitecore.Commerce.Engine";

            // Get the model and register the ODataRoute
            var model = contextResult.GetEdmModel();
            app.UseRouter(new ODataRoute("Api", model));

            // Register the bootstrap context for the engine
            modelBuilder = new ODataConventionModelBuilder();
            var contextOpsResult = contextOpsServiceApiPipeline.Run(modelBuilder, node.GetPipelineContextOptions()).Result;
            contextOpsResult.Namespace = "Sitecore.Commerce.Engine";

            // Get the model and register the ODataRoute
            model = contextOpsResult.GetEdmModel();
            app.UseRouter(new ODataRoute("CommerceOps", model));
        }
    }
}
