
namespace SearchService.Helpers
{
    public interface IConfiguration
    {
        string PageSize { get; }
        string MaxItems { get; }
        string MaxSearchableValueStringSize { get; }
    }
}