using System.Collections.Generic;

namespace Model
{
    public enum UserSource
    {
        Unknown = -1,
        Database = 0,
        Windows = 1
    }


    public interface IUser
    {
        #region Properties

        string Department { get; set; }                     // (NULLABLE)
        string Email { get; set; }                          // (NULLABLE)
        bool Enabled { get; set; }
        string FirstName { get; set; }
        List<IGroup> GroupMembership { get; }
        InstanceAdminRole? InstanceAdminRole { get; set; }  // (NULLABLE) From "InstanceAdminRoleId" field in database.
        string LastName { get; set; }
        LicenseType License { get; set; }                   // This isn't in the database, it's inferred by the Group memberships.
        string Password { get; set; }                       // (NULLABLE)
        IEnumerable<byte> Picture { get; set; }             // (NULLABLE) "Image_ImageId" in database.
        UserSource Source { get; }
        string Title { get; set; }                          // (NULLABLE)
        IBlueprintToken Token { get; set; }                 // This isn't in the database.
        string Username { get; set; }                       // i.e. "Login" field in database.

        #endregion Properties

        #region Serialized JSON Properties

        string DisplayName { get; set; }                    // (NULLABLE)
        int Id { get; set; }

        #endregion Serialized JSON Properties

        #region Methods

        /// <summary>
        /// Creates a new user on the Blueprint server.
        /// </summary>
        /// <param name="source">The source where this user is defined.</param>
        void CreateUser(UserSource source = UserSource.Database);

        /// <summary>
        /// Deletes a user from the Blueprint server.
        /// </summary>
        /// <param name="deleteFromDatabase">(optional) By default the user is only disabled by setting the EndTimestamp field.
        ///     Pass true to really delete the user from the database.</param>
        void DeleteUser(bool deleteFromDatabase = false);

        /// <summary>
        /// Sets the token for this user.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <exception cref="ArgumentException">If the specified token is invalid.</exception>
        void SetToken(string token);

        /// <summary>
        /// Updates the user on the Blueprint server with any changes that were made to this object.
        /// </summary>
        void UpdateUser();

        /// <summary>
        /// Tests whether the specified IUser is equal to this one.
        /// </summary>
        /// <param name="user">The User to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        bool Equals(IUser user);

        #endregion Methods
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]    // Ignore this warning.
    public interface IDatabaseUser : IUser
    {

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]    // Ignore this warning.
    public interface IWindowsUser : IUser
    {
        
    }
}
