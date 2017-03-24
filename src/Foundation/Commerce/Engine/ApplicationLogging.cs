using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Foundation.Commerce.Engine
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
