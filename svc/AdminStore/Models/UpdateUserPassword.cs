using System.Text.RegularExpressions;

namespace AdminStore.Models
{
    public class UpdateUserPassword
    {
        public string Password { get; set; }
        public int UserId { get; set; }

        public bool IsPasswordIsValid()
        {
            //password must contain at least one number, uppercase letter, and non-alphanumeric character
            return Password != null && Regex.Match(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$").Success;
        }
    }
}