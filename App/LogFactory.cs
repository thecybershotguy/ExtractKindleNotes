using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Windows.Markup;

namespace ExtractKindleNotes
{
    public static class LogFactory
    {

        /// <summary>
        /// LoggerFactory used to Create logger.
        /// </summary>
        /// <value>
        /// The logger factory.
        /// </value>
        public static ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// Initializes the logger with specified path to nlog configuration.
        /// </summary>
        /// <param name="pathToNlogConfig">The path to nlog configuration.</param>
        public static void Initalise(string pathToNlogConfig)
        {
            try
            {
                var config = new XmlLoggingConfiguration(pathToNlogConfig);
                LogManager.ThrowConfigExceptions = true;
                LogManager.Configuration = config;
                LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    builder.AddNLog();
                });
            }
            catch (System.Exception)
            {
                throw;
            }

            
        }
    }
}
