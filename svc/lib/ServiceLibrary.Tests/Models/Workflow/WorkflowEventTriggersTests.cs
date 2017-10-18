using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow
{
    [TestClass]
    public class WorkflowEventTriggersTests
    {
        private Mock<IWorkflowEventAction> _workflowEventAction;
        private string defaultTriggerName = "Test Trigger Name";
        private int fakePropertyTypeId = -1;
        [TestInitialize]
        public void Initialize()
        {
            _workflowEventAction = new Mock<IWorkflowEventAction>();
        }

        [TestMethod]
        public async Task ProcessTriggers_WhenValidationErrors_ReturnsErrorMessage()
        {
            var triggers = new WorkflowEventTriggers();
            _workflowEventAction.Setup(a => a.ValidateAction(It.IsAny<IExecutionParameters>())).Returns(new PropertySetResult(fakePropertyTypeId, -1, ""));

            triggers.Add(new WorkflowEventTrigger() {Action = _workflowEventAction.Object, Name = defaultTriggerName });

            var result = await triggers.ProcessTriggers(null);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.ContainsKey(defaultTriggerName + fakePropertyTypeId));
        }

        [TestMethod]
        public async Task ProcessTriggers_WhenDuplicateTriggerNameFailsValidation_ReturnsErrorMessage()
        {
            var triggers = new WorkflowEventTriggers();
            _workflowEventAction.Setup(a => a.ValidateAction(It.IsAny<IExecutionParameters>())).Returns(new PropertySetResult(-1, -1, ""));

            triggers.Add(
                new WorkflowEventTrigger() { Action = _workflowEventAction.Object, Name = defaultTriggerName });
            triggers.Add(
                new WorkflowEventTrigger() { Action = _workflowEventAction.Object, Name = defaultTriggerName });

            var result = await triggers.ProcessTriggers(null);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.ContainsKey(defaultTriggerName + fakePropertyTypeId));
            Assert.IsTrue(result.Keys.Count == 1);
        }
    }
}
