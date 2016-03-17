namespace Model.OpenApiModel
{
    public interface IOpenApiAuthor
    {
        string Type { get; set; }
        int Id { get; set; }
        string DisplayName { get; set; }
    }
}
