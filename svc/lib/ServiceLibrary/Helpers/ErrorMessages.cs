namespace ServiceLibrary.Helpers
{
    public class ErrorMessages
    {
        //Users
        public const string InvalidPagination = "Pagination object is invalid.";
        public static readonly string LoginRequired = "The \"Login\" field is required.";
        public static readonly string DisplayNameRequired = "The \"Display name\" field is required.";
        public static readonly string FirstNameRequired = "The \"First name\" field is required.";
        public static readonly string LastNameRequired = "The \"Last name\" field is required.";
        public static readonly string LoginNameUnique = "The login name must be unique in the instance.";
        public static readonly string SessionIsEmpty = "The session is empty.";
        public static readonly string UserDoesNotHavePermissions = "The user does not have permissions.";
        public static readonly string LoginFieldLimitation = "The length of the \"Login\" field must be between 4 and 256 characters.";
        public static readonly string DisplayNameFieldLimitation = "The length of the \"Display name\" field must be between 2 and 255 characters.";
        public static readonly string FirstNameFieldLimitation = "The length of the \"First name\" field must be between 2 and 255 characters.";
        public static readonly string LastNameFieldLimitation = "The length of the \"Last name\" field must be between 2 and 255 characters.";
        public static readonly string EmailFieldLimitation = "The length of the \"Email\" field must be between 4 and 255 characters.";
        public static readonly string TitleFieldLimitation = "The length of the \"Title\" field must be between 2 and 255 characters.";
        public static readonly string DepartmentFieldLimitation = "The length of the \"Department\" field must be between 1 and 255 characters.";
        public static readonly string UserModelIsEmpty = "The user model is empty.";
        public static readonly string GeneralErrorOfCreatingUser = "The user was not created.";
        public static readonly string GeneralErrorOfUpdatingUser = "The user was not updated.";
        public static readonly string GeneralErrorOfDeletingUsers = "An error has occurred when performed users (s) deletion operation";
        public static readonly string GeneralErrorOfDeletingGroups = "An error has occurred when performed group(s) deletion operation";
        public static readonly string UserNotExist = "The user with the current userId doesn’t exist or removed from the system.";
        public static readonly string UserVersionsNotEqual = "The current version from the request doesn’t match the current version in DB.";
        public static readonly string IncorrectUserId = "Incorrect userId.";
        public static readonly string InvalidDeleteUsersParameters = "Invalid parameters to delete users";
        public static readonly string InvalidDeleteGroupsParameters = "Invalid parameters to delete group(s)";
        public static readonly string IncorrectLimitParameter = "The \"limit\" parameter should be more than 0.";
        public static readonly string IncorrectOffsetParameter = "The \"offset\" parameter should not be negative.";
        public static readonly string TotalNull = "The \"total\" is null.";
        public static readonly string GeneralErrorOfGettingUserGroups = "The general error of getting user's groups.";
        public static readonly string CreationOnlyDatabaseUsers = "You can create only database users.";
        public static readonly string EmailFormatIncorrect = "The email format is incorrect.";
        public static readonly string LoginInvalid = "The \"Login\" field is invalid.";
        public static readonly string PasswordSameAsLogin = "Password cannot be equal to login name.";
        public static readonly string PasswordSameAsDisplayName = "Password cannot be equal to display name.";
        public static readonly string InvalidChangeInstanceAdminPasswordParameters = "Parameters are invalid";
        public static readonly string InvalidInstanceAdminUserPassword = "Password validation failed";
        public static readonly string SourceFieldValueShouldBeOnlyDatabase = "You can modify only database users.";      
        public static readonly string InvalidDeleteUserFromGroupsParameters = "Invalid parameters to delete user from groups.";
        public static readonly string GeneralErrorOfDeletingUserFromGroups = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string CantGetUsersToBeDeleted = "Can't get users to be deleted";
    }
}
