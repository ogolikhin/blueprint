using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Factories;

namespace Model.Impl
{
    public class OpenApiUser : UserDataModel
    {
        protected UserDataModel UserData { get; set; }

        #region Constructors

        /// <summary>
        /// Default constructor. Generates database user with all necessary values.
        /// </summary>
        public OpenApiUser()
        {
            Source = UserSource.Database;
            Username = RandomGenerator.RandomAlphaNumeric(10);
            FirstName = RandomGenerator.RandomAlphaNumeric(10);
            LastName = RandomGenerator.RandomAlphaNumeric(10);
            Password = RandomGenerator.RandomAlphaNumeric(10);
            DisplayName = I18NHelper.FormatInvariant("{0} {1}", FirstName, LastName);
        }

        /// <summary>
        /// Constructor with minimal amount of values to create user
        /// </summary>
        /// <param name="source">Database or Windows user</param>
        /// <param name="username">Username</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="password">Password</param>
        /// <param name="displayName">User display name</param>
        public OpenApiUser(UserSource source, string userName, string firstName, string lastName, string password, string displayName)
        {
            Source = source;
            Username = userName;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            DisplayName = displayName;
        }

        #endregion Constructors
    }
}
