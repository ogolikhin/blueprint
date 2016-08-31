using System.Configuration;
using ServiceLibrary.Helpers;

namespace FileStore.Repositories
{
    public class ConfigRepository : IConfigRepository
    {
        private static readonly ConfigRepository _instance = new ConfigRepository();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ConfigRepository() { }

        private ConfigRepository() { }

        public static ConfigRepository Instance
        {
            get
            {
                return _instance;
            }
        }

        private string _fileStoreDatabase;
        public string FileStoreDatabase
        {
            get
            {
                if (_fileStoreDatabase == null)
                {
                    _fileStoreDatabase = GetConnectionString("FileStoreDatabase");
                }

                return _fileStoreDatabase;
            }
        }

        private string _fileStreamDatabase;
        public string FileStreamDatabase
        {
            get
            {
                if (_fileStreamDatabase == null)
                {
                    _fileStreamDatabase = GetConnectionString("FileStreamDatabase");
                }

                return _fileStreamDatabase;
            }
        }

        private static int DefaultFileChunkSize = 1;

        private int _fileChunkSize;
        public int FileChunkSize
        {
            get
            {
                if (_fileChunkSize == 0)
                {
                    _fileChunkSize = 1024 * 1024 * GetConfigValue("FileChunkSize", DefaultFileChunkSize);
                }

                return _fileChunkSize;
            }
        }

        private int _legacyFileChunkSize;
        public int LegacyFileChunkSize
        {
            get
            {
                if (_legacyFileChunkSize == 0)
                {
                    // #DEBUG _legacyFileChunkSize = 1024 * 1024 * GetConfigValue("LegacyFileChunkSize", 1);
                    _legacyFileChunkSize = 4096;
                }

                return _legacyFileChunkSize;
            }
        }

        private const int DefaultCommandTimeout = 60;

        private int? _commandTimeout;
        public int CommandTimeout
        {
            get
            {
                if (!_commandTimeout.HasValue)
                {
                    var configValue = GetConfigValue("CommandTimeout", DefaultCommandTimeout);
                    if (configValue <= 0)
                    {
                        configValue = DefaultCommandTimeout;
                    }

                    _commandTimeout = configValue;
                }

                return _commandTimeout.Value;
            }
        }

        private static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        private static int GetConfigValue(string configKey, int defaultValue)
        {
            string configValue = ConfigurationManager.AppSettings[configKey];
            return configValue != null ? I18NHelper.Int32ParseInvariant(configValue) : defaultValue;
        }
    }
}
