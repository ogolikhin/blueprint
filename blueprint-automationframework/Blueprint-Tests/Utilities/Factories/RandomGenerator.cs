using System;
using System.Linq;
using Common;

namespace Utilities.Factories
{
    public static class RandomGenerator
    {
        public const string UpperCaseAndNumbers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string UpperAndLowerCaseAndNumbers = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static Random _random = new Random();


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
        /// Generates a random number.
        /// </summary>
        /// <returns>A random number.</returns>
        public static int RandomNumber()
        {
            return _random.Next(1, int.MaxValue);
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
    }
}
