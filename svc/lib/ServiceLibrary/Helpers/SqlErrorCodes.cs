﻿namespace ServiceLibrary.Helpers
{
    public enum SqlErrorCodes
    {
        None = 0,
		GeneralSqlError = 50000,
        UserLoginExist = 50001,
        UserLoginNotExist = 50002,
        UserVersionsNotEqual = 50003,
        GroupWithNameAndLicenseIdExist = 50004,
        GroupWithNameAndScopeExist = 50005,
        GroupWithCurrentIdNotExist = 50006,
        GroupVersionsNotEqual = 50007,
        GroupCanNotBeUpdatedWithExistingScope = 50008,
        UserAlreadyAssignedToTheGroup = 50009,
        GroupAlreadyAssignedToTheGroup = 50010,
        CurrentProjectIsNotExist = 50011,
        FolderWithSuchNameExistsInParentFolder = 50012,
        InstanceFolderContainsChildrenItems = 50013,
        ParentFolderNotExists = 50014,
        FolderWithCurrentIdNotExist = 50015,
        ProjectWithCurrentIdNotExist = 50016,
        ProjectWithSuchNameExistsInParentFolder = 50017,
        ParentFolderIdReferenceToDescendantItem = 50018,
        EditRootFolderIsForbidden = 50019,
        RolesForProjectNotExist = 50020

    }
}
