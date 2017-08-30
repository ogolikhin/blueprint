using System.Text.RegularExpressions;

namespace ServiceLibrary.Helpers.Validators
{
    public static class EmailValidator
    {
        public static bool IsEmailAddress(string emailAddress)
        {
            return Regex.IsMatch(emailAddress, @"^([\w-\.\']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}
