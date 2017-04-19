using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Model.NovaModel.Metadata
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Models/Metadata/ItemTypesInformation.cs
    [Serializable]
    public class ItemTypesInformation : Dictionary<int, ItemTypeInformation>
    {
        public ItemTypesInformation()
        {
        }

        protected ItemTypesInformation(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
