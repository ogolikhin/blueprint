using System;

namespace ServiceLibrary.Helpers
{
    /// <summary>
    /// Declares some functions that throw exceptions if specific conditions are met.
    /// </summary>
    public static class ThrowIf
    {
        /// <summary>
        /// Throws an ArgumentNullException if the arg is null.
        /// Example:  ThrowIf.ArgumentNull(user, nameof(user));
        /// </summary>
        /// <param name="arg">The argument to check for null.</param>
        /// <param name="nameOfArg">The name of the arg (use the nameof() function for this).</param>
        /// <param name="message">(optional) An additional message to include in the exception.</param>
        public static void ArgumentNull([ValidatedNotNull] object arg, string nameOfArg, string message = null)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameOfArg, message);
            }
        }

        // The naming is important to inform FxCop.
        sealed class ValidatedNotNullAttribute : Attribute { }
    }
}
