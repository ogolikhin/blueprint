using System.Configuration;

namespace SearchService.Helpers
{
    public class SearchConfiguration : ISearchConfiguration
    {
        public string MaxItems
        {
            get
            {
                return ConfigurationManager.AppSettings["MaxItems"];
            }
        }

        public string MaxSearchableValueStringSize
        {
            get
            {
                return ConfigurationManager.AppSettings["MaxSearchableValueStringSize"];
            }
        }

        public string PageSize
        {
            get
            {
                return ConfigurationManager.AppSettings["PageSize"];
            }
        }

        /// <summary>
        /// Search Sql Timeout in seconds. System defined default is 120 seconds.
        /// Sql server default value is 30 secs.
        /// Setting it to 0 means no timeout
        /// </summary>
        public string SearchTimeout
        {
            get
            {
                return ConfigurationManager.AppSettings["SearchTimeout"];
            }
        }

    }
}