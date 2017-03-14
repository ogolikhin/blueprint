using System;

namespace Model.ArtifactModel.Enums
{
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ItemIndicatorFlags : uint
    {
        None = 0x0,
        HasComments = 0x1,
        HasAttachmentsOrDocumentRefs = 0x2,
        HasManualReuseOrOtherTraces = 0x4,
        HasLast24HoursChanges = 0x8,
        HasUIMockup = 0x10
        ////None
        //None = 0x0000
    }
}
