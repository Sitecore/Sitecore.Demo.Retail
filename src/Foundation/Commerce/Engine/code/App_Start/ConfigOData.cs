using Microsoft.AspNetCore.OData.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Sitecore.Commerce.Core;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;

namespace Sitecore.Foundation.Commerce.Engine.App_Start
{
    public static class ConfigOData
    {
        public static void Register(IApplicationBuilder app, IConfigureServiceApiPipeline contextPipeline, IConfigureOpsServiceApiPipeline contextOpsServiceApiPipeline, NodeContext node)
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
