using System.Text.RegularExpressions;

namespace ServiceLibrary.Helpers
{
    public static class UserManagementHelper
    {
        public static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^([\w-\.\']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            return emailRegex.IsMatch(email);
        }
    }
}