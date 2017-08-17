using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Helpers.Workflow
{
    [TestClass]
    public class WorkflowHelperTests
    {
        [TestMethod]
        public void CollectionEquals_NullNull_True()
        {
            // Assert
            Assert.IsTrue(WorkflowHelper.CollectionEquals<int>(null, null));
        }

        [TestMethod]
        public void CollectionEquals_FirstNull_False()
        {
            // Assert
            Assert.IsFalse(WorkflowHelper.CollectionEquals(null, new List<int> { 1 }));
        }

        [TestMethod]
        public void CollectionEquals_SecondNull_False()
        {
            // Assert
            Assert.IsFalse(WorkflowHelper.CollectionEquals(new List<int> { 1 }, null));
        }

        [TestMethod]
        public void CollectionEquals_CountsNotEqual_False()
        {
            // Assert
            Assert.IsFalse(WorkflowHelper.CollectionEquals(new List<int> { 1, 2 }, new List<int> { 1 }));
        }

        [TestMethod]
        public void CollectionEquals_Equals_True()
        {
            // Assert
            Assert.IsTrue(WorkflowHelper.CollectionEquals(new List<int> { 1, 2 }, new List<int> { 1, 2 }));
        }
    }
}
