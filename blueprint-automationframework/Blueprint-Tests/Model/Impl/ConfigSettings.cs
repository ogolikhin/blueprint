using System.Collections.Generic;
using Utilities;

namespace Model.Impl
{
    /// <summary>
    /// This class stores the config settings returned by AdminStore.GetSettings().
    /// </summary>
    public class ConfigSettings
    {
        public class SettingValues
        {
            public Dictionary<string, string> Values { get; internal set; }

            /// <summary>
            /// Constructor to initialize this object with the values in the specified Dictionary.
            /// </summary>
            /// <param name="settingValues">The keys/values to add to this object.</param>
            public SettingValues(Dictionary<string, string> settingValues)
            {
                Values = settingValues;
            }

            /// <summary>
            /// Index operator for convenience in accessing values.
            /// </summary>
            /// <param name="key">The dictionary key whose value you want to get.</param>
            /// <returns>The dictionary value corresponding to the specified key.</returns>
            public string this[string key]
            {
                get { return Values[key]; }
            }
        }

        public Dictionary<string, SettingValues> Settings { get; } = new Dictionary<string, SettingValues>();

        /// <summary>
        /// Constructor to initialize this object with the values in the specified Dictionary.
        /// </summary>
        /// <param name="configSettings">The keys/values to add to this object.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")] // Ignore this warning.  This is what AdminStore returns.
        public ConfigSettings(Dictionary<string, Dictionary<string, string>> configSettings)
        {
            ThrowIf.ArgumentNull(configSettings, nameof(configSettings));

            foreach (var keyPair in configSettings)
            {
                var values = new SettingValues(keyPair.Value);
                Settings.Add(keyPair.Key, values);
            }
        }

        /// <summary>
        /// Index operator for convenience in accessing values.
        /// </summary>
        /// <param name="key">The dictionary key whose value you want to get.</param>
        /// <returns>The dictionary value corresponding to the specified key.</returns>
        public SettingValues this[string key]
        {
            get { return Settings[key]; }
        }
    }
}
