using Model.Impl;
using Utilities.Factories;
using Common;

namespace Model.Factories
{
    public static class GroupFactory
    {
        /// <summary>
        /// Creates new Group in memory.  AddGroupToDatabase() can be called for created Group.
        /// </summary>
        /// <param name="licenseType">The group license type.</param>
        /// <returns>The created Group.</returns>
        public static IGroup CreateGroup(GroupLicenseType licenseType)
        {
            string name = RandomGenerator.RandomAlphaNumeric(6);
            string description = RandomGenerator.RandomAlphaNumeric(10);
            string email = I18NHelper.FormatInvariant("{0}@{1}.com", name, RandomGenerator.RandomAlphaNumeric(10));
            IGroup group = new Group(name, email, description, licenseType);
            return group;
        }

        /// <summary>
        /// Creates new Group in memory.  AddGroupToDatabase() can be called for created Group.
        /// </summary>
        /// <param name="name">(Optional) The name of the group.  By default a random value is used.</param>
        /// <param name="email">(Optional) The E-mail address of the group.  By default a random value is used.</param>
        /// <param name="description">(Optional) The description of the group.  By default a random value is used.</param>
        /// <param name="licenseType">(Optional) Group license.  By default use Author license.</param>
        /// <returns>The created Group.</returns>
        public static IGroup CreateGroup(string name = null,
            string email = null,
            string description = null,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            if (name == null)
            {
                name = RandomGenerator.RandomAlphaNumeric(6);
            }

            if (email == null)
            {
                email = I18NHelper.FormatInvariant("{0}@{1}.com", name, RandomGenerator.RandomAlphaNumeric(10));
            }

            if (description == null)
            {
                description = RandomGenerator.RandomAlphaNumeric(10);
            }

            IGroup group = new Group(name, email, description, licenseType);
            return group;
        }
    }
}
