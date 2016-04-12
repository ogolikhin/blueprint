
namespace CustomAttributes
{
    public static class Categories
    {
        // Please keep these names in alphabetical order.

        public const string AccessControl = "AccessControl";
        public const string AccessControlDouble = "AccessControlDouble";
        public const string AdminStore = "AdminStore";

        /// <summary>
        /// This test runs several operations in multiple threads.
        /// </summary>
        public const string ConcurrentTest = "ConcurrentTest";

        public const string ConfigControl = "ConfigControl";
        public const string Filestore = "Filestore";

        /// <summary>
        /// This test Injects errors into the AccessControlDouble and therefore cannot be run concurrently with other tests.
        /// </summary>
        public const string InjectsErrorsIntoAccessControl = "InjectsErrorsIntoAccessControl";

        public const string LoggingDatabase = "LoggingDatabase";

        public const string OpenApi = "OpenApi";
        public const string Storyteller = "Storyteller";
    }
}
