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
    }
}