namespace SearchService.Helpers
{
    public interface ISearchConfigurationProvider
    {
        int PageSize { get; }
        int MaxItems { get; }
        int MaxSearchableValueStringSize { get; }
    }
}