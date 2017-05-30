using System;
using System.Collections.Generic;
using Model.Impl;
using Model.Common.Enums;
using Model.InstanceAdminModel;

namespace Model
{
    public interface IUser
    {
        #region Properties

        UserDataModel UserData { get; }

        CustomInstanceAdminRole CustomInstanceAdminRole { get; set; }
        string Department { get; set; }                     // (NULLABLE)
        string Email { get; set; }                          // (NULLABLE)
        bool? Enabled { get; set; }
        string FirstName { get; set; }
        List<IGroup> GroupMembership { get; }
        InstanceAdminRole? InstanceAdminRole { get; set; }  // (NULLABLE) From "InstanceAdminRoleId" field in database.
        string LastName { get; set; }
        LicenseLevel? License { get; set; }                   // This isn't in the database, it's inferred by the Group memberships.
        string Password { get; set; }                       // (NULLABLE)
        IEnumerable<byte> Picture { get; set; }             // (NULLABLE) "Image_ImageId" in database.
        UserSource Source { get; }
        string Title { get; set; }                          // (NULLABLE)
        IBlueprintToken Token { get; set; }                 // This isn't in the database.
        string Username { get; set; }                       // i.e. "Login" field in database.
        bool? FallBack { set; get; }

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
        /// <param name="useSqlUpdate">(optional) By default the user is deleted by a REST call.
        ///     Pass true to update the user in the database by setting the EndTimestamp field instead of the REST call.</param>
        void DeleteUser(bool useSqlUpdate = true);  // TODO: Change useSqlUpdate = false when OpenAPI Delete call is working.

        /// <summary>
        ///  Adds an icon for user
        /// </summary>
        /// <param name="userId">User Id to which icon will be added</param>
        /// <param name="value">Icon raw data</param>
        void SetUserIcon(int userId, byte[] value);

        /// <summary>
        /// Change LastPassswordChangeTimestamp.
        /// </summary>
        /// <param name="dateTime">dataTime representing new value for LastPassswordChangeTimestamp.</param>
        void ChangeLastPasswordChangeTimestamp(DateTime dateTime);

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
