using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// Enumeration of available Actions
    /// Note: not in use for now
    public enum ActionTypes
    {
        None,
        EmailNotification,
        PropertyUpdateChoice,
        PropertyUpdateUser,
        PropertyUpdateDate,
        PropertyUpdateText,
        PropertyUpdateNumber,
        GenerateTestCase,
        GenerateUserStory,
        GenerateChildArtifact
    };

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
    [Serializable()]
    public abstract class IeBaseAction
    {
        public IeBaseAction() { }
        public string Name { get; set; }

        public string Description { get; set; }

        //[XmlElement("Type")]
        //public int ActionType { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Parameters"), XmlArrayItem("Parameter")]
        public List<IeActionParameter> Parameters { get; set; }

        public virtual bool IsValid()
        {
            bool result = true;
            foreach (var par in Parameters)
            {
                result &= par.IsValid();
            }
            return result;
        }
    }

}