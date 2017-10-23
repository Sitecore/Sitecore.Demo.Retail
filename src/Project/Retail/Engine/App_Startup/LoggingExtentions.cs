using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Sitecore.Commerce.Core;

namespace Sitecore.Project.Commerce.Retail.Engine.App_Startup
{
    public static class LoggingExtentions
    {
        public static void ConfigureLogging(this ILoggerFactory loggerFactory, IHostingEnvironment hostEnv, string nodeInstanceId)
        {
            // Set the primary LoggerFactory to use everywhere
            ApplicationLogging.LoggerFactory = loggerFactory;

            // Setup logging
            var logsPath = Path.Combine(hostEnv.WebRootPath, "logs");

            var loggingConfig = new LoggerConfiguration()
                .Enrich.With(new ScLogEnricher())
                .MinimumLevel.Information(); // leave all filtering to the MS provider

            if (hostEnv.IsDevelopment())
            {
                loggingConfig.WriteTo.LiterateConsole(Serilog.Events.LogEventLevel.Information);
                loggingConfig.WriteTo.RollingFile($@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{nodeInstanceId}.txt", Serilog.Events.LogEventLevel.Information, "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");

            }
            else if (hostEnv.IsProduction())
            {
                //// Uncomment if you want to log into Mongo
                ////.WriteTo.MongoDBCapped("mongodb://localhost/logs", collectionName: "SitecoreCommerce")
                loggingConfig.WriteTo.RollingFile($@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{nodeInstanceId}.txt", Serilog.Events.LogEventLevel.Warning, "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");
            }
            else if (hostEnv.IsEnvironment("load"))
            {
                //// Uncomment if you want to log into Mongo
                ////.WriteTo.MongoDBCapped("mongodb://localhost/logs", collectionName: "SitecoreCommerce")
                loggingConfig.WriteTo.RollingFile($@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{nodeInstanceId}.txt", Serilog.Events.LogEventLevel.Warning, "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");
            }

            var logger = loggingConfig.CreateLogger();
            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", LogLevel.Warning },
                    { "System", LogLevel.Warning },

                })
                .AddSerilog(logger);

            ////loggerFactory.AddConsole(LogLevel.Error, true);
        }
    }
}
