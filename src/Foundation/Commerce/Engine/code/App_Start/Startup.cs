using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Foundation.Commerce.Engine.App_Start;
using Sitecore.Framework.Diagnostics;

namespace Sitecore.Foundation.Commerce.Engine
{
    public class Startup
    {
        private readonly ILogger _logger;

        private readonly IHostingEnvironment _hostEnv;
        private readonly IServiceProvider _hostServices;
        private readonly IConfigurationRoot _configuration;
        private readonly string _nodeInstanceId = Guid.NewGuid().ToString("N");

        private NodeContext _node;

        /// <summary>
        /// Initializess a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public Startup(IServiceProvider serviceProvider, IHostingEnvironment hostEnv, ILoggerFactory loggerFactory)
        {
            this._hostEnv = hostEnv;
            this._hostServices = serviceProvider;

            ConfigLogging.Register(hostEnv, loggerFactory, _nodeInstanceId);
            this._logger = ApplicationLogging.CreateLogger("Startup");

            this._configuration = ConfigSettings.Register(_hostEnv);
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigDataProtection.Register(_configuration, services);

            _node = ConfigNode.Initialize(_nodeInstanceId, _hostEnv);
            var environment = ConfigEnvironment.Register(_configuration, _node, services);

            ConfigNode.Register(_configuration, _nodeInstanceId, environment, _node, services);

            services.AddOData();
            services.AddMvc();

            ConfigApplicationInsights.Register(_configuration, services);
            ConfigSitecore.Register(_hostServices, services);

            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(this._logger);
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="contextPipeline">The context pipeline.</param>
        /// <param name="startNodePipeline">The start node pipeline.</param>
        /// <param name="contextOpsServiceApiPipeline">The API pipeline.</param>
        /// <param name="startEnvironmentPipeline">The start environment pipeline.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(
            IApplicationBuilder app,
            IConfigureServiceApiPipeline contextPipeline,
            IStartNodePipeline startNodePipeline,
            IConfigureOpsServiceApiPipeline contextOpsServiceApiPipeline,
            IStartEnvironmentPipeline startEnvironmentPipeline,
            ILoggerFactory loggerFactory)
        {
            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();
            app.UseDiagnostics();
            app.UseStaticFiles();

            ConfigErrorPage.Register(_hostEnv, app);

            startNodePipeline.Run(this._node, this._node.GetPipelineContextOptions()).Wait();

            ConfigOData.Register(app, contextPipeline, contextOpsServiceApiPipeline, _node);
        }
    }
}
