using System;
using System.Configuration;

namespace ServiceLibrary.Helpers
{
    public class SessionsCacheSettings
    {
        private static readonly int DefaultSessionCacheExpiration = 30;

        private static Lazy<int> _sessionCacheExpirationSetting = new Lazy<int>(() => {
            int expirationTimeInSeconds;
            try
            {
                var value = I18NHelper.Int32ParseInvariant(ConfigurationManager.AppSettings["SessionCacheExpiration"]);
                expirationTimeInSeconds = value >= 0 ? value : DefaultSessionCacheExpiration;
            }
            catch (Exception)
            {
                expirationTimeInSeconds = DefaultSessionCacheExpiration;
            }

            return expirationTimeInSeconds;
        });


        public static bool IsSessionCacheEnabled = _sessionCacheExpirationSetting.Value > 0;

        public static TimeSpan SessionCacheExpiration = TimeSpan.FromSeconds(_sessionCacheExpirationSetting.Value);
    }
}
