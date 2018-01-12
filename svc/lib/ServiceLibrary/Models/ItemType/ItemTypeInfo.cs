using System.Collections.Generic;

namespace ServiceLibrary.Models.ItemType
{
    public class ItemTypeInfo
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public ItemTypePredefined Predefined { get; set; }
        public bool HasCustomIcon { get; set; }
        public IEnumerable<byte> Icon { get; set; }
    }
}