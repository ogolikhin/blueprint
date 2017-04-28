
namespace CustomAttributes
{
    public static class Categories
    {
        // Please keep these names in alphabetical order.

        public const string AccessControl = "AccessControl";
        public const string AccessControlDouble = "AccessControlDouble";
        public const string AdminPortal = "AdminPortal";
        public const string AdminStore = "AdminStore";
        public const string ArtifactStore = "ArtifactStore";
        public const string ArtifactVersion = "ArtifactVersion";

        /// <summary>
        /// This test cannot be run in parallel with other tests.
        /// </summary>
        public const string CannotRunInParallel = "CannotRunInParallel";

        /// <summary>
        /// This test runs several operations in multiple threads.
        /// </summary>
        public const string ConcurrentTest = "ConcurrentTest";

        public const string ConfigControl = "ConfigControl";

        /// <summary>
        /// This tests runs with predefined data project "Custom Data".
        /// </summary>
        public const string CustomData = "CustomData";

        public const string FileStore = "FileStore";

        /// <summary>
        /// This test uses "Golden" pre-created data, so it won't pass on machines that don't contain that pre-created data.
        /// </summary>
        public const string GoldenData = "GoldenData";

        public const string ImpactAnalysis = "ImpactAnalysis";

        /// <summary>
        /// This test Injects errors into the AccessControlDouble and therefore cannot be run concurrently with other tests.
        /// </summary>
        public const string InjectsErrorsIntoAccessControl = "InjectsErrorsIntoAccessControl";

        public const string LoggingDatabase = "LoggingDatabase";

        public const string Navigation = "Navigation";
        public const string OpenApi = "OpenApi";

        public const string SearchService = "SearchService";

        public const string Storyteller = "Storyteller";

        /// <summary>
        /// This tests the Automation Framework itself, not the Blueprint product.
        /// </summary>
        public const string UtilityTest = "UtilityTest";
    }
}
