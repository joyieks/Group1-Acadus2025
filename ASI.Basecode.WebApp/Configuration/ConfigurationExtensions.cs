using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Extensions.Configuration
{
    /// <summary>
    /// Configuration Extension
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets the setup root directory path.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <returns>Set up root path</returns>
        public static string GetSetupRootDirectoryPath(this IConfiguration configuration)
        {
            return configuration.GetSection("Common")
                                .GetValue<string>("SetupRoot");
        }

        /// <summary>
        /// Gets the CSV output folder path.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static string GetCsvOutputFolderPath(this IConfiguration configuration)
        {
            return configuration.GetSection("Common")
                                .GetValue<string>("CsvOutputFolderPath");
        }

        /// <summary>
        /// Gets the CSV import backup path.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static string GetCsvImportBackupPath(this IConfiguration configuration)
        {
            return configuration.GetSection("Common")
                               .GetValue<string>("CsvImportBackupPath");
        }

        /// <summary>
        /// Gets the logging section.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <returns>Logging settings</returns>
        public static IConfigurationSection GetLoggingSection(this IConfiguration configuration)
        {
            return configuration.GetSection("Logging");
        }

        /// <summary>
        /// Gets the logging log level.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="name">Name</param>
        /// <returns>Logging level</returns>
        public static LogLevel GetLoggingLogLevel(this IConfiguration configuration, string name = "Default")
        {
            return configuration.GetSection("Logging")
                                .GetValue<LogLevel>(string.Format("LogLevel:{0}", name));
        }

        /// <summary>
        /// Gets the size limit of each log file.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Log size limit</returns>
        public static string GetLogFileSize(this IConfiguration configuration)
        {
            return configuration.GetSection("Common")
                                .GetValue<string>("LogFileSize");
        }
    }
}
