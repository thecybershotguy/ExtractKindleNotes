﻿using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectTools
{
    public class BaseClass : INotifyPropertyChanged
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseClass"/> class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <exception cref="InvalidOperationException">Log factory not initialized: {nameof(LogFactory)}</exception>
        public BaseClass(string className = null)
        {
            if (string.IsNullOrEmpty(className))
            {
                return;
            }

            if (LogFactory.Logger is null)
                throw new InvalidOperationException($"Log factory not initialized: {nameof(LogFactory)}");

            _logger = LogFactory.Logger.CreateLogger(className);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Logs the debug level messages.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogDebug(string message) => _logger.LogDebug(message);

        /// <summary>
        /// Logs the error level message.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">The message.</param>
        public void LogError(Exception e, params string[] message) => _logger.LogError(e, e.Message, message);

        /// <summary>
        /// Logs the information level messages.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogInformation(string message) => _logger.LogInformation(message);

        /// <summary>
        /// Notifies the property changed to refresh the UI Element.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
