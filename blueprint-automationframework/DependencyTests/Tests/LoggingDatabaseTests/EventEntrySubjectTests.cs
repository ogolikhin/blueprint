using CustomAttributes;
using Logging.Database.Utility;
using LoggingDatabaseModel;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoggingDatabaseTests
{
    [TestFixture]
    [Category(Categories.LoggingDatabase)]
    public static class EventEntrySubjectTests
    {
        [TestCase]
        [Description("EventEntrySubject should call OnNext")]
        public static void EventEntrySubject_ShouldCallOnNext()
        {
            using (var subject = new EventEntrySubject())
            {
                var observer = new MockObserver<EventEntry>();

                subject.Subscribe(observer);

                var entry1 = CreateEntry();
                var entry2 = CreateEntry();
                subject.OnNext(entry1);
                subject.OnNext(entry2);

                Assert.AreSame(entry1, observer.OnNextCalls.ElementAt(0));
                Assert.AreSame(entry2, observer.OnNextCalls.ElementAt(1));
            }
        }

        [TestCase]
        [Description("EventEntrySubject should call OnCompleted")]
        public static void EventEntrySubject_ShouldCallOnCompleted()
        {
            using (var subject = new EventEntrySubject())
            {
                var observer = new MockObserver<EventEntry>();
                subject.Subscribe(observer);

                Assert.IsFalse(observer.OnCompletedCalled);

                subject.OnCompleted();

                Assert.IsTrue(observer.OnCompletedCalled);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "For testing purposes")]
        [TestCase]
        [Description("EventEntrySubject should call OnError")]
        public static void EventEntrySubject_ShouldCallOnError()
        {
            using (var subject = new EventEntrySubject())
            {
                var observer = new MockObserver<EventEntry>();
                subject.Subscribe(observer);
                var error = new Exception();
                subject.OnError(error);

                subject.OnNext(CreateEntry());

                Assert.AreSame(error, observer.OnErrorException);
                Assert.AreEqual(0, observer.OnNextCalls.Count);
            }
        }

        [TestCase]
        [Description("EventEntrySubject Dispose Calls OnCompleted")]
        public static void EventEntrySubject_DisposeCallsOnCompleted()
        {
            var observer = new MockObserver<EventEntry>();

            using (var subject = new EventEntrySubject())
            {
                subject.Subscribe(observer);

                Assert.IsFalse(observer.OnCompletedCalled);
            }

            Assert.IsTrue(observer.OnCompletedCalled);
        }

        [TestCase]
        [Description("EventEntrySubject OnCompled stops propagating events")]
        public static void EventEntrySubject_OnCompletedStopsPropagatingEvents()
        {
            using (var subject = new EventEntrySubject())
            {
                var observer = new MockObserver<EventEntry>();
                subject.Subscribe(observer);

                subject.OnCompleted();
                subject.OnNext(CreateEntry());

                Assert.AreEqual(0, observer.OnNextCalls.Count);
            }
        }

        [TestCase]
        [Description("When EventEntrySubject Unsubscribe is called then stop propagating events")]
        public static void EventEntrySubject_UnsubscribeStopsPropagatingEvents()
        {
            using (var subject = new EventEntrySubject())
            {
                var observer = new MockObserver<EventEntry>();
                var subscription = subject.Subscribe(observer);

                subscription.Dispose();
                subject.OnNext(CreateEntry());

                Assert.AreEqual(0, observer.OnNextCalls.Count);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "For testing purposes")]
        [TestCase]
        [Description("When EventEntrySubject OnError is called then stop propagating events")]
        public static void EventEntrySubject_OnErrorStopsPropagatingEvents()
        {
            using (var subject = new EventEntrySubject())
            {
                var observer = new MockObserver<EventEntry>();
                subject.Subscribe(observer);
                subject.OnError(new Exception());

                subject.OnNext(CreateEntry());

                Assert.AreEqual(0, observer.OnNextCalls.Count);
            }
        }

        [TestCase]
        [Description("When EventEntrySubject OnCompleted is called then OnCompleted is sent to all new subsribers")]
        public static void EventEntrySubject_OnCompletedIsSentToAllNewSubscribersAfterItWasCompleted()
        {
            using (var subject = new EventEntrySubject())
            {
                subject.OnCompleted();

                var observer = new MockObserver<EventEntry>();
                subject.Subscribe(observer);

                Assert.IsTrue(observer.OnCompletedCalled);
            }
        }

        [TestCase]
        [Description("When EventEntrySubject OnCompled is called then we can Unsubscribe")]
        public static void EventEntrySubject_CanUnsubscribeNewSubscribersAfterItWasCompleted()
        {
            using (var subject = new EventEntrySubject())
            {
                subject.OnCompleted();

                var observer = new MockObserver<EventEntry>();
                var subscription = subject.Subscribe(observer);

                subscription.Dispose();
            }
        }

        [TestCase]
        [Description("When EventEntrySubject OnCompleted is called then it should call OnCompleted in parallel")]
        public static void EventEntrySubject_ShouldCallOnCompletedInParallel()
        {
            using (var observer1 = new MockBlockingObserver())
            using (var observer2 = new MockBlockingObserver())
            using (var subject = new EventEntrySubject())
            {
                subject.Subscribe(observer1);
                subject.Subscribe(observer2);

                var task = Task.Run(() => subject.OnCompleted());

                Thread.Sleep(30);

                Assert.IsTrue(observer1.OnCompletedCalled);
                Assert.IsTrue(observer2.OnCompletedCalled);

                Assert.IsFalse(task.IsCompleted);

                observer1.ResetEvent.Set();
                observer2.ResetEvent.Set();

                Assert.IsTrue(task.Wait(500));
            }
        }

        [TestCase]
        [Description("EventEntrySubject subsribe with null should throw ArgumentNullException")]
        [ExpectedException(typeof(ArgumentNullException))]
        public static void EventEntrySubject_NullSubscribe_ShouldThrow()
        {
            using (var subject = new EventEntrySubject())
            {
                subject.Subscribe(null);
            }
        }

        private static EventEntry CreateEntry(int id = 1)
        {
            return new EventEntry(Guid.Empty, id, null, null, DateTimeOffset.UtcNow, null);
        }

    }
}
