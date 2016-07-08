using Model.Impl;

namespace Model.Factories
{
    public static class GroupFactory
    {
        /// <summary>
        /// Creates new Group in memory. AddGroupToDatabase() can be called for created Group.
        /// </summary>
        /// <param name="name">Group name</param>
        /// <param name="description">Group description - not available in UI</param>
        /// <param name="email">Group email</param>
        /// <param name="licenseType">(Optional)Group license. By default use Author license.</param>
        /// <returns>Created Group.</returns>
        public static IGroup CreateGroup(string name, string description, string email,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            IGroup group = new Group(name, description, email, licenseType);
            return group;
        }
    }
}
