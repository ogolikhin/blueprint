using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
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

        /// <summary>
        /// Throws an ArgumentException if the collection is empty.
        /// </summary>
        /// <typeparam name="T">The class type stored in the collection.</typeparam>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <param name="nameOfArg">The name of the collection variable.</param>
        /// <param name="message">(optional) An additional message to include in the exception.</param>
        public static void IsEmpty<T>(ICollection<T> collection, string nameOfArg, string message = null)
        {
            ThrowIf.ArgumentNull(collection, nameOfArg);

            if (!collection.Any())
            {
                throw new ArgumentException(nameOfArg, message);
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the string is null or whitespace.
        /// Example:  ThrowIf.IsNullOrWhiteSpace(username, nameof(username));
        /// </summary>
        /// <param name="arg">The string to check for null or whitespace.</param>
        /// <param name="nameOfArg">The name of the string variable (use the nameof() function for this).</param>
        /// <param name="message">(optional) An additional message to include in the exception.</param>
        public static void IsNullOrWhiteSpace([ValidatedNotNull] string arg, string nameOfArg, string message = null)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentNullException(nameOfArg, message);
            }
        }

        // The naming is important to inform FxCop.
        sealed class ValidatedNotNullAttribute : Attribute { }
    }
}
