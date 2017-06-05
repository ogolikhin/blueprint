namespace Model.NovaModel.AdminStoreModel
{
    public class CreateResult
    {
        public static CreateResult Empty => new CreateResult { TotalCreated = 0 };

        public int TotalCreated { get; set; }
    }
}
