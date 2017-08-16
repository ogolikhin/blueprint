using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    /// <summary>
    /// Base class for Actions of specific type
    /// </summary>
    [XmlType("BaseAction")]
    public abstract class IeBaseAction
    {
        [XmlIgnore]
        public abstract ActionTypes ActionType { get; }

        // Not used, but we keep it for now
        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeBaseAction other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeBaseAction) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        #endregion
    }

}