﻿using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("ValidValue")]
    public class IeValidValue
    {
        // Optional, not used for the import, will be used for the update
        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public int? Id { get; set; }

        [XmlAttribute("Id")]
        public int IdSerializable
        {
            get { return Id.GetValueOrDefault(); }
            set { Id = value; }
        }

        public bool ShouldSerializeIdSerializable()
        {
            return Id.HasValue;
        }
        //========================================================

        [XmlElement(IsNullable = false)]
        public string Value { get; set; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeValidValue other)
        {
            return Id.GetValueOrDefault() == other.Id.GetValueOrDefault() && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeValidValue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        #endregion
    }
}