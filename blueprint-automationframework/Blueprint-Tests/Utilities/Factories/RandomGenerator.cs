using System;
using System.Linq;
using Common;

namespace Utilities.Factories
{
    public static class RandomGenerator
    {
        public const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        public const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string UpperCaseAndNumbers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string UpperAndLowerCaseAndNumbers = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string SpecialChars = "~!@#$%^&*()_+`-=[]\\{}|;':\",./<>?";    // NOTE: These are only the ones from the keyboard, but there are many more possible chars.

        private static readonly Random _random = new Random();


        /// <summary>
        /// Generates a random string of the specified length using the specified characters.
        /// </summary>
        /// <param name="length">The length of the random string to create.</param>
        /// <param name="chars">The characters to use in the random string.</param>
        /// <returns>A random string.</returns>
        public static string RandomString(uint length, string chars)
        {
            return new string(Enumerable.Repeat(chars, (int)length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a random alpha-numeric string of the specified length.
        /// </summary>
        /// <param name="length">The length of the random string to create.</param>
        /// <returns>A random alpha-numeric string.</returns>
        public static string RandomAlphaNumeric(uint length)
        {
            return RandomString(length, UpperCaseAndNumbers);
        }

        /// <summary>
        /// Generates a random number from 0 to the max value specified.
        /// </summary>
        /// <param name="maxValue">(optional) The maximum random number to create.</param>
        /// <returns>A random number.</returns>
        public static int RandomNumber(int maxValue = int.MaxValue)
        {
            return _random.Next(0, maxValue);
        }

        /// <summary>
        /// Generates a random string of only lower case letters of the specified length.
        /// </summary>
        /// <param name="length">The length of the random string to create.</param>
        /// <returns>A random lower case string.</returns>
        public static string RandomLowerCase(uint length)
        {
            return RandomString(length, LowerCase);
        }

        /// <summary>
        /// Generates a random string of only special characters of the specified length.
        /// </summary>
        /// <param name="length">The length of the random string to create.</param>
        /// <returns>A random special characters string.</returns>
        public static string RandomSpecialChars(uint length)
        {
            return RandomString(length, SpecialChars);
        }

        /// <summary>
        /// Generates a random string of only upper case letters of the specified length.
        /// </summary>
        /// <param name="length">The length of the random string to create.</param>
        /// <returns>A random upper case string.</returns>
        public static string RandomUpperCase(uint length)
        {
            return RandomString(length, UpperCase);
        }

        /// <summary>
        /// Generates a random alpha-numeric string (with both upper and lower case letters) of the specified length.
        /// </summary>
        /// <param name="length">The length of the random string to create.</param>
        /// <returns>A random alpha-numeric string.</returns>
        public static string RandomAlphaNumericUpperAndLowerCase(uint length)
        {
            return RandomString(length, UpperAndLowerCaseAndNumbers);
        }

        /// <summary>
        /// Create Random Value with a supplied Prefix
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <param name="numberOfCharacters">The number of alphanumeric characters to append to the prefix</param>
        /// <returns>A random alpha numeric character value with a supplied prefix</returns>
        public static string RandomValueWithPrefix(string prefix, uint numberOfCharacters)
        {
            return I18NHelper.FormatInvariant("{0}_{1}", prefix, RandomAlphaNumericUpperAndLowerCase(numberOfCharacters));
        }

        /// <summary>
        /// Generates a random alpha-numeric string (with both upper and lower case letters, numbers and spaces).
        /// </summary>
        /// <returns>A random string with spaces.</returns>
        public static string RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces()
        {
            string randomString = string.Empty;

            randomString += RandomAlphaNumericUpperAndLowerCase(5);
            randomString += " ";
            randomString += RandomAlphaNumericUpperAndLowerCase(5);
            randomString += " ";
            randomString += RandomSpecialChars(4);
            randomString += " ";
            randomString += RandomNumber();

            return randomString;
        }
    }
}
