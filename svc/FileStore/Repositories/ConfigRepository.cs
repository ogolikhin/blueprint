using System.Configuration;
using ServiceLibrary.Helpers;

namespace FileStore.Repositories
{
    public class ConfigRepository : IConfigRepository
    {
        private readonly static object Locker = new object();

        private ConfigRepository() { }

        private static ConfigRepository _instance;
        public static ConfigRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        string _fileStoreDatabase;
        public string FileStoreDatabase
        {
            get
            {
                if (_fileStoreDatabase == null)
                {
                    _fileStoreDatabase =
                        ConfigurationManager.ConnectionStrings["FileStoreDatabase"].ConnectionString;
                }
                return _fileStoreDatabase;
            }
        }

        string _fileStreamDatabase;
        public string FileStreamDatabase
        {
            get
            {
                if (_fileStreamDatabase == null)
                {
                    _fileStreamDatabase =
                         ConfigurationManager.ConnectionStrings["FileStreamDatabase"].ConnectionString;
                }
                return _fileStreamDatabase;
            }
        }

        int _fileChunkSize;
        public int FileChunkSize
        {
            get
            {
                if (_fileChunkSize == 0)
                {
                    _fileChunkSize = 1024 * 1024 * GetConfigValue("FileChunkSize", 1);
                }
                return _fileChunkSize;
            }
        }

        int _legacyFileChunkSize;
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

        public static int GetConfigValue(string configValue, int defaultValue)
        {
            return (ConfigurationManager.AppSettings[configValue] != null ? I18NHelper.Int32ParseInvariant(ConfigurationManager.AppSettings[configValue]) : defaultValue);
        }
    }
}
