using ServiceLibrary.Models;

namespace ArtifactStore.Models.Reuse
{
    public class SqlItemTypeInfo
    {
        public int ItemId { get; set; }

        public int TypeId { get; set; }

        public int? InstanceTypeId { get; set; }

        public ItemTypePredefined ItemTypePredefined { get; set; }
    }
}