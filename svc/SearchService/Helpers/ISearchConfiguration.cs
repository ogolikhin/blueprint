
namespace SearchService.Helpers
{
    public interface ISearchConfiguration
    {
        string PageSize { get; }
        string MaxItems { get; }
        string MaxSearchableValueStringSize { get; }

        /// <summary>
        /// Search Sql Timeout in seconds. System defined default is 120 seconds.
        /// Sql server default value is 30 secs.
        /// Setting it to 0 means no timeout
        /// </summary>
        string SearchTimeout { get; }
    }
}