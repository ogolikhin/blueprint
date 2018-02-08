using System.Diagnostics.CodeAnalysis;

namespace ServiceLibrary.Models.ItemType
{
    public class ItemTypeInfo
    {
        public int Id { get; set; }

        public string Prefix { get; set; }

        public ItemTypePredefined Predefined { get; set; }

        public bool HasCustomIcon { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:Properties should not return arrays")]
        public byte[] Icon { get; set; }

        public bool IsPrimitiveType { get; set; }
    }
}