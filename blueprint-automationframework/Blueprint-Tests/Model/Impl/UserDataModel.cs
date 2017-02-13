using Common;
using System.Collections.Generic;
using Utilities.Factories;

namespace Model.Impl
{
    public class UserDataModel
    {
        #region Properties

        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
        public List<IGroup> GroupMembership { get; set; }
        public InstanceAdminRole? InstanceAdminRole { get; set; }
        public bool? ExpirePassword { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Default constructor. Generates all necessary values.
        /// </summary>
        public User()
        {
            Source = UserSource.Database;
            Username = RandomGenerator.RandomAlphaNumeric(10);
            FirstName = RandomGenerator.RandomAlphaNumeric(10);
            LastName = RandomGenerator.RandomAlphaNumeric(10);
            Password = RandomGenerator.RandomAlphaNumeric(10);
            DisplayName = I18NHelper.FormatInvariant("{0} {1}", FirstName, LastName);
        }

        /// <summary>
        /// Constractor with all necessary to creation that user need to provide
        /// </summary>
        /// <param name="source">Database or Windows user</param>
        /// <param name="username">Username</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="password">Password</param>
        /// <param name="displayName">User display name</param>
        public User(UserSource source, string userName, string firstName, string lastName, string password, string displayName)
        {
            Source = source;
            Username = userName;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            DisplayName = displayName;
        }

        #endregion Properties
    }
}
