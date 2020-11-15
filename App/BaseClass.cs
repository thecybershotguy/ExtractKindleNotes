using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExtractKindleNotes
{
    public class BaseClass : INotifyPropertyChanged
    {
        private readonly ILogger _logger;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies the property changed to refresh the UI Element.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseClass"/> class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <exception cref="InvalidOperationException">Log factory not initialized: {nameof(LogFactory)}</exception>
        public BaseClass(string className = null)
        {
            if (LogFactory.LoggerFactory is null)
                throw new InvalidOperationException($"Log factory not initialized: {nameof(LogFactory)}");

            _logger = LogFactory.LoggerFactory.CreateLogger(className);
        }

        /// <summary>
        /// Logs the information level messages.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogInformation(string message) => _logger.LogInformation(message);

        /// <summary>
        /// Logs the debug level messages.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogDebug(string message) => _logger.LogDebug(message);
    }
}
