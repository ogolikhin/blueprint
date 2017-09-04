using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ServiceLibrary.Models.Workflow
{
    [TestClass]
    public class WorkflowEventTriggersTests
    {
        private Mock<IWorkflowEventAction> _workflowEventAction;
        private string defaultTriggerName = "Test Trigger Name";
        [TestInitialize]
        public void Initialize()
        {
            _workflowEventAction = new Mock<IWorkflowEventAction>();
        }

        [TestMethod]
        public async Task ProcessTriggers_WhenValidationErrors_ReturnsErrorMessage()
        {
            var triggers = new WorkflowEventTriggers();
            _workflowEventAction.Setup(a => a.ValidateAction(It.IsAny<IExecutionParameters>())).Returns(false);

            triggers.Add(new WorkflowEventTrigger() {Action = _workflowEventAction.Object, Name = defaultTriggerName });

            var result = await triggers.ProcessTriggers(null);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.ContainsKey(defaultTriggerName));
        }

        [TestMethod]
        public async Task ProcessTriggers_WhenDuplicateTriggerNameFailsValidation_ReturnsErrorMessage()
        {
            var triggers = new WorkflowEventTriggers();
            _workflowEventAction.Setup(a => a.ValidateAction(It.IsAny<IExecutionParameters>())).Returns(false);

            triggers.Add(
                new WorkflowEventTrigger() { Action = _workflowEventAction.Object, Name = defaultTriggerName }
                );
            triggers.Add(
                new WorkflowEventTrigger() { Action = _workflowEventAction.Object, Name = defaultTriggerName }
                );

            var result = await triggers.ProcessTriggers(null);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.ContainsKey(defaultTriggerName));
            Assert.IsTrue(result.Keys.Count == 1);
        }
    }
}
