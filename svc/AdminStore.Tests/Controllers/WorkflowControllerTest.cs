using System;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AdminStore.Controllers
{
    /// <summary>
    /// Test Actions Container class
    /// </summary>
    [Serializable()]
    [XmlRoot("WorkflowTriggers")]
    [XmlType("WorkflowTriggers")]
    public class WorkflowTriggers
    {
        public WorkflowTriggers() { }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Triggers")]
        [XmlArrayItem("TransitionTrigger", typeof(IeTransitionTrigger))]
        [XmlArrayItem("PropertyChangeTrigger", typeof(IePropertyChangeTrigger))]
        public List<IeTrigger> Triggers { get; set; }
       
    }
    /// <summary>
    /// Test Actions Container class
    /// </summary>
    [Serializable()]
    [XmlRoot("WorkflowActions")]
    [XmlType("WorkflowActions")]
    public class WorkflowActions
    {
        public WorkflowActions() { }
        
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Actions")]
        [XmlArrayItem("NotificationAction", typeof(IeNotificationAction))]
        [XmlArrayItem("PropertyChangeAction", typeof(IePropertyChangeAction))]
        [XmlArrayItem("GenerateAction", typeof(IeGenerateAction))]
        public List<IeBaseAction> Actions { get; set; }

        public bool IsValid()
        {
            bool result = true;
            foreach(var act in Actions)
            {
                result &= act.IsValid();
            }
            return result;
        }
    }

 
    [TestClass]
    public class XmlActionsTest
    {
        /// <summary>
        /// Deserialize/Serialize XML Actions
        /// </summary>
        [TestMethod]
        public void DeserializeActions()
        {

            string xml = 
                "<WorkflowActions>" +
                "<Actions>" +
                    "<NotificationAction>" +
                        "<Name>NotificationAction</Name>" +
                        "<Description>Notification Test</Description>" +
                        "<Groups>" +
                            "<Group>Group111</Group>" +
                            "<Group>Group222</Group>" +
                        "</Groups>" +
                        "<Users>" +
                            "<User>User111</User>" +
                            "<User>User222</User>" +
                        "</Users>" +
                        "<Emails>" +
                            "<Email>user1@mail.com</Email>" +
                            "<Email>user2@mail.com</Email>" +
                        "</Emails>" +
                        "<PropertyTarget>Property Name</PropertyTarget>" +
                        "<Message>Property was changed</Message>" +
                    "</NotificationAction>" +
                    "<PropertyChangeAction>" +
                        "<Name>Property Change</Name>" +
                        "<Description>Property Change Test</Description>" +
                        "<Group>Group Admin</Group>" +
                        "<User>User1111</User>" +
                        "<PropertyName>Standard Property</PropertyName>" +
                        "<PropertyValue>1111111111111-2222222222</PropertyValue>" +
                        "<PropertyValueType>Text</PropertyValueType>" +
                    "</PropertyChangeAction>" +
                    "<GenerateAction>" +
                        "<Name>Generate Action</Name>" +
                        "<Description>Generate Action Test</Description>" +
                        "<Childs>3</Childs>" +
                        "<ArtifactType>UserStory</ArtifactType>" +
                    "</GenerateAction>" +
                "</Actions>" +
                "</WorkflowActions>";

            // Test Deserialization of imported XML Actions
            WorkflowActions result = null;
            try
            {
                result = SerializationHelper.FromXml<WorkflowActions>(xml);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null && result.IsValid());

            // Test resulting Actions Serialization
            try
            {
                string xmlActions = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlActions);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                result = null;
            }

            return;
        }

        /// <summary>
        /// Deserialize/Serialize XML Triggers
        /// </summary>
        [TestMethod]
        public void DeserializeTriggers()
        {

            string xml =
                "<WorkflowTriggers>" +
                    "<Triggers>" +
                        "<TransitionTrigger>" +
                            "<TriggerType>1</TriggerType>"+
                            "<Name>TestTransition</Name>" +
                            "<Description>Trigger Deserialization test</Description>" + 
                            "<FromState>Begin</FromState>" +
                            "<ToState>TheEnd</ToState>" + 
                            "<Actions></Actions>" +
                            "<PermissionGroups></PermissionGroups>" +
                        "</TransitionTrigger>" +
                        "<PropertyChangeTrigger>" +
                            "<TriggerType>2</TriggerType>" +
                            "<Name>TestPropChange</Name>" +
                            "<Description>PropChangeTrigger Deserialization test</Description>" +
                            "<FromState>Begin</FromState>" +
                            "<ToState>TheEnd</ToState>" +
                            "<Actions></Actions>" +
                            "<PermissionGroups></PermissionGroups>" +
                        "</PropertyChangeTrigger>" +
                    "</Triggers>" +
                "</WorkflowTriggers>";

            // Test Deserialization of imported XML Triggers
            WorkflowTriggers result = null;
            try
            {
                result = SerializationHelper.FromXml<WorkflowTriggers>(xml);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null);

            // Serialize test
            try
            {
                string xmlTriggers = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlTriggers);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                result = null;
            }
        }
    }
}
