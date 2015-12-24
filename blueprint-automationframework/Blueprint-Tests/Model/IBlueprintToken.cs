
namespace Model
{
    public interface IBlueprintToken
    {
        string AccessControlTokenHeader { get; }
        string OpenApiTokenHeader { get; }

        string AccessControlToken { get; set; }
        string OpenApiToken { get; set; }
    }
}
