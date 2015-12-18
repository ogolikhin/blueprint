﻿// Copyright (c) Microsoft Corporation. All rights reserved. 
// Modified to exclude uneeded methods

using System;
using System.Globalization;

namespace Logging.Database.Utility
{
    internal static class Guard
    {
        /// <summary>
        /// Throws <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <exception cref="ArgumentNullException"> If tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="argumentName">Name of the argument being tested.</param>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the string is empty</exception>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException(Properties.Resources.ArgumentIsEmptyError, argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the tested TimeSpam argument is not a valid timeout value.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the argument is not null and is not a valid timeout value.</exception>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        public static void ArgumentIsValidTimeout(TimeSpan? argumentValue, string argumentName)
        {
            if (argumentValue.HasValue)
            {
                long totalMilliseconds = (long)argumentValue.Value.TotalMilliseconds;
                if (totalMilliseconds < (long)-1 || totalMilliseconds > (long)2147483647)
                {
                    throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.TimeSpanOutOfRangeError, argumentName));
                }
            }
        }
    }
}
