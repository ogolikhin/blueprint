
namespace SearchService.Helpers
{
    public interface ISearchConfiguration
    {
        string PageSize { get; }
        string MaxItems { get; }
        string MaxSearchableValueStringSize { get; }
    }
}