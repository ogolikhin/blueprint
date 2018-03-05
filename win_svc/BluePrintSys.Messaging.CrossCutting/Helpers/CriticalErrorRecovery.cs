using BluePrintSys.Messaging.CrossCutting.Logging;
using NServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrintSys.Messaging.CrossCutting.Helpers
{
    public class CriticalErrorRecovery
    {
        const int Requested = 1;
        const int NotRequested = 0;

        private readonly int _retryCount;
        private readonly TimeSpan _delay;
        private readonly Func<Task> _createEndPoint;
        private readonly Action _onFailure;
        private readonly bool _ignoreCriticalErrors;
        private int _isRecoveryRequested = NotRequested;

        public CriticalErrorRecovery(int retryCount, TimeSpan delay, Func<Task> createEndPoint, Action onFailure, bool ignoreCriticalErrors = false)
        {
            _retryCount = retryCount;
            _delay = delay;
            _createEndPoint = createEndPoint;
            _onFailure = onFailure;
            _ignoreCriticalErrors = ignoreCriticalErrors;
        }

        public void TryToRecover()
        {
            if (Interlocked.CompareExchange(ref _isRecoveryRequested, Requested, NotRequested) == Requested)
            {
                // Already requested
                return;
            }

            Task.Factory.StartNew(Recover);
        }

        private async Task Recover()
        {
            for (int i = 0; i < _retryCount; i++)
            {
                Log.Info("ReCreating EndPoint: " + i);
                await Task.Delay(_delay);
                try
                {
                    Log.Info("ReCreating EndPoint Started");
                    await _createEndPoint.Invoke();
                    Log.Warn("ReCreating EndPoint Succeed");
                    Interlocked.Exchange(ref _isRecoveryRequested, NotRequested);
                    return;
                }
                catch (Exception ex)
                {
                    Log.Warn("ReCreating EndPoint Failed: " + ex.Message);
                }
            }
            _onFailure?.Invoke();
        }

        public async Task OnCriticalError(ICriticalErrorContext context)
        {
            if (_ignoreCriticalErrors)
            {
                Log.Warn("Critical Error ignored");
                return;
            }

            try
            {
                // To leave the process active, dispose the bus.
                // When the bus is disposed, the attempt to send message will cause an ObjectDisposedException.
                await context.Stop().ConfigureAwait(false);
            }
            finally
            {
                TryToRecover();
            }
        }
    }
}
