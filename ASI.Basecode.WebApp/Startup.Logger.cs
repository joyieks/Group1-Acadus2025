using System.IO;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.Seq;
using ASI.Basecode.Data;

namespace ASI.Basecode.WebApp
{
    // Logger configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure the logger
        /// </summary>
        private void ConfigureLogger()
        {
            var loggerFactory = this._app.ApplicationServices.GetService<ILoggerFactory>();

            var logDir = Path.Combine(
                PathManager.DirectoryPath.ApplicationLogsDirectory(this._environment.ApplicationName),
                System.DateTime.Today.ToString("yyyyMM"));

            var defaultLogLevel = Configuration.GetLoggingLogLevel();
            var logFileSize = long.Parse(Configuration.GetLogFileSize());
            int? retained = null; //Retain all files
            var logEventLevel = (Serilog.Events.LogEventLevel)defaultLogLevel;
            var serilogger = new LoggerConfiguration()
                            .MinimumLevel.Is(logEventLevel)
                            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                            .Enrich.FromLogContext()
                            .WriteTo.File(logDir + "\\.txt",
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: logFileSize,
                                rollOnFileSizeLimit: true,
                                retainedFileCountLimit: retained)
                            .WriteTo.Logger(lc => lc
                                            .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Debug)
                                            .WriteTo.File(logDir + "\\Debug_.txt",
                                                rollingInterval: RollingInterval.Day,
                                                fileSizeLimitBytes: logFileSize,
                                                rollOnFileSizeLimit: true,
                                                retainedFileCountLimit: retained))
                            .WriteTo.Logger(lc => lc
                                            .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Warning)
                                            .WriteTo.File(logDir + "\\Warning_.txt",
                                                rollingInterval: RollingInterval.Day,
                                                fileSizeLimitBytes: logFileSize,
                                                rollOnFileSizeLimit: true,
                                                retainedFileCountLimit: retained))
                            .WriteTo.Logger(lc => lc
                                            .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Error)
                                            .WriteTo.File(logDir + "\\Error_.txt",
                                                rollingInterval: RollingInterval.Day,
                                                fileSizeLimitBytes: logFileSize,
                                                rollOnFileSizeLimit: true,
                                                retainedFileCountLimit: retained))
                            .WriteTo.Logger(lc => lc
                                            .MinimumLevel.Debug()
                                            .Filter.ByIncludingOnly(Matching.FromSource<ServiceBase>())
                                            .WriteTo.File(logDir + "\\Services_.txt",
                                                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                                                outputTemplate: "{Message}{NewLine}{Exception}",
                                                rollingInterval: RollingInterval.Day,
                                                fileSizeLimitBytes: logFileSize,
                                                rollOnFileSizeLimit: true,
                                                retainedFileCountLimit: retained))
                            .CreateLogger();

            loggerFactory.AddSerilog(serilogger);

            // Optional: if Seq settings exist, enrich Serilog with Seq sink
            var seqUrl = Configuration.GetSection("Seq").GetValue<string>("ServerUrl");
            var seqApiKey = Configuration.GetSection("Seq").GetValue<string>("ApiKey");
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is((Serilog.Events.LogEventLevel)Configuration.GetLoggingLogLevel())
                    .WriteTo.Seq(seqUrl, apiKey: string.IsNullOrWhiteSpace(seqApiKey) ? null : seqApiKey)
                    .CreateLogger();

                loggerFactory.AddSerilog(Log.Logger, dispose: true);
            }
        }
    }
}
