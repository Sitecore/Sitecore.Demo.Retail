using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine.App_Start
{
    public static class ConfigNode
    {
        /// <summary>
        /// Initializes the node context.
        /// </summary>
        /// <returns>A <see cref="NodeContext"/></returns>
        public static NodeContext Initialize(string nodeInstanceId, IHostingEnvironment hostEnv)
        {
            var logger = ApplicationLogging.CreateLogger("ConfigNode");

            // temporary environment status
            var environment = new CommerceEnvironment { Name = "Bootstrap" };

            return new NodeContext(logger, new TelemetryClient())
            {
                CorrelationId = nodeInstanceId,
                ConnectionId = "Node_Global",
                ContactId = "Node_Global",
                GlobalEnvironment = environment,
                Environment = environment,
                WebRootPath = hostEnv.WebRootPath,
                LoggingPath = hostEnv.WebRootPath + @"\logs\",
                BootStrapProviderPath = hostEnv.WebRootPath + @"\Bootstrap\", // The default
                BootStrapEnvironmentPath = "Global", // The default
            };
        }

        public static void Register(IConfigurationRoot configuration, string nodeInstanceId, CommerceEnvironment environment, NodeContext node, IServiceCollection services)
        {
            node.Environment = environment;
            node.GlobalEnvironmentName = environment.Name;

            node.AddDataMessage("NodeStartup", $"Status='Started',GlobalEnvironmentName='{node.GlobalEnvironmentName}'");

            if (!string.IsNullOrEmpty(environment.GetPolicy<DeploymentPolicy>().DeploymentId))
            {
                node.ContactId = $"{environment.GetPolicy<DeploymentPolicy>().DeploymentId}_{nodeInstanceId}";
            }
            else if(configuration.GetSection("AppSettings:BootStrapFile").Value != null)
            {
                node.ContactId = configuration.GetSection("AppSettings:NodeId").Value;

            }

            node.Objects.Add(services);

            services.AddSingleton(node);
        }
    }
}
