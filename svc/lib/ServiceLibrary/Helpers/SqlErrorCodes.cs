namespace ServiceLibrary.Helpers
{
    public enum SqlErrorCodes
    {
        None = 0,
		GeneralSqlError = 50000,
        UserLoginExist = 50001,
        UserLoginNotExist = 50002,
        UserVersionsNotEqual = 50003,
        GroupWithNameAndLicenseIdExist = 50004,
        GroupWithNameAndScopeExist = 50005
    }
}
