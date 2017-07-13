﻿using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum GenerateActionTypes
    {
        None,
        Children,
        UserStories,
        TestCases
    };


    /// <summary>
    /// Generate Action 
    /// </summary>
    [XmlType("GenerateAction")]
    public class IeGenerateAction : IeBaseAction
    {
        [XmlElement(IsNullable = false)]
        public GenerateActionTypes GenerateActionType { get; set; }

        // Used only for GenerateActionType = Children
        [XmlElement(IsNullable = false)]
        public int ChildCount { get; set; }

        // Used only for GenerateActionType = Children
        [XmlElement(IsNullable = false)]
        public string ArtifactType { get; set; }
    }
}