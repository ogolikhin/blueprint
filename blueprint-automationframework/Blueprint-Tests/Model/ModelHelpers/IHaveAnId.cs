namespace Model.ModelHelpers
{
    /// <summary>
    /// This is an interface to expose the Id property of artifacts.
    /// </summary>
    public interface IHaveAnId
    {
        int Id { get; set; }
    }
}
