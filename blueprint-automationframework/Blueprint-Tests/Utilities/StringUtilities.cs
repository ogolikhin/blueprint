using System;
using System.Globalization;

namespace Utilities
{
    public static class StringUtilities
    {
        /// <summary>
        /// Capitalizes the First Character in a String
        /// </summary>
        /// <param name="valueToModify">The string to modify</param>
        /// <returns>The modified string</returns>
        public static string CapitalizeFirstCharacter(this string valueToModify)
        {
            ThrowIf.ArgumentNull(valueToModify, nameof(valueToModify));

            valueToModify = char.ToUpper(valueToModify[0], CultureInfo.InvariantCulture) + valueToModify.Substring(1);

            return valueToModify;
        }

        /// <summary>
        /// Lowers  the Case of the First Character in a String
        /// </summary>
        /// <param name="valueToModify">The string to modify</param>
        /// <returns>The modified string</returns>
        public static string LowerCaseFirstCharacter(this string valueToModify)
        {
            ThrowIf.ArgumentNull(valueToModify, nameof(valueToModify));

            valueToModify = char.ToLower(valueToModify[0], CultureInfo.InvariantCulture) + valueToModify.Substring(1);

            return valueToModify;
        }

        /// <summary>
        /// Checks if the source string contains the sub-string without case sensitivity.
        /// </summary>
        /// <param name="source">The source string to look in.</param>
        /// <param name="subString">The sub-string to find in the source string.</param>
        /// <returns>True if the sub-string was found in the source, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]  // Ignore this.
        public static bool ContainsIgnoreCase(this string source, string subString)
        {
            ThrowIf.ArgumentNull(source, nameof(source));
            ThrowIf.ArgumentNull(subString, nameof(subString));

            return source.IndexOf(subString, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}