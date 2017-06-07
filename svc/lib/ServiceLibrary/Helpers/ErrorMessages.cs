namespace ServiceLibrary.Helpers
{
    public class ErrorMessages
    {
        //Users
        public const string InvalidPagination = "The \"offset\" and/or \"limit\" parameters are required.";
        public static readonly string LoginRequired = "The user name field is required.";
        public static readonly string DisplayNameRequired = "The display name field is required.";
        public static readonly string FirstNameRequired = "The \"First name\" field is required.";
        public static readonly string LastNameRequired = "The \"Last name\" field is required.";
        public static readonly string LoginNameUnique = "Please enter a unique username.";
        public static readonly string SessionIsEmpty = "The session is empty.";
        public static readonly string UserDoesNotHavePermissions = "The user does not have permissions.";
        public static readonly string LoginFieldLimitation = "Please enter a user name between 4 and 255 characters.";
        public static readonly string DisplayNameFieldLimitation = "Please enter a display name between 1 and 255 characters.";
        public static readonly string FirstNameFieldLimitation = "Please enter a first name between 1 and 255 characters.";
        public static readonly string LastNameFieldLimitation = "Please enter a last name between 1 and 255 characters.";
        public static readonly string EmailFieldLimitation = "Please enter an email between 4 and 255 characters.";
        public static readonly string TitleFieldLimitation = "Please enter a title between 1 and 255 characters.";
        public static readonly string DepartmentFieldLimitation = "Please enter a department between 1 and 255 characters.";
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
        public static readonly string IncorrectLimitParameter = "The \"limit\" parameter is required and should not be negative.";
        public static readonly string IncorrectOffsetParameter = "The \"offset\" parameter is required and should not be negative.";
        public static readonly string TotalNull = "The \"total\" is null.";
        public static readonly string GeneralErrorOfGettingUserGroups = "The general error of getting user's groups.";
        public static readonly string CreationOnlyDatabaseUsers = "You can create only database users.";
        public static readonly string EmailFormatIncorrect = "Please ensure the email address is in the following format: user@company.com.";
        public static readonly string LoginInvalid = "The \"Login\" field is invalid.";
        public static readonly string PasswordSameAsLogin = "Password cannot be equal to login name.";
        public static readonly string PasswordSameAsDisplayName = "Password cannot be equal to display name.";
        public static readonly string InvalidChangeInstanceAdminPasswordParameters = "Parameters are invalid";
        public static readonly string InvalidInstanceAdminUserPassword = "Password validation failed";
        public static readonly string SourceFieldValueShouldBeOnlyDatabase = "You can modify only database users.";
        public static readonly string InvalidDeleteUserFromGroupsParameters = "Invalid parameters to delete user from groups.";
        public static readonly string GeneralErrorOfDeletingUserFromGroups = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string InvalidAddUserToGroupsParameters = "Invalid parameters to add user to groups.";
        public static readonly string GeneralErrorOfAddingUserToGroups = "An error has occurred when the operation was being performed at the SQL level.";
        public static readonly string CantGetUsersToBeDeleted = "Can't get users to be deleted";
        public static readonly string IncorrectBase64FormatPasswordField = "The password is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters";
        //Groups
        public static readonly string GroupModelIsEmpty = "The group model is empty.";
        public static readonly string GroupName = "Please ensure the Group Name is not empty.";
        public static readonly string GroupNameFieldLimitation = "The length of the \"Group Name\" field must be between 4 and 255 characters.";
        public static readonly string GroupEmailFieldLimitation = "The length of the \"Group Email\" field must be between 4 and 255 characters.";
        public static readonly string GroupEmailFormatIncorrect = "Please ensure the email address is correct.";
        public static readonly string GeneralErrorOfCreatingGroup = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string CreationOnlyDatabaseGroup = "You can create only database group.";
        public static readonly string CreationGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses = "You can create groups only with the \"collaborator\", the \"author\" or empty license.";
        public static readonly string CreationGroupWithScopeAndLicenseIdSimultaneously = "You can not create the \"Access Group\" and \"License Group\" simultaneously.";
        public static readonly string GroupAlreadyExist = "The same group already exists. You cannot add duplicate groups.";
        public static readonly string GroupDoesNotExist = "The group with this Id does not exist or removed from the system.";
        public static readonly string SourceFieldValueForGroupsShouldBeOnlyDatabase = "You can modify only database groups.";
        public static readonly string TheScopeCannotBeChanged = "The scope cannot be changed.";
        public static readonly string UpdateGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses = "You can modify groups only with the \"collaborator\", the \"author\" or empty license values.";
        public static readonly string GeneralErrorOfUpdatingGroup = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string GroupNotExist = "The group with the current groupId doesn’t exist or removed from the system.";
        public static readonly string ImpossibleChangeLicenseInGroupWithScope = "It is impossible to change the license type value in the group which has the scope value.";
        public static readonly string InvalidGroupMembersParameters = "Invalid parameters to delete members from the group.";
        public static readonly string GeneralErrorOfRemovingMembersFromGroup = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string AssignMemberScopeEmpty = "Please provide the scope for the assign operation";
        public static readonly string UserAlreadyAssignedToGroup = "User already assigned to the group";
        public static readonly string GroupAlreadyAssignedToGroup = "Group already assigned to the group";
    }
}
