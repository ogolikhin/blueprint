using System;
using Common;
using Helper;

namespace TestCommon
{
    /// <summary>
    /// This is a base class for all tests that use the TestHelper.  It disposes the TestHelper,
    /// but tests should always explicitly dispose it themselves in their TearDown method.
    /// </summary>
    public class TestBase : IDisposable
    {
        /// <summary>
        /// A helper that simplifies the creation and management of users, artifacts...
        /// </summary>
        protected TestHelper Helper { get; set; }

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly called, or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(TestBase), nameof(TestBase.Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Helper?.Dispose();
            }

            _isDisposed = true;

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestBase), nameof(TestBase.Dispose));
        }

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
