using Model.Impl;

namespace Model.Factories
{
    public static class GroupFactory
    {
        public static IGroup CreateGroup(string name, string description, string email,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            IGroup group = new Group(name, description, email, licenseType);
            return group;
        }
    }
}
