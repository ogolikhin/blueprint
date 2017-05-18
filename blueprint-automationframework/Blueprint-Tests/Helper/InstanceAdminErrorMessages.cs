namespace Helper
{
    /// <summary>
    /// ErrorMessages class for Instance User validation errors
    /// </summary>
    public static class InstanceAdminErrorMessages
    {
        //Users
        public const string InvalidPageOrPageNumber = "Page, PageSize are missing or invalid.";
        public const string LoginRequired = "The \"Login\" field is required.";
        public const string DisplayNameRequired = "The \"Display name\" field is required.";
        public const string FirstNameRequired = "The \"First name\" field is required.";
        public const string LastNameRequired = "The \"Last name\" field is required.";
        public const string LoginNameUnique = "The login name must be unique in the instance.";
        public const string SessionIsEmpty = "The session is empty.";
        public const string UserDoesNotHavePermissions = "The user does not have permissions.";
        public const string LoginFieldLimitation = "The length of the \"Login\" field must be between 4 and 256 characters.";
        public const string DisplayNameFieldLimitation = "The length of the \"Display name\" field must be between 2 and 255 characters.";
        public const string FirstNameFieldLimitation = "The length of the \"First name\" field must be between 2 and 255 characters.";
        public const string LastNameFieldLimitation = "The length of the \"Last name\" field must be between 2 and 255 characters.";
        public const string EmailFieldLimitation = "The length of the \"Email\" field must be between 4 and 255 characters.";
        public const string TitleFieldLimitation = "The length of the \"Title\" field must be between 2 and 255 characters.";
        public const string DepartmentFieldLimitation = "The length of the \"Department\" field must be between 1 and 255 characters.";
        public const string UserModelIsEmpty = "The user model is empty.";
        public const string GeneralErrorOfCreatingUser = "The user was not created.";
        public const string GeneralErrorOfUpdatingUser = "The user was not updated.";
        public const string UserNotExist = "The user with the current userId doesn’t exist or removed from the system.";
        public const string UserVersionsNotEqual = "The current version from the request doesn’t match the current version in DB.";
        public const string IncorrectUserId = "Incorrect userId.";
        public const string CreateOnlyDatabaseUsers = "You can create only database users.";
        public const string EmailFormatIncorrect = "The email format is incorrect.";
        public const string LoginInvalid = "The \"Login\" field is invalid.";

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
