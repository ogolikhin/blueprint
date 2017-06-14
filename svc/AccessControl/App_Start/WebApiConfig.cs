using System;
using System.Configuration;
using System.Web.Http;
using AccessControl.Controllers;
using AccessControl.Helpers;
using ServiceLibrary.Helpers;

namespace AccessControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            new SessionsController().LoadAsync();
        }

        private const int DefaultSessionTimeoutInterval = 1200;
        private const int DefaultSessionCacheExpiration = 60;

        public static string AdminStorage = ConfigurationManager.ConnectionStrings["AdminStorage"].ConnectionString;

        private static int? _sessionTimeoutInterval;
        public static int SessionTimeoutInterval
        {
            get
            {
                if (!_sessionTimeoutInterval.HasValue)
                {
                    try
                    {
                        var timeout = I18NHelper.Int32ParseInvariant(ConfigurationManager.AppSettings["SessionTimeoutInterval"]);
                        _sessionTimeoutInterval = timeout > 0 ? timeout : DefaultSessionTimeoutInterval;
                    }
                    catch (Exception)
                    {
                        _sessionTimeoutInterval = DefaultSessionTimeoutInterval;
                    }
                }

                return _sessionTimeoutInterval.Value;
            }
        }

        private static Lazy<TimeSpan> _sessionCacheExpiration = new Lazy<TimeSpan>(() => {
            int expirationTimeInSeconds;
            try
            {
                var value = I18NHelper.Int32ParseInvariant(ConfigurationManager.AppSettings["SessionCacheExpiration"]);
                expirationTimeInSeconds = value > 0 ? value : DefaultSessionCacheExpiration;
            }
            catch (Exception)
            {
                expirationTimeInSeconds = DefaultSessionCacheExpiration;
            }

            return TimeSpan.FromSeconds(expirationTimeInSeconds);
        });
        public static TimeSpan SessionCacheExpiration => _sessionCacheExpiration.Value;

        public static int LicenseHoldTime = LicenceHelper.GetLicenseHoldTime(
            ConfigurationManager.AppSettings["LHTSetting"], 1440);

        internal static string LogSourceSessions= "AccessControl.Sessions";
        internal static string LogSourceStatus = "AccessControl.Status";
        internal static string LogSourceLicenses = "AccessControl.Licenses";
    }
}
