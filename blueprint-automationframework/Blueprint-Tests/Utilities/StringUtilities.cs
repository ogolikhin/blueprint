using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
        public static bool ContainsIgnoreCase(this string source, string subString)
        {
            ThrowIf.ArgumentNull(source, nameof(source));
            ThrowIf.ArgumentNull(subString, nameof(subString));

            return source.IndexOf(subString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Wraps text into div tag. Format returned by discussions related REST.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <returns>Wrapped text.</returns>
        public static string WrapInDiv(string text)
        {
            return ("<div>" + text + "</div>");
        }

        ///<summary>
        /// Wraps text in html format
        ///</summary>
        ///<param name="text"> Text to warp.</param>
        ///<returns>HTML wrapped text</returns>
        public static string WrapInHTML(string text)
        {
            return ("<html><head></head>" + text + "</html>");
        }

        /// <summary>
        /// Convert HTML text into plain text.
        /// </summary>
        /// <param name="htmlText">HTML text to convert to plain text.</param>
        /// <returns>The plain text that was extracted from the HTML text.</returns>
        public static string ConvertHtmlToText(string htmlText)
        {
            if (htmlText == null)
            {
                return string.Empty;
            }

            string plainText = WebUtility.HtmlDecode( Regex.Replace(htmlText, "<(.|\n)*?>", "") );
            //TODO: Better way of removing zero-width space from htmlText?
            char ZeroWidthSpace = (char)8203;
            string resultPlainText = plainText.Trim(ZeroWidthSpace);
            return resultPlainText;
        }

        /// <summary>
        /// Decodes the Base64 encoded string into plain text.
        /// </summary>
        /// <param name="base64EncodedText">The Base64 string to convert.</param>
        /// <returns>The plain text version of the string.</returns>
        public static string DecodeFromBase64(this string base64EncodedText)
        {
            var bytes = Convert.FromBase64String(base64EncodedText);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Encodes a string to Base64.
        /// </summary>
        /// <param name="plainText">The string to encode.</param>
        /// <returns>The Base64 version of the string.</returns>
        public static string EncodeToBase64(this string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }
    }
}