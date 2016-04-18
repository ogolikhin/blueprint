using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace LoggingDatabaseModel
{
    public sealed class MockBlockingObserver : IObserver<EventEntry>, IDisposable
    {
        public ManualResetEvent ResetEvent = new ManualResetEvent(false);
        public ConcurrentQueue<EventEntry> OnNextCalls = new ConcurrentQueue<EventEntry>();
        public bool OnCompletedCalled;
        public Exception OnErrorException;

        void IObserver<EventEntry>.OnCompleted()
        {
            if (OnCompletedCalled) { throw new InvalidOperationException(); }
            this.OnCompletedCalled = true;
            this.ResetEvent.WaitOne();
        }

        void IObserver<EventEntry>.OnError(Exception error)
        {
            if (OnErrorException != null) { throw new InvalidOperationException(); }
            this.OnErrorException = error;
            this.ResetEvent.WaitOne();
        }

        void IObserver<EventEntry>.OnNext(EventEntry value)
        {
            this.OnNextCalls.Enqueue(value);
            this.ResetEvent.WaitOne();
        }

        #region IDisposable members

        /// <summary>
        /// Disposes this object explicitly.
        /// </summary>
        public void Dispose()
        {
            if (ResetEvent != null) ResetEvent.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable members

    }
}
