using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Diagnostics;
using Sitecore.Foundation.Commerce.Engine.App_Startup;

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

        public Startup(IServiceProvider serviceProvider, IHostingEnvironment hostEnv, ILoggerFactory loggerFactory)
        {
            this._hostEnv = hostEnv;
            this._hostServices = serviceProvider;

            loggerFactory.ConfigureLogging(hostEnv, _nodeInstanceId);
            this._logger = ApplicationLogging.CreateLogger("Startup");

            this._configuration = _hostEnv.ConfigureSettings();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureDataProtection(_configuration);

            _node = _hostEnv.CommerceNodeInitialize(_nodeInstanceId);
            var environment = services.ConfigureCommerceEnvironment(_configuration, _node);

            services.ConfigureCommerceNode(_configuration, _nodeInstanceId, environment, _node);
            services.AddOData();
            services.AddMvc();
            services.ConfigureApplicationInsights(_configuration);
            services.ConfigureSitecore(_hostServices);

            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(this._logger);
        }

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
            app.ConfigureErrorPage(_hostEnv);

            startNodePipeline.Start(this._node);
            startEnvironmentPipeline.Start(this._node, this._configuration);

            app.ConfigureOData(contextPipeline, contextOpsServiceApiPipeline, _node);
        }
    }
}
