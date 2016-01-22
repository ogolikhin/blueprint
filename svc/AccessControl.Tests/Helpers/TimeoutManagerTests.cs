using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories.ConfigControl;

namespace AccessControl.Helpers
{
    [TestClass]
    public class TimeoutManagerTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var timeoutManager = new TimeoutManager<int>();

            // Assert
            Assert.IsInstanceOfType(timeoutManager.Timer, typeof(TimerWrapper));
        }

        #endregion Constructor

        #region Insert

        [TestMethod]
        public void Insert_InOrder_UpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(10.0);
            DateTime timeout2 = now.AddMinutes(20.0);

            // Act
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, null);

            // Assert
            Assert.AreEqual(2, timeoutManager.Items.Count);
            Assert.AreEqual(2, timeoutManager.TimeoutsByItem.Count);
            Assert.AreEqual((timeout1 - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        [TestMethod]
        public void Insert_SameItemWithNewTimeout_UpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout = now.AddMinutes(20.0);
            timeoutManager.Insert(1, timeout, null);
            timeout = now.AddMinutes(10.0);

            // Act
            timeoutManager.Insert(1, timeout, null);

            // Assert
            Assert.AreEqual(1, timeoutManager.Items.Count);
            Assert.AreEqual(1, timeoutManager.TimeoutsByItem.Count);
            Assert.AreEqual((timeout - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        [TestMethod]
        public void Insert_InReverseOrder_UpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(20.0);
            DateTime timeout2 = now.AddMinutes(10.0);

            // Act
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, null);

            // Assert
            Assert.AreEqual(2, timeoutManager.Items.Count);
            Assert.AreEqual(2, timeoutManager.TimeoutsByItem.Count);
            Assert.AreEqual((timeout2 - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        [TestMethod]
        public void Insert_TwoDifferentItemsWithSameTimeout_UpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(20.0);
            DateTime timeout2 = now.AddMinutes(20.0);

            // Act
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, null);

            // Assert
            Assert.AreEqual(2, timeoutManager.Items.Count);
            Assert.AreEqual(2, timeoutManager.TimeoutsByItem.Count);
            Assert.AreEqual((timeout2 - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        [TestMethod]
        public void Insert_ExpiredItem_InvokesCallbackAndUpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(20.0);
            DateTime timeout2 = now.AddMinutes(-20.0);
            bool callbackInvoked = false;
            Action callback = () => { callbackInvoked = true; };

            // Act
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, callback);

            // Assert
            Assert.AreEqual(1, timeoutManager.Items.Count);
            Assert.AreEqual(1, timeoutManager.TimeoutsByItem.Count);
            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual((timeout1 - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Insert_AfterDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            timeoutManager.Dispose();
            DateTime timeout = now.AddMinutes(20.0);

            // Act
            timeoutManager.Insert(1, timeout, null);

            // Assert
        }

        #endregion Insert

        #region TimerOnElapsed

        [TestMethod]
        public void TimerOnElapsed_Always_InvokesCallbackAndUpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout = now.AddMinutes(20.0);
            bool callbackInvoked = false;
            Action callback = () => { callbackInvoked = true; };
            timeoutManager.Insert(1, timeout, callback);

            // Act
            timer.Setup(t => t.Now()).Returns(timeout);
            timer.Raise(t => t.Elapsed += null, (EventArgs)null);

            // Assert
            Assert.AreEqual(0, timeoutManager.Items.Count);
            Assert.AreEqual(0, timeoutManager.TimeoutsByItem.Count);
            Assert.IsTrue(callbackInvoked);
            Assert.IsFalse(timer.Object.Enabled);
        }

        [TestMethod]
        public void TimerOnElapsed_Exception_CaughtAndLogged()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout = now.AddMinutes(20.0);
            Action callback = () => { };
            timeoutManager.Insert(1, timeout, callback);
            Exception ex = new Exception();

            // Act
            timer.Setup(t => t.Now()).Returns(timeout);
            timer.SetupSet(t => t.Enabled = It.IsAny<bool>()).Throws(ex);
            timer.Raise(t => t.Elapsed += null, (EventArgs)null);

            // Assert
            log.Verify(l => l.LogError(WebApiConfig.LogSourceSessions, ex,
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        #endregion TimerOnElapsed

        #region Remove

        [TestMethod]
        public void Remove_EarliestItem_UpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(20.0);
            DateTime timeout2 = now.AddMinutes(10.0);
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, null);

            // Act
            timeoutManager.Remove(2);

            // Assert
            Assert.AreEqual(1, timeoutManager.Items.Count);
            Assert.AreEqual(1, timeoutManager.TimeoutsByItem.Count);
            Assert.AreEqual((timeout1 - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        [TestMethod]
        public void Remove_AllItems_UpdatesDictionariesAndTimer()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(20.0);
            DateTime timeout2 = now.AddMinutes(10.0);
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, null);

            // Act
            timeoutManager.Remove(2);
            timeoutManager.Remove(1);

            // Assert
            Assert.AreEqual(0, timeoutManager.Items.Count);
            Assert.AreEqual(0, timeoutManager.TimeoutsByItem.Count);
            Assert.IsFalse(timer.Object.Enabled);
        }

        [TestMethod]
        public void Remove_NonExistantItems_HasNoEffect()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var timer = CreateTimer(now);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);
            DateTime timeout1 = now.AddMinutes(20.0);
            DateTime timeout2 = now.AddMinutes(10.0);
            timeoutManager.Insert(1, timeout1, null);
            timeoutManager.Insert(2, timeout2, null);

            // Act
            timeoutManager.Remove(3);

            // Assert
            Assert.AreEqual(2, timeoutManager.Items.Count);
            Assert.AreEqual(2, timeoutManager.TimeoutsByItem.Count);
            Assert.AreEqual((timeout2 - now).TotalMilliseconds, timer.Object.Interval);
            Assert.IsTrue(timer.Object.Enabled);
        }

        #endregion Remove

        #region Dispose

        [TestMethod]
        public void Dispose_Always_DisposesAndNullsTimer()
        {
            // Arrange
            var timer = CreateTimer(DateTime.UtcNow);
            var log = new Mock<IServiceLogRepository>();
            var timeoutManager = new TimeoutManager<int>(timer.Object, log.Object);

            // Act
            timeoutManager.Dispose();

            // Assert
            timer.Verify(t => t.Dispose());
            Assert.IsNull(timeoutManager.Timer);
        }

        #endregion Dispose

        private static Mock<ITimer> CreateTimer(DateTime now)
        {
            var timer = new Mock<ITimer>();
            timer.Setup(t => t.Now()).Returns(now);
            timer.SetupProperty(t => t.Interval);
            timer.SetupProperty(t => t.Enabled);
            return timer;
        }
    }
}
