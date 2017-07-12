using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // Enumeration of data types
    public enum ActionDataTypes
    {
        None,
        Number,
        Text,
        Date
    }

    /// <summary>
    /// Base class for Actions of specific type
    /// </summary>
    public abstract class IeBaseAction
    {
        public string Name { get; set; }

        public string Description { get; set; }
        
    }

}