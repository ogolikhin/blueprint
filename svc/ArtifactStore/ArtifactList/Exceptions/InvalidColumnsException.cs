using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using ArtifactStore.ArtifactList.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.ArtifactList.Exceptions
{
    [Serializable]
    public class InvalidColumnsException : Exception
    {
        public IEnumerable<ProfileColumn> ProfileColumns { get; }

        public InvalidColumnsException(IEnumerable<ProfileColumn> items)
        {
            ProfileColumns = items;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(ServiceConstants.ErrorContentName, ProfileColumns);
        }
    }
}
