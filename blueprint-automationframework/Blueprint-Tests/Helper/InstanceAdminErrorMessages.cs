namespace Helper
{
    /// <summary>
    /// ErrorMessages class for Instance User validation errors
    /// </summary>
    public static class InstanceAdminErrorMessages
    {
        //Users
        public const string InvalidPagination = "The \"offset\" and/or \"limit\" parameters are required.";
        public const string LoginRequired = "The user name field is required.";
        public const string DisplayNameRequired = "The display name field is required.";
        public const string FirstNameRequired = "The \"First name\" field is required.";
        public const string LastNameRequired = "The \"Last name\" field is required.";
        public const string LoginNameUnique = "Please enter a unique username.";
        public const string SessionIsEmpty = "The session is empty.";

        public const string UserDoesNotHavePermissions = "The user does not have permissions.";

        public const string LoginFieldLimitation = "Please use a user name between 4 and 255 alphanumeric characters.";
        public const string DisplayNameFieldLimitation = "Please enter a display name between 1 and 255 characters.";
        public const string FirstNameFieldLimitation = "Please enter a first name between 1 and 255 characters.";
        public const string LastNameFieldLimitation = "Please enter a last name between 1 and 255 characters.";
        public const string EmailFieldLimitation = "Please enter an email between 4 and 255 characters.";
        public const string TitleFieldLimitation = "Please enter a title between 1 and 255 characters.";
        public const string DepartmentFieldLimitation = "Please enter a department between 1 and 255 characters.";

        public const string UserModelIsEmpty = "The user model is empty.";
        public const string GeneralErrorOfCreatingUser = "The user was not created.";
        public const string GeneralErrorOfUpdatingUser = "The user was not updated.";
        public const string UserNotExist = "The user with the current userId doesn’t exist or removed from the system.";
        public const string UserVersionsNotEqual = "The current version from the request doesn’t match the current version in DB.";
        public const string IncorrectUserId = "Incorrect userId.";
        public const string CreateOnlyDatabaseUsers = "You can create only database users.";
        public const string ModifyOnlyDatabaseUsers = "You can modify only database users.";
        public const string EmailFormatIncorrect = "Please ensure the email address is in the following format: user@company.com.";
        public const string LoginInvalid = "The \"Login\" field is invalid.";
        public const string IncorrectLimitParameter = "The \"limit\" parameter is required and should not be negative.";
        public const string IncorrectOffsetParameter = "The \"offset\" parameter is required and should not be negative.";

        //Passwords
        public const string PasswordMissing = "Password is required";
        public const string PasswordInvalidLength = "Password must be between 8 and 128 characters";
        public const string PasswordDoesNotHaveNonAlphanumeric = "Password must contain a non-alphanumeric character";
        public const string PasswordDoesNotHaveNumber = "Password must contain a number";
        public const string PasswordDoesNotHaveUpperCase = "Password must contain an upper-case letter";
        public const string PasswordSameAsLogin = "Password cannot be equal to login name.";
        public const string PasswordSameAsDisplayName = "Password cannot be equal to display name.";
    }
}
