using System;
using System.Configuration;

namespace FileStore.Repositories
{
    public class ConfigRepository : IConfigRepository
    {
        string _fileStoreDatabase;
        string IConfigRepository.FileStoreDatabase
        {
            get {
                return _fileStoreDatabase ??
                       (_fileStoreDatabase =
                           ConfigurationManager.ConnectionStrings["FileStoreDatabase"].ConnectionString);
            }
        }

        string _fileStreamDatabase;
        string IConfigRepository.FileStreamDatabase
        {
            get {
                return _fileStreamDatabase ??
                       (_fileStreamDatabase =
                           ConfigurationManager.ConnectionStrings["FileStreamDatabase"].ConnectionString);
            }
        }
    }
}