using System.Linq;
using ActionHandlerService.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Workflow;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the ActionsParser
    /// </summary>
    [TestClass]
    public class ActionsParserTests
    {
        private ActionsParser _actionsParser;

        [TestInitialize]
        public void TestInitialize()
        {
            _actionsParser = new ActionsParser();
        }

        [TestMethod]
        public void ActionsParser_ReturnsNoActions_WhenTriggersAreNull()
        {
            const string triggers = null;
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = triggers
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(0, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsNoActions_WhenTriggersAreEmpty()
        {
            const string triggers = "";
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = triggers
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(0, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsNoActions_WhenTriggersAreWhitespace()
        {
            const string triggers = " ";
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = triggers
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(0, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsNoActions_WhenTriggersAreInvalid()
        {
            const string triggers = "Invalid Trigger XML";
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = triggers
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(0, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsANotificationAction_WhenATriggerExists()
        {
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = "<TSR><TS><T><N>Trigger 1</N><AEN><ES><E>test1@blueprintsys.com</E></ES><M>Message 1</M></AEN><SC><SID>111</SID></SC></T></TS></TSR>",
                    EventPropertyTypeId = 1
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(1, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsMultipleNotificationActions_WhenMutipleTriggersExist()
        {
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = "<TSR><TS><T><N>Trigger 1</N><AEN><ES><E>test1@blueprintsys.com</E></ES><M>Message 1</M></AEN><SC><SID>111</SID></SC></T><T><N>Trigger 2</N><AEN><ES><E>test2@blueprintsys.com</E></ES><M>Message 2</M></AEN><SC><SID>222</SID></SC></T></TS></TSR>",
                    EventPropertyTypeId = 1
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(2, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsMultipleNotificationActions_WhenMultipleEventsExist()
        {
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = "<TSR><TS><T><N>Trigger 1</N><AEN><ES><E>test1@blueprintsys.com</E></ES><M>Message 1</M></AEN><SC><SID>111</SID></SC></T></TS></TSR>",
                    EventPropertyTypeId = 1
                },
                new SqlWorkflowEvent
                {
                    Triggers = "<TSR><TS><T><N>Trigger 2</N><AEN><ES><E>test2@blueprintsys.com</E></ES><M>Message 2</M></AEN><SC><SID>222</SID></SC></T></TS></TSR>",
                    EventPropertyTypeId = 2
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(2, notificationActions.Count);
        }

        [TestMethod]
        public void ActionsParser_ReturnsAnActionWithMultipleEmails_WhenTheTriggerHasMultipleEmails()
        {
            const string toEmail1 = "test1@blueprintsys.com";
            const string toEmail2 = "test2@blueprintsys.com";
            const int conditionalStateId = 111;
            var triggers = $"<TSR><TS><T><N>Trigger 1</N><AEN><ES><E>{toEmail1}</E><E>{toEmail2}</E></ES><M>Message 1</M></AEN><SC><SID>{conditionalStateId}</SID></SC></T></TS></TSR>";
            const int eventPropertyTypeId = 123;
            var workflowEvents = new[]
            {
                new SqlWorkflowEvent
                {
                    Triggers = triggers,
                    EventPropertyTypeId = eventPropertyTypeId
                }
            };
            var notificationActions = _actionsParser.GetNotificationActions(workflowEvents);
            Assert.AreEqual(1, notificationActions.Count);
            var notificationAction = notificationActions.Single();
            Assert.AreEqual(conditionalStateId, notificationAction.ConditionalStateId);
            Assert.AreEqual(eventPropertyTypeId, notificationAction.PropertyTypeId);
            var toEmails = notificationAction.Emails.ToList();
            Assert.AreEqual(2, toEmails.Count);
            Assert.AreEqual(1, toEmails.Count(e => e == toEmail1));
            Assert.AreEqual(1, toEmails.Count(e => e == toEmail2));
        }
    }
}
