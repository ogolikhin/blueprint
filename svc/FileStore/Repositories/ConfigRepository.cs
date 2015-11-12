using System.Configuration;

namespace FileStore.Repositories
{
    public class ConfigRepository : IConfigRepository
    {
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
            get {
                if (_fileStreamDatabase == null)
                {
                    _fileStreamDatabase =
                        ConfigurationManager.ConnectionStrings["FileStreamDatabase"].ConnectionString;
                }
                return _fileStreamDatabase;
            }
        }
    }
}