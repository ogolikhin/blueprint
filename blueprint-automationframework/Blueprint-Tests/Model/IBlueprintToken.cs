
namespace Model
{
    public interface IBlueprintToken
    {
        string AccessControlTokenHeader { get; }
        string OpenApiTokenHeader { get; }

        string AccessControlToken { get; set; }
        string OpenApiToken { get; set; }

        /// <summary>
        /// Sets the token (either for AccessControl or OpenAPI, depending on the token format).
        /// </summary>
        /// <param name="token">A token string from AccessControl/AdminStore or OpenAPI.</param>
        void SetToken(string token);
    }
}
