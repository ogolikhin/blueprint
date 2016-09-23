using System;
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
    }
}