using System;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Retail.Project.Engine
{
    public static class ApplicationLogging
    {
        private static ILoggerFactory _loggerFactory = null;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    throw new Exception("Error, logger factory not available. This should never happen.");
                }
                return _loggerFactory;
            }
            set { _loggerFactory = value; }
        }
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
    }
}
