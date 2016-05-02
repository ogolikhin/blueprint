using System.Configuration;

namespace ServiceLibrary.Helpers
{
    public static class ServiceConstants
    {
        public const string SessionProperty = "Session";

        public const string ErrorCodeName = "ErrorCode";

        public static string RaptorMain = ConfigurationManager.ConnectionStrings["RaptorMain"].ConnectionString;
    }
}
