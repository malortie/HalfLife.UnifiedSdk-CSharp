﻿using Serilog;
using System.CommandLine.Binding;

namespace HalfLife.UnifiedSdk.Utilities.Logging
{
    /// <summary>
    /// Provides a <see cref="ILogger"/> in a <see cref="System.CommandLine.Command"/> handler.
    /// The logger is configured to log to the console.
    /// </summary>
    public sealed class LoggerBinder : BinderBase<ILogger>
    {
        /// <summary>Singleton binder instance.</summary>
        public static LoggerBinder Instance { get; } = new();

        /// <inheritdoc/>
        protected override ILogger GetBoundValue(BindingContext bindingContext)
        {
            return new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
