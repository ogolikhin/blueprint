using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public static  class ErrorMessages
    {
        public static readonly string LoginRequered = "The login field is required.";
        public static readonly string DisplayNameRequered = "The display name field is required.";
        public static readonly string FirstNameRequered = "The first name field is required.";
        public static readonly string LastNameRequered = "The last name field is required.";
        public static readonly string PasswordRequered = "The password field is required.";
        public static readonly string LoginNameUnique = "Login name must be unique in the instance.";
        public static readonly string SessionIsEmpty = "The session is empty.";
        public static readonly string UserDoesNotHavePermissions = "The user does not have permissions.";
        public static readonly string LoginFieldLimitation = "The length of the Login field must be between 4 and 256 characters.";
        public static readonly string DisplayNameFieldLimitation = "The length of the DisplayName field must be between 2 and 255 characters.";
        public static readonly string FirstNameFieldLimitation = "The length of the FirstName field must be between 2 and 255 characters.";
        public static readonly string LastNameFieldLimitation = "The length of the LastName field must be between 2 and 255 characters.";
        public static readonly string EmailFieldLimitation = "The length of the Email field must be between 4 and 255 characters.";
        public static readonly string TitleFieldLimitation = "The length of the Title field must be between 2 and 255 characters.";
        public static readonly string DepartmentFieldLimitation = "The length of the Department field must be between 1 and 255 characters.";
    }
}
