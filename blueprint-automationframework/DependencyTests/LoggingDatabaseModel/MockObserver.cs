using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoggingDatabaseModel
{
    public sealed class MockObserver<T> : IObserver<T>
    {
        public ConcurrentQueue<T> OnNextCalls = new ConcurrentQueue<T>();
        public bool OnCompletedCalled;
        public Exception OnErrorException;

        void IObserver<T>.OnCompleted()
        {
            if (OnCompletedCalled) { throw new InvalidOperationException(); }
            this.OnCompletedCalled = true;
        }

        void IObserver<T>.OnError(Exception error)
        {
            if (OnErrorException != null) { throw new InvalidOperationException(); }
            this.OnErrorException = error;
        }

        void IObserver<T>.OnNext(T value)
        {
            this.OnNextCalls.Enqueue(value);
        }
    }

}
