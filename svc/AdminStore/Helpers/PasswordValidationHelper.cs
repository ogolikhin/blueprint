using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace AdminStore.Helpers
{
    public class PasswordValidationHelper
    {
        private static readonly int PasswordMaximumLength = 128; // OWASP Recommended
        private static readonly int PasswordMinimumLength = 8;

        public static bool ValidatePassword(string password, bool isPasswordRequired, out string errorMessage)
        {
            errorMessage = "";
            bool valid = true;

            // Check if blank. Return
            if (string.IsNullOrEmpty(password))
            {
                if (isPasswordRequired)
                {
                    errorMessage = "Password is required";
                    return false;
                }
                else
                {
                    return true;
                }
            }

            // Further checks. Do not return until we perform all checks.
            // Give the user a message explaining all policies that were not satisfied.
            // Check length.
            int length = password.Length;
            if (length > PasswordMaximumLength || length < PasswordMinimumLength)
            {
                if (isPasswordRequired)
                {
                    errorMessage += String.Format(CultureInfo.CurrentCulture, "Password must be between {0} and {1} characters" + Environment.NewLine, PasswordMinimumLength, PasswordMaximumLength);
                }
                else
                {
                    errorMessage += String.Format(CultureInfo.CurrentCulture, "Password must be between {0} and {1} characters, or left blank" + Environment.NewLine, PasswordMinimumLength, PasswordMaximumLength);
                }

                valid = false;
            }

            // Check for presence of non-alphanumeric character.
            if (!Regex.IsMatch(password, @"[^A-Za-z0-9]"))
            {
                errorMessage += isPasswordRequired
                    ? "Password must contain a non-alphanumeric character"
                    : "Password must contain a non-alphanumeric character, or be left blank";
                errorMessage += Environment.NewLine;

                valid = false;
            }

            // Check for presence of numeric character.
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage += isPasswordRequired
                    ? "Password must contain a number"
                    : "Password must contain a number, or be left blank";
                errorMessage += Environment.NewLine;

                valid = false;
            }

            // Check for presence of a capital letter.
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage += isPasswordRequired
                    ? "Password must contain an upper-case letter"
                    : "Password must contain an upper-case letter, or be left blank";
                errorMessage += Environment.NewLine;

                valid = false;
            }

            errorMessage = errorMessage.TrimEnd(); // Remove final newline.

            return valid;
        }
    }
}