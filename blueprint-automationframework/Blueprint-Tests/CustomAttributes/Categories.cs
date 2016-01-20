
namespace CustomAttributes
{
    public static class Categories
    {
        // Please keep these names in alphabetical order.

        public const string AccessControl = "AccessControl";
        public const string AccessControlDouble = "AccessControlDouble";
        public const string AdminStore = "AdminStore";
        public const string ConcurrentTest = "ConcurrentTest";
        public const string Filestore = "Filestore";

        /// <summary>
        /// This test Injects errors into the AccessControlDouble and therefore cannot be run concurrently with other tests.
        /// </summary>
        public const string InjectsErrorsIntoAccessControl = "InjectsErrorsIntoAccessControl";

        public const string OpenApi = "OpenApi";
    }
}
