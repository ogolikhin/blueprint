using System;

namespace ServiceLibrary.Models
{
    public class ItemDetails
    {
        public int HolderId;
        public string Name;
        public int PrimitiveItemTypePredefined;
        public string Prefix;
        public int ItemTypeId;
    }

    public class ItemLabel
    {
        public int ItemId { get; set; }
        public string Label { get; set; }
    }
    public class ItemRawDataCreatedDate
    {
        public int ItemId { get; set; }
        public string RawData { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}