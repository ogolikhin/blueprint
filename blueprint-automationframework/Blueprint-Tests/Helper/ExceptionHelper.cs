using System;
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
        public static Exception Catch(TestDelegate code)
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
    }
}
