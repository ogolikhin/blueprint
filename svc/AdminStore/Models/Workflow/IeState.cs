﻿using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("State")]
    public class IeState
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
        public string Name { get; set; }

        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IsInitial { get; set; }

        [XmlAttribute("IsInitial")]
        public bool IsInitialSerializable
        {
            get { return IsInitial.GetValueOrDefault(); }
            set { IsInitial = value; }
        }

        public bool ShouldSerializeIsInitialSerializable()
        {
            return IsInitial.HasValue;
        }
        //========================================================
    }
}