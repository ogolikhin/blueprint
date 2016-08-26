using Newtonsoft.Json;
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

        [JsonIgnore]
        string Department { get; set; }                     // (NULLABLE)
        //[JsonIgnore] - For UserStoryTests
        string Email { get; set; }                          // (NULLABLE)
        [JsonIgnore]
        bool Enabled { get; set; }
        //[JsonIgnore] - For UserStoryTests
        string FirstName { get; set; }
        [JsonIgnore]
        List<IGroup> GroupMembership { get; }
        //[JsonIgnore] - For UserStoryTests
        InstanceAdminRole? InstanceAdminRole { get; set; }  // (NULLABLE) From "InstanceAdminRoleId" field in database.
        //[JsonIgnore] - For UserStoryTests
        string LastName { get; set; }
        [JsonIgnore]
        LicenseType License { get; set; }                   // This isn't in the database, it's inferred by the Group memberships.
        [JsonIgnore]
        string Password { get; set; }                       // (NULLABLE)
        [JsonIgnore]
        IEnumerable<byte> Picture { get; set; }             // (NULLABLE) "Image_ImageId" in database.
        [JsonIgnore]
        UserSource Source { get; }
        [JsonIgnore]
        string Title { get; set; }                          // (NULLABLE)
        [JsonIgnore]
        IBlueprintToken Token { get; set; }                 // This isn't in the database.
        //[JsonIgnore] - For UserStoryTests
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
