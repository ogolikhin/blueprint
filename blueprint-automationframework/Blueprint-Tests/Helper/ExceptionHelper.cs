using System;
using System.Threading;
using Common;
using NUnit.Framework;
using Utilities;

namespace Helper
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Catches any exceptions thrown by the delegate and returns the exception object.
        /// </summary>
        /// <param name="code">The delegate code whose exceptions you want to catch.</param>
        /// <returns>The exception that was caught, or null if no exception was thrown.</returns>
        /// <exception cref="ArgumentNullException">If a null delegate was passed.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // We want to catch ALL exceptions!
        public static Exception Catch(Action code)
        {
            ThrowIf.ArgumentNull(code, nameof(code));

            try
            {
                code();
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }

        /// <summary>
        /// Re-runs the specified delegate code until a certain type of exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of exception we're expecting to be thrown by the delegate.</typeparam>
        /// <param name="code">The code we want to run and retry until the exception is thrown.</param>
        /// <param name="maxAttempts">The max number of times to re-run the code.</param>
        /// <param name="sleepMs">The number of milliseconds to sleep in between retries.</param>
        /// <param name="message">The assert message to use if the expected exception isn't thrown after all the retries.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]    // Ignore this warning.
        public static void RetryIfExceptionNotThrown<T>(Action code, int maxAttempts, int sleepMs, string message) where T : Exception
        {
            ThrowIf.ArgumentNull(code, nameof(code));

            Exception ex = null;

            for (int attempt = 0; attempt < maxAttempts; ++attempt)
            {
                ex = Catch(code);

                if (ex == null)
                {
                    // No exception was thrown, so sleep and try again.
                    if (attempt < maxAttempts)
                    {
                        Logger.WriteWarning("No exception was thrown.  Sleeping for {0}ms before trying again...", sleepMs);
                        Thread.Sleep(sleepMs);
                    }
                }
                else if (ex is T)
                {
                    // This is the exception we expect to get.
                    break;
                }
                else
                {
                    Assert.Fail("We were expecting an exception of type: {0}, but instead we got: {1} {2}\n{3}",
                        typeof(T), ex.GetType(), ex.Message, ex.StackTrace);
                }
            }

            Assert.NotNull(ex, message);
        }

        /// <summary>
        /// Re-runs the specified delegate code until a certain type of exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of exception we're expecting to be thrown by the delegate.</typeparam>
        /// <param name="code">The code we want to run and retry until the exception is thrown.</param>
        /// <param name="maxAttempts">The max number of times to re-run the code.</param>
        /// <param name="sleepMs">The number of milliseconds to sleep in between retries.</param>
        /// <param name="format">The format string for the assert message to use if the expected exception isn't thrown after all the retries.</param>
        /// <param name="args">The arguments for the format string.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]    // Ignore this warning.
        public static void RetryIfExceptionNotThrown<T>(Action code, int maxAttempts, int sleepMs, string format, params Object[] args) where T : Exception
        {
            string message = I18NHelper.FormatInvariant(format, args);
            RetryIfExceptionNotThrown<T>(code, maxAttempts, sleepMs, message);
        }
    }
}
