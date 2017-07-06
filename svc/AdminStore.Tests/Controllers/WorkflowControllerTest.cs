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
    [XmlRoot("WorkflowActions")]
    [XmlType("WorkflowActions")]
    public class WorkflowActions
    {
        public WorkflowActions() { }
        
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Actions")]
        [XmlArrayItem("NotificationAction", typeof(IeNotificationAction))]
        [XmlArrayItem("PropertyAction", typeof(IePropertyAction))]
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
                "<Name>NotificationAction</Name><Description>Notfication Test</Description>" +
                "<UserGroup>456</UserGroup>" +
                "<Users><User>111</User><User>222</User></Users>" +
                "<Addresses><Email>user1@mail.com</Email><Email>user2@mail.com</Email></Addresses>" +
                "<Parameters>" +
                "<Parameter><Name>Param1</Name><Type>1</Type><Value>123</Value></Parameter>" +
                "<Parameter><Name>Param2</Name><Type>2</Type><Value>Hello</Value></Parameter>" +
                "</Parameters>" +
                "</NotificationAction>" +

                "<PropertyAction>" +
                "<Name>PropertyAction</Name><Description>PropChange Test</Description>" +
                "<Project>111111</Project>" +
                "<Artifact>22222222</Artifact>" +
                "<Parameters>" +
                "<Parameter><Name>Param1</Name><Type>1</Type><Value>123</Value></Parameter>" +
                "<Parameter><Name>Param2</Name><Type>2</Type><Value>Hello</Value></Parameter>" +
                "</Parameters>" +
                "</PropertyAction>" +
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
    }
}
