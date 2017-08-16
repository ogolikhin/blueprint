using System;
using AdminStore.Helpers.Workflow;
using AdminStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Models.Workflow
{
    [TestClass]
    public class IeWorkflowTests
    {
        [TestMethod]
        public void Equals_Equal_True()
        {
            // Arrange
            var workflow = WorkflowTestHelper.TestWorkflow;
            var clone = WorkflowHelper.CloneViaXmlSerialization(workflow);
            
            // Act
            var actual = workflow.Equals(clone);

            // Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void Equals_NotEqual_False()
        {
            // Arrange
            var workflow = WorkflowTestHelper.TestWorkflow;
            var clone = WorkflowHelper.CloneViaXmlSerialization(workflow);
            (clone.TransitionEvents[0].Triggers[0].Action as IeEmailNotificationAction).Emails[0] = "changed";

            // Act
            var actual = workflow.Equals(clone);

            // Assert
            Assert.AreEqual(false, actual);
        }
    }
}
