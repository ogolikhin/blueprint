using System.Configuration;

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
                    _fileChunkSize = 1024 * 1024 * ConfigValue("FileChunkSize", 1);
                }
                return _fileChunkSize;
            }
        }

        public static int ConfigValue(string configValue, int defaultValue)
        {
            return (ConfigurationManager.AppSettings[configValue] != null ? int.Parse(ConfigurationManager.AppSettings[configValue]) : defaultValue);
        }
    }
}