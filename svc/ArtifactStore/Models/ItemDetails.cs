using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Repositories
{
    internal class ItemDetails
    {
        internal int HolderId;
        internal string Name;
        internal int PrimitiveItemTypePredefined;
        internal string Prefix;
        internal int ItemTypeId;
    }

    public class ItemLabel
    {
        internal int ItemId { get; set; }
        internal string Label { get; set; }
    }

}