
namespace CustomAttributes
{
    /// <summary>
    /// Contains category strings related to when or how often to run tests.
    /// </summary>
    public static class Execute
    {
        /// <summary>
        /// This tests will be run weekly.
        /// </summary>
        public const string Weekly = "Weekly";

        /// <summary>
        /// This tests will be run daily.
        /// </summary>
        public const string Daily = "Daily";

        /// <summary>
        /// This tests will be run every two hours.
        /// </summary>
        public const string BiHourly = "BiHourly";

        /// <summary>
        /// This tests will be run hourly.
        /// </summary>
        public const string Hourly = "Hourly";
    }
}
