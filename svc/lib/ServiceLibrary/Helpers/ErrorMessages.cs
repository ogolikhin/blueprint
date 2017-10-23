namespace ServiceLibrary.Helpers
{
    public class ErrorMessages
    {
        // Search
        public const string SearchFieldLimitation = "The limit of the search field is 250 characters.";
        // Users
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
        public static readonly string IncorrectLimitParameter = "The \"limit\" parameter is required and should be a positive integer.";
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
        public static readonly string TheUserIdCanNotBeNegative = "The userId can not be negative.";
        public static readonly string MaxUsersPerInstanceLimitReached = "Maximum users limit per instance was reached.";
        // Groups
        public static readonly string GroupModelIsEmpty = "The group model is empty.";
        public static readonly string GroupNameRequired = "Please enter a Group Name.";
        public static readonly string GroupNameFieldLimitation = "The length of the \"Group Name\" field must be between 1 and 255 characters.";
        public static readonly string GroupEmailFieldLimitation = "The length of the \"Group Email\" field must be between 4 and 255 characters.";
        public static readonly string GeneralErrorOfCreatingGroup = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string CreationOnlyDatabaseGroup = "You can create only database group.";
        public static readonly string CreationGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses = "You can create groups only with the \"collaborator\", the \"author\" or empty license.";
        public static readonly string CreationGroupWithScopeAndLicenseIdSimultaneously = "You can not create the \"Access Group\" and \"License Group\" simultaneously.";
        public static readonly string GroupAlreadyExist = "An existing group has the same name. Please try another.";
        public static readonly string SourceFieldValueForGroupsShouldBeOnlyDatabase = "You can modify only database groups.";
        public static readonly string TheScopeCannotBeChanged = "The scope cannot be changed.";
        public static readonly string UpdateGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses = "You can modify groups only with the \"collaborator\", the \"author\" or empty license values.";
        public static readonly string GeneralErrorOfUpdatingGroup = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string GroupNotExist = "The group with the current groupId doesn’t exist or removed from the system.";
        public static readonly string ImpossibleChangeLicenseInGroupWithScope = "It is impossible to change the license type value in the group which has the scope value.";
        public static readonly string InvalidGroupMembersParameters = "Invalid parameters to delete members from the group.";
        public static readonly string GeneralErrorOfRemovingMembersFromGroup = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string AssignMemberScopeEmpty = "Please provide the scope for the assign operation";
        public static readonly string UnassignMemberScopeEmpty = "Please provide the scope for the unassign operation";
        public static readonly string UserAlreadyAssignedToGroup = "User already assigned to the group";
        public static readonly string GroupAlreadyAssignedToGroup = "Group already assigned to the group";
        public static readonly string GroupEmailFormatIncorrect = "Please ensure the email address is in the following format: group@company.com.";
        public static readonly string TheProjectDoesNotExist = "The project does not exist.";
        public static readonly string ProjectIdIsInvalid = "The project Id is invalid";
        // Workflow
        public static readonly string WorkflowNotExist = "The workflow with the current workflowId doesn’t exist or removed from the system.";
        public static readonly string WorkflowIsActive = "The workflow with the current id is active.";
        public static readonly string WorkflowModelIsEmpty = "The model is empty.";
        public static readonly string WorkflowVersionsNotEqual = "The current version the workflow from the request doesn’t match the current version in DB.";
        public static readonly string WorkflowWasNotUpdated = "The workflow with current parameters was not updated.";
        public static readonly string InvalidDeleteWorkflowsParameters = "Invalid parameters to delete workflows";
        public static readonly string GeneralErrorOfDeletingWorkflows = "An error has occurred when performed workflows (s) deletion operation";
        public static readonly string WorkflowNameError = "Please enter a Workflow Name between 4 and 24 characters";
        public static readonly string WorkflowDescriptionLimit = "Please enter a Workflow Description up to 400 characters";
        public static readonly string CreateWorkfloModelIsEmpty = "The body of the call is malformed or has invalid parameter";
        public static readonly string WorkflowAlreadyExists = "Workflow with such a name already exists";
        public static readonly string GeneralErrorOfCreatingWorkflow = "General error of creating workflow";
        public static readonly string GeneralErrorOfAssignProjectsAndArtifactTypesToWorkflow = "General error of assign projects and artifacts to workflows";
        public static readonly string WorkflowProjectHasNoArtifactTypes = "Workflow project has no assigned artifact types.";
        public static readonly string GeneralErrorOfUpdatingWorkflow = "General error of updating workflow.";
        public static readonly string WorkflowWithoutProjectArtifactTypeAssignmentsCannotBeActivated = "Workflow without project/artifact type assignments cannot be activated.";
        public static readonly string WorkflowHasSameProjectArtifactTypeAssignedToAnotherActiveWorkflow = "There is at least one project-artifact type assigned to the current workflow which is also assigned to another active workflow.";
        // Folder
        public static readonly string FolderWithSuchNameExistsInParentFolder = "A folder with the same name already exists in the parent folder. Please use a different name.";
        public static readonly string ModelIsEmpty = "The body of the call is malformed or has invalid parameter.";
        public static readonly string FolderNameLimitation = "Please enter a name between 1 and 128 characters.";
        public static readonly string ErrorOfDeletingFolderThatContainsChildrenItems = "The Folder cannot be deleted as it contains Projects and/or Folders.";
        public static readonly string FolderNotExist = "The folder with current folderId doesn't exist or removed from the system.";
        public static readonly string WorkflowImportErrorsNotFound = "The workflow import errors for GUID={0} are not found.";
        public static readonly string ParentFolderNotExists = "The parent folder with current id does not exist.";
        public static readonly string GeneralErrorOfCreatingFolder = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string GeneralErrorOfUpdatingFolder = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string FolderReferenceToItself = "The folder cannot be placed into itself. Please select a different location.";
        public static readonly string ParentFolderIdReferenceToDescendantItem = "The parent folder cannot be placed into its descendant. Please select a different location.";
        public static readonly string EditRootFolderIsForbidden = "Root folder cannot be edited.";
        // Project
        public static readonly string ProjectNameLimitation = "Please enter a name between 1 and 128 characters.";
        public static readonly string LocationIsRequired = "Please select a location.";
        public static readonly string GeneralErrorOfUpdatingProject = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string ProjectNotExist = "The project with the current id doesn't exist or removed from the system.";
        public static readonly string ProjectWithSuchNameExistsInParentFolder = "A project with the same name already exists in the parent folder. Please use a different name.";
        public static readonly string ProjectWasDeletedByAnotherUser = "Project with ID:{0}({1}) was deleted by another user!";
        public static readonly string ForbidToPurgeSystemInstanceProjectForInternalUseOnly = "Could not purge project because it is a system instance project for internal use only and without it database is corrupted. Purge project aborted for projectId {0}.";
        public static readonly string ArtifactWasMovedToAnotherProject = "Could not purge project because an artifact was moved to another project and we cannot reliably purge it without corrupting the other project.  PurgeProject aborted for projectId  {0}.";
        public static readonly string UnhandledStatusOfProject = "Unhandled case for ProjectStatus: {0}";
        public static readonly string PrivilegesForProjectNotExist = "User privileges for project (Id:{0}) is not found.";
        // Roles
        public static readonly string RolesForProjectNotExist = "Roles for the requested project are missing";
        public static readonly string InvalidDeleteRoleAssignmentsParameters = "Invalid parameters to delete role assignments.";
        public static readonly string GeneralErrorOfDeletingRoleAssignments = "An error has occurred when the operation was being performed at SQL level.";
        public static readonly string GeneralErrorOfUpdatingRoleAssignment = "An error has occurred when the operation of role assignment update was being performed at SQL level.";
        public static readonly string RoleNameIsRequiredField = "Please enter a Role Name.";
        public static readonly string GroupIsRequiredField = "Please select a Group.";
        public static readonly string GroupIsNotFound = "The group with the current id is not found on the instance and project levels.";
        public static readonly string RoleIsNotFound = "The role with the current id is not found in the project's roles.";
        public static readonly string RoleAssignmentAlreadyExists = "Project Role Assignment already exists. You cannot add duplicate assignments.";
        public static readonly string RoleAssignmentNotFound = "Project Role Assignment with the current id is not found.";
        // Artifacts
        public static readonly string ArtifactTypeIdsNotValid = "Please provide valid artifact type ids";
        // Reviews
        public static readonly string ReviewSettingsAreRequired = "Review settings must be provided.";
        public static readonly string ReviewNotFound = "Review (Id:{0}) is not found.";
        public static readonly string ReviewOrRevisionNotFound = "Review (Id:{0}) or its revision (#{1}) is not found.";
        public static readonly string ArtifactIsNotReview = "Artifact (Id:{0}) is not a review.";
        public static readonly string CannotAccessReview = "Review (Id:{0}) cannot be accessed.";
        public static readonly string ReviewIsClosed = "Review (Id:{0}) is now closed. No modifications can be made to its artifacts or participants.";
        public static readonly string ReviewIsNotDraft = "Review (Id:{0}) is not a draft. This action cannot be completed.";
        public static readonly string RequireESignatureDisabled = "Meaning of Signature setting cannot be updated. Electornic signatures are not enabled for Review (Id:{0}).";
        public static readonly string MeaningOfSignatureDisabledInProject = "Meaning of Signature is disabled for the current project.";
    }
}
