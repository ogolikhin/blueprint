
namespace CustomAttributes
{
    /// <summary>
    /// A class containing strings to use in [ignore] & [explicit] attributes.
    /// </summary>
    public static class IgnoreReasons
    {
        // Please keep these names in alphabetical order.

        /// <summary>A test that works, but we aren't deploying the component being tested yet.</summary>
        public const string DeploymentNotReady = "DeploymentNotReady";

        /// <summary>A test that randomly passes & fails.</summary>
        public const string FlakyTest = "FlakyTest";

        /// <summary>This test usually fails because it creates more load than Blueprint can handle.</summary>
        public const string OverloadsTheSystem = "OverloadsTheSystem";

        /// <summary>This test should only be run manually.</summary>
        public const string ManualOnly = "ManualOnly";

        /// <summary>This test fails because of a product defect.</summary>
        public const string ProductBug = "ProductBug";

        /// <summary>This test fails because of a test defect.</summary>
        public const string TestBug = "TestBug";

        /// <summary>A test that is incomplete and still being written.</summary>
        public const string UnderDevelopment = "UnderDevelopment";
    }
}
