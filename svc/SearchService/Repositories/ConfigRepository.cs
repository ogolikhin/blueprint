using System.Configuration;
using ServiceLibrary.Repositories.ConfigControl;

namespace SearchService.Repositories
{
    public class ConfigRepository : IConfigRepository
    {
        private static readonly object Locker = new object();

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

        string _blueprintDatabase;
        public string BlueprintDatabase
        {
            get
            {
                if (_blueprintDatabase == null)
                {
                    _blueprintDatabase =
                        ConfigurationManager.ConnectionStrings["Blueprint"].ConnectionString;
                }
                return _blueprintDatabase;
            }
        }

    }
}
