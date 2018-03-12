using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Collections.Models
{
    public enum ColumnValidationStatus
    {
        AllValid = 0,
        SomeValid = 1,
        AllInvalid = 2
    }
}