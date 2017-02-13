using Common;
using System.Collections.Generic;
using Utilities.Factories;

namespace Model.Impl
{
    public class OpenApiUser : IOpenApiUser
    {
        protected UserDataModel UserData { get; set; }

        public UserSource Source { get { return UserSource.Unknown; } }

        #region Serialized JSON Properties

        public int Id
        {
            get { return UserData.Id; }
            set { UserData.Id = value; }
        }
        public string Username
        {
            get { return UserData.Username; }
            set { UserData.Username = value; }
        }
        public string DisplayName
        {
            get { return UserData.DisplayName; }
            set { UserData.DisplayName = value; }
        }
        public string FirstName
        {
            get { return UserData.FirstName; }
            set { UserData.FirstName = value; }
        }
        public string LastName
        {
            get { return UserData.LastName; }
            set { UserData.LastName = value; }
        }
        public string Email
        {
            get { return UserData.Email; }
            set { UserData.Email = value; }
        }
        public string Title
        {
            get { return UserData.Title; }
            set { UserData.Title = value; }
        }
        public string Department
        {
            get { return UserData.Department; }
            set { UserData.Department = value; }
        }
        public string Password
        {
            get { return UserData.Password; }
            set { UserData.Password = value; }
        }
        public List<IGroup> GroupMembership
        {
            get { return UserData.GroupMembership; }
            set { UserData.GroupMembership = value; }
        }
        public InstanceAdminRole? InstanceAdminRole
        {
            get { return UserData.InstanceAdminRole; }
            set { UserData.InstanceAdminRole = value; }
        }
        public bool? ExpirePassword
        {
            get { return UserData.ExpirePassword; }
            set { UserData.ExpirePassword = value; }
        }
        public bool Enabled
        {
            get { return UserData.Enabled; }
            set { UserData.Enabled = value; }
        }

        #endregion Serialized JSON Properties

        #region Constructors

        /// <summary>
        /// Constructor with minimal amount of values to create user
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="password">Password</param>
        /// <param name="displayName">User display name</param>
        /// <param name="source">(optional) By default it is database user</param>
        public OpenApiUser(string userName, string firstName, string lastName, string password, string displayName, UserSource source = UserSource.Database)
        {
            Username = userName;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            DisplayName = displayName;
        }

        /// <summary>
        /// Constructor with minimal amount of values to create user
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="password">Password</param>
        /// <param name="displayName">User display name</param>
        public OpenApiUser()
        {

        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Default constructor. Generates database user with all necessary values.
        /// </summary>
        public void GenerateOpenApiUser(UserSource source = UserSource.Database)
        {
            Username = RandomGenerator.RandomAlphaNumeric(10);
            FirstName = RandomGenerator.RandomAlphaNumeric(10);
            LastName = RandomGenerator.RandomAlphaNumeric(10);
            Password = RandomGenerator.RandomAlphaNumeric(10);
            DisplayName = I18NHelper.FormatInvariant("{0} {1}", FirstName, LastName);
        }

        #endregion Methods
    }
}
