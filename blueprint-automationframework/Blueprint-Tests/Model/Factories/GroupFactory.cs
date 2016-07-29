using Model.Impl;
using Utilities.Factories;
using Common;

namespace Model.Factories
{
    public static class GroupFactory
    {
        /// <summary>
        /// Creates new Group in memory. AddGroupToDatabase() can be called for created Group.
        /// </summary>
        /// <param name="licenseType">(Optional)Group license. By default use Author license.</param>
        /// <returns>Created Group.</returns>
        public static IGroup CreateGroup(GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            string name = RandomGenerator.RandomAlphaNumeric(6);
            string description = RandomGenerator.RandomAlphaNumeric(10);
            string email = I18NHelper.FormatInvariant("{0}@{1}.com", name, RandomGenerator.RandomAlphaNumeric(10));
            IGroup group = new Group(name, email, description, licenseType);
            return group;
        }
    }
}
