using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("NewArtifact")]
    public class IeNewArtifactEvent : IeEvent
    {
        [XmlIgnore]
        public override EventTypes EventType => EventTypes.NewArtifact;

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeNewArtifactEvent other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeNewArtifactEvent)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

    }
}