using System;
using System.Linq;
using ServiceLibrary.Exceptions;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public static class NServiceBusValidator
    {
        /// <summary>
        /// Determines the Transport Type by parsing the Connection String
        /// </summary>
        /// <param name="connectionString">The Connection String to parse</param>
        /// <returns>The NServiceBus Transport Type</returns>
        public static NServiceBusTransportType GetTransportType(string connectionString)
        {
            var settings = new string(connectionString.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant().Split(';');
            if (SettingExists(settings, "HOST="))
            {
                return NServiceBusTransportType.RabbitMq;
            }
            if (SettingExists(settings, "DATASOURCE="))
            {
                return NServiceBusTransportType.Sql;
            }
            throw new NServiceBusConnectionException(connectionString);
        }

        /// <summary>
        /// Returns true if the setting exists and is not empty
        /// </summary>
        /// <param name="settings">All the settings</param>
        /// <param name="setting">The setting to find</param>
        /// <returns>True if the setting exists and is not empty, false otherwise</returns>
        private static bool SettingExists(string[] settings, string setting)
        {
            return settings.Any(s => s.StartsWith(setting, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(s.Replace(setting, string.Empty)));
        }
    }
}
