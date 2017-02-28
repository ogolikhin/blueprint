namespace Model.Common.Enums
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]    // Production doesn't have a 0 value.  Use null instead.
    public enum InstanceAdminRole
    {
        AdministerALLProjects = 7,
        AssignInstanceAdministrators = 8,
        BlueprintAnalytics = 9,
        DefaultInstanceAdministrator = 1,
        Email_ActiveDirectory_SAMLSettings = 3,
        InstanceStandardsManager = 10,
        LogGatheringAndLicenseReporting = 2,
        ManageAdministratorRoles = 4,
        ProvisionProjects = 6,
        ProvisionUsers = 5
    }
}


