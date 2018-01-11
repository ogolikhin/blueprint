namespace ServiceLibrary.Models.ItemType
{
    public class ItemTypeInfo
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public ItemTypePredefined Predefined { get; set; }
        public bool HasCustomIcon { get; set; }
        public byte[] Icon { get; set; }
    }
}