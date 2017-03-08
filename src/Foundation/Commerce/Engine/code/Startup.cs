

//TODO: THis is from the referece storefront. Remove when validated refactoring


//using System;
//using System.IO;

//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.Extensibility;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.DataProtection;
//using Microsoft.AspNetCore.DataProtection.XmlEncryption;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.OData.Builder;
//using Microsoft.AspNetCore.OData.Extensions;
//using Microsoft.AspNetCore.OData.Routing;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//using Serilog;

//using Sitecore.Commerce.Core;
//using Sitecore.Commerce.Provider.FileSystem;
//using Sitecore.Framework.Diagnostics;
//using Sitecore.Framework.Rules;

//namespace Sitecore.Foundation.Commerce.Engine
//{
//    public class Startup
//    {
//        private string _nodeInstanceId = Guid.NewGuid().ToString("N");
//        private readonly IServiceProvider _hostServices;
//        private readonly IHostingEnvironment _hostEnv;
//        private readonly IConfiguration _configuration;
//        private volatile CommerceEnvironment _environment;
//        private readonly Microsoft.Extensions.Logging.ILogger _logger;
//        private volatile NodeContext _nodeContext;
//        private IServiceCollection _services;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="Startup"/> class.
//        /// </summary>
//        /// <param name="serviceProvider">The service provider.</param>
//        /// <param name="hostEnv">The host env.</param>
//        /// <param name="loggerFactory">The logger factory.</param>
//        public Startup(IServiceProvider serviceProvider, IHostingEnvironment hostEnv, ILoggerFactory loggerFactory)
//        {
//            this._hostEnv = hostEnv;
//            this._hostServices = serviceProvider;

//            // Setup logging
//            var logsPath = Path.Combine(this._hostEnv.WebRootPath, "logs");

//            var loggingConfig = new LoggerConfiguration()
//                .Enrich.With(new ScLogEnricher())
//                .MinimumLevel.Information(); // leave all filtering to the MS provider

//            if (this._hostEnv.IsDevelopment())
//            {
//                loggingConfig.WriteTo.LiterateConsole(Serilog.Events.LogEventLevel.Information);
//                loggingConfig.WriteTo.RollingFile($@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{this._nodeInstanceId}.txt", Serilog.Events.LogEventLevel.Information, "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");

//            }
//            else if (this._hostEnv.IsProduction())
//            {
//                //// Uncomment if you want to log into Mongo
//                ////.WriteTo.MongoDBCapped("mongodb://localhost/logs", collectionName: "SitecoreCommerce")
//                loggingConfig.WriteTo.RollingFile($@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{this._nodeInstanceId}.txt", Serilog.Events.LogEventLevel.Warning, "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");
//            }
//            else if (this._hostEnv.IsEnvironment("load"))
//            {
//                //// Uncomment if you want to log into Mongo
//                ////.WriteTo.MongoDBCapped("mongodb://localhost/logs", collectionName: "SitecoreCommerce")
//                loggingConfig.WriteTo.RollingFile($@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{this._nodeInstanceId}.txt", Serilog.Events.LogEventLevel.Warning, "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");
//            }

//            var logger = loggingConfig.CreateLogger();
//            loggerFactory
//                .WithFilter(new FilterLoggerSettings
//                {
//                    { "Microsoft", LogLevel.Warning },
//                    { "System", LogLevel.Warning },

//                })
//                .AddSerilog(logger);

//            ////loggerFactory.AddConsole(LogLevel.Error, true);
//            this._logger = loggerFactory.CreateLogger("Startup");

//            // Setup configuration sources.
//            var builder = new ConfigurationBuilder()
//                .SetBasePath(hostEnv.WebRootPath)
//                .AddJsonFile("config.json")
//                .AddEnvironmentVariables();

//            this._configuration = builder.Build();

//            // TODO uncomment for Application Insights
//            if (this._hostEnv.IsDevelopment())
//            {
//                builder.AddApplicationInsightsSettings(developerMode: true);
//            }
//        }

//        /// <summary>
//        /// Gets the node context.
//        /// </summary>
//        /// <value>
//        /// The node context.
//        /// </value>
//        public NodeContext NodeContext => this._nodeContext ?? this.InitializeNodeContext();

//        /// <summary>
//        /// Gets or sets the Initial Startup Environment. This will tell the Node how to behave
//        /// This will be overloaded by the Environment stored in configuration.
//        /// </summary>
//        /// <value>
//        /// The startup environment.
//        /// </value>
//        public CommerceEnvironment StartupEnvironment
//        {
//            get
//            {
//                return this._environment ?? (this._environment = new CommerceEnvironment { Name = "Bootstrap" });
//            }

//            set
//            {
//                this._environment = value;
//            }
//        }

//        /// <summary>
//        /// Configures the services.
//        /// </summary>
//        /// <param name="services">The services.</param>
//        public void ConfigureServices(IServiceCollection services)
//        {
//            this._services = services;

//            this.SetupDataProtection(services);

//            this.StartupEnvironment = this.GetGlobalEnvironment();
//            this.NodeContext.Environment = this.StartupEnvironment;

//            this._services.AddSingleton(this.StartupEnvironment);
//            this._services.AddSingleton(this.NodeContext);

//            // Add the ODataServiceBuilder to the  services collection
//            services.AddOData();

//            // Add MVC services to the services container.
//            services.AddMvc();

//            // TODO uncomment for Application Insights
//            services.AddApplicationInsightsTelemetry(this._configuration);

//            TelemetryConfiguration.Active.DisableTelemetry = true;

//            this._logger.LogInformation("BootStrapping Services...");

//            services.Sitecore()
//                .Eventing()
//                //// .Bootstrap(this._hostServices)
//                //// .AddServicesDiagnostics()
//                .Caching(config => config
//                    .AddMemoryStore("GlobalEnvironment")
//                    .ConfigureCaches("GlobalEnvironment.*", "GlobalEnvironment"))
//            //// .AddCacheDiagnostics()
//            .Rules(config => config
//                .IgnoreNamespaces(n => n.Equals("Sitecore.Commerce.Plugin.Tax")))
//            .RulesSerialization();
//            services.Add(new ServiceDescriptor(typeof(IRuleBuilderInit), typeof(RuleBuilder), ServiceLifetime.Transient));

//            this._logger.LogInformation("BootStrapping application...");
//            services.Sitecore().Bootstrap(this._hostServices);

//            // TODO uncomment for Application Insights
//            services.Add(new ServiceDescriptor(typeof(TelemetryClient), typeof(TelemetryClient), ServiceLifetime.Singleton));
//            this.NodeContext.Objects.Add(services);

//            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(this._logger);
//        }

//        /// <summary>
//        /// Configures the specified application.
//        /// </summary>
//        /// <param name="app">The application.</param>
//        /// <param name="contextPipeline">The context pipeline.</param>
//        /// <param name="startNodePipeline">The start node pipeline.</param>
//        /// <param name="contextOpsServiceApiPipeline">The API pipeline.</param>
//        /// <param name="startEnvironmentPipeline">The start environment pipeline.</param>
//        /// <param name="loggerFactory">The logger factory.</param>
//        public void Configure(
//            IApplicationBuilder app,
//            IConfigureServiceApiPipeline contextPipeline,
//            IStartNodePipeline startNodePipeline,
//            IConfigureOpsServiceApiPipeline contextOpsServiceApiPipeline,
//            IStartEnvironmentPipeline startEnvironmentPipeline,
//            ILoggerFactory loggerFactory)
//        {
//            // Add Application Insights monitoring to the request pipeline as a very first middleware.
//            app.UseApplicationInsightsRequestTelemetry();

//            // Add Application Insights exceptions handling to the request pipeline.
//            app.UseApplicationInsightsExceptionTelemetry();

//            app.UseDiagnostics();

//            // Add static files to the request pipeline.
//            app.UseStaticFiles();

//            // Set the error page
//            if (this._hostEnv.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseStatusCodePages();
//            }

//            startNodePipeline.Run(this.NodeContext, this.NodeContext.GetPipelineContextOptions()).Wait();

//            // Starting the environment to register Minion policies and run Minions
//            var environmentName = this._configuration.GetSection("AppSettings:EnvironmentName").Value;

//            ////this.NodeContext.AddDataMessage("EnvironmentStartup", $"StartEnvironment={environmentName}");

//            ////startEnvironmentPipeline.Run(environmentName, this.NodeContext.GetPipelineContextOptions()).Wait();

//            // Initialize plugins OData contexts
//            app.InitializeODataBuilder();
//            var modelBuilder = new ODataConventionModelBuilder();

//            // Run the pipeline to configure the plugin's OData context
//            var contextResult = contextPipeline.Run(modelBuilder, this.NodeContext.GetPipelineContextOptions()).Result;
//            contextResult.Namespace = "Sitecore.Commerce.Engine";

//            // Get the model and register the ODataRoute
//            var model = contextResult.GetEdmModel();
//            app.UseRouter(new ODataRoute("Api", model));

//            // Register the bootstrap context for the engine
//            modelBuilder = new ODataConventionModelBuilder();
//            var contextOpsResult = contextOpsServiceApiPipeline.Run(modelBuilder, this.NodeContext.GetPipelineContextOptions()).Result;
//            contextOpsResult.Namespace = "Sitecore.Commerce.Engine";

//            // Get the model and register the ODataRoute
//            model = contextOpsResult.GetEdmModel();
//            app.UseRouter(new ODataRoute("CommerceOps", model));
//        }

//        /// <summary>
//        /// Initializes the node context.
//        /// </summary>
//        /// <returns>A <see cref="NodeContext"/></returns>
//        private NodeContext InitializeNodeContext()
//        {
//            this._nodeContext = new NodeContext(this._logger, new TelemetryClient())
//            {
//                CorrelationId = this._nodeInstanceId,
//                ConnectionId = "Node_Global",
//                ContactId = "Node_Global",
//                GlobalEnvironment = this.StartupEnvironment,
//                Environment = this.StartupEnvironment,
//                WebRootPath = this._hostEnv.WebRootPath,
//                LoggingPath = this._hostEnv.WebRootPath + @"\logs\"
//            };
//            return this._nodeContext;
//        }

//        /// <summary>
//        /// Setups the data protection storage and encryption protection type
//        /// </summary>
//        /// <param name="services">The services.</param>
//        private void SetupDataProtection(IServiceCollection services)
//        {
//            var builder = services.AddDataProtection();
//            var pathToKeyStorage = this._configuration.GetSection("AppSettings:EncryptionKeyStorageLocation").Value;

//            // Persist keys to a specific directory (should be a network location in distributed application)
//            builder.PersistKeysToFileSystem(new DirectoryInfo(pathToKeyStorage));

//            var protectionType = this._configuration.GetSection("AppSettings:EncryptionProtectionType").Value.ToUpperInvariant();

//            switch (protectionType)
//            {
//                case "DPAPI-SID":
//                    var storageSid = this._configuration.GetSection("AppSettings:EncryptionSID").Value.ToUpperInvariant();
//                    //// Uses the descriptor rule "SID=S-1-5-21-..." to encrypt with domain joined user
//                    builder.ProtectKeysWithDpapiNG($"SID={storageSid}", flags: DpapiNGProtectionDescriptorFlags.None);
//                    break;
//                case "DPAPI-CERT":
//                    var storageCertificateHash = this._configuration.GetSection("AppSettings:EncryptionCertificateHash").Value.ToUpperInvariant();
//                    //// Searches the cert store for the cert with this thumbprint
//                    builder.ProtectKeysWithDpapiNG(
//                        $"CERTIFICATE=HashId:{storageCertificateHash}",
//                        flags: DpapiNGProtectionDescriptorFlags.None);
//                    break;
//                case "LOCAL":
//                    //// Only the local user account can decrypt the keys
//                    builder.ProtectKeysWithDpapiNG();
//                    break;
//                case "MACHINE":
//                    //// All user accounts on the machine can decrypt the keys
//                    builder.ProtectKeysWithDpapi(protectToLocalMachine: true);
//                    break;
//                default:
//                    //// All user accounts on the machine can decrypt the keys
//                    builder.ProtectKeysWithDpapi(protectToLocalMachine: true);
//                    break;
//            }
//        }

//        /// <summary>
//        /// Gets the global environment.
//        /// </summary>
//        /// <returns>A <see cref="CommerceEnvironment"/></returns>
//        private CommerceEnvironment GetGlobalEnvironment()
//        {
//            CommerceEnvironment environment;

//            this._logger.LogInformation($"Loading Global Environment using Filesystem Provider from: {this._hostEnv.WebRootPath} s\\Bootstrap\\");

//            // Use the default File System provider to setup the environment
//            this.NodeContext.BootStrapProviderPath = this._hostEnv.WebRootPath + @"\Bootstrap\";
//            var bootstrapProvider = new FileSystemEntityProvider(NodeContext.BootStrapProviderPath);

//            var bootstrapFile = this._configuration.GetSection("AppSettings:BootStrapFile").Value;

//            if (!string.IsNullOrEmpty(bootstrapFile))
//            {
//                this.NodeContext.BootStrapEnvironmentPath = bootstrapFile;

//                this.NodeContext.AddDataMessage("NodeStartup", $"GlobalEnvironmentFrom='Configuration: {bootstrapFile}'");
//                environment = bootstrapProvider.Find<CommerceEnvironment>(this.NodeContext, bootstrapFile, false).Result;
//            }
//            else
//            {
//                // Load the NodeContext default
//                bootstrapFile = "Global";
//                this.NodeContext.BootStrapEnvironmentPath = bootstrapFile;
//                this.NodeContext.AddDataMessage("NodeStartup", $"GlobalEnvironmentFrom='{bootstrapFile}.json'");
//                environment = bootstrapProvider.Find<CommerceEnvironment>(this.NodeContext, bootstrapFile, false).Result;
//            }

//            this.NodeContext.BootStrapEnvironmentPath = bootstrapFile;

//            this.NodeContext.GlobalEnvironmentName = environment.Name;
//            this.NodeContext.AddDataMessage("NodeStartup", $"Status='Started',GlobalEnvironmentName='{NodeContext.GlobalEnvironmentName}'");

//            if (this._configuration.GetSection("AppSettings:BootStrapFile").Value != null)
//            {
//                this.NodeContext.ContactId = this._configuration.GetSection("AppSettings:NodeId").Value;
//            }

//            if (!string.IsNullOrEmpty(environment.GetPolicy<DeploymentPolicy>().DeploymentId))
//            {
//                this.NodeContext.ContactId = $"{environment.GetPolicy<DeploymentPolicy>().DeploymentId}_{this._nodeInstanceId}";
//            }

//            return environment;
//        }
//    }
//}
