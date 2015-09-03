using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace FileStore.Helpers
{
    public class WebConfig
    {
        public const string Undefined = "Undefined";
        public static string GetAppSetting(string name)
        {
            try
            {
                return WebConfigurationManager.AppSettings[name];
            }
            catch
            {
                return Undefined;
            }
        }

        public static string GetConnectionString(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch
            {
                return Undefined;
            }
        } 
    }
}