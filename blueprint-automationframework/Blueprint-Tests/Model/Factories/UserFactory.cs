using Common;
using Model.Common.Enums;
using Model.Impl;
using System;
using System.Collections.Generic;
using TestConfig;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class UserFactory
    {
        /// <summary>
        /// Creates a deep copy of the specified user.
        /// </summary>
        /// <param name="user">The user to copy.</param>
        /// <returns>A new user that has the same data as the specified user.</returns>
        public static IUser CreateCopyOfUser(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            IUser copy = null;

            if (user.Source == UserSource.Database)
            {
                copy = new DatabaseUser(user as DatabaseUser);
            }
            else if (user.Source == UserSource.Windows)
            {
                copy = new WindowsUser(user as WindowsUser);
            }
            else
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("user.Source is set to an invalid type: '{0}'", user.Source.ToString()));
            }

            return copy;
        }

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database.
        /// </summary>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new unique user object that was added to the database.</returns>
        public static IUser CreateUserAndAddToDatabase(InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            var user = CreateUserOnly(source);
            user.InstanceAdminRole = instanceAdminRole;
            user.CreateUser();
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username & password specified
        /// and adds it to the Blueprint database.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public static IUser CreateUserAndAddToDatabase(string username, string password,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            var user = CreateUserOnly(username, password, source);
            user.InstanceAdminRole = instanceAdminRole;
            user.CreateUser();
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username, password, and displayname specified
        /// and adds it to the Blueprint database.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="displayname">The displayname</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public static IUser CreateUserAndAddToDatabase(string username, string password, string displayname,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            var user = CreateUserOnly(username, password, source, displayname);
            user.InstanceAdminRole = instanceAdminRole;
            user.CreateUser();
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values.
        /// </summary>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new unique user object.</returns>
        public static IUser CreateUserOnly(UserSource source = UserSource.Database)
        {
            string username = RandomGenerator.RandomAlphaNumeric(10);
            string password = RandomGenerator.RandomAlphaNumeric(10);

            return CreateUserOnly(username, password, source);
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username & password specified.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="displayname">The displayname.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public static IUser CreateUserOnly(string username, string password, UserSource source = UserSource.Database, string displayname = null)
        {
            User user;

            if (source == UserSource.Database)
            {
                user = new DatabaseUser { Username = username, Password = password, StartTimestamp = DateTime.Now };
            }
            else if (source == UserSource.Windows)
            {
                user = new WindowsUser { Username = username, Password = password };
            }
            else
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("source cannot be set to '{0}'", source.ToString()));
            }

            // Generate random data for the rest of the user properties.
            user.Department = RandomGenerator.RandomAlphaNumeric(10);
            user.Email = I18NHelper.FormatInvariant("{0}@{1}.com", user.Username, RandomGenerator.RandomAlphaNumeric(10));
            user.Enabled = true;
            user.FirstName = RandomGenerator.RandomAlphaNumeric(10);
            user.InstanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator;
            user.LastName = RandomGenerator.RandomAlphaNumeric(10);
            user.DisplayName = displayname ?? I18NHelper.FormatInvariant("{0} {1}", user.FirstName, user.LastName);
            user.License = LicenseType.Author;
            user.Title = RandomGenerator.RandomAlphaNumeric(10);

            user.UserSALT = Guid.NewGuid();
            user.EncryptedPassword = HashingUtilities.GenerateSaltedHash(user.Password, user.UserSALT.ToString());

            return user;
        }

        /// <summary>
        /// Gets the user defined in the TestConfiguration.
        /// NOTE: This user may or may not exist in the database.
        /// </summary>
        /// <returns>The user object from the TestConfiguration.</returns>
        public static IUser GetUserFromTestConfig()
        {
            var testConfig = TestConfiguration.GetInstance();
            string username = testConfig.Username;
            string password = testConfig.Password;

            return CreateUserOnly(username, password);
        }

        /// <summary>
        /// Gets a list of users from the Blueprint server.
        /// </summary>
        /// <param name="includeDeletedUsers">(optional) Pass true if you want to get all users, including "deleted" users.
        /// By default, only users who have a NULL EndTimestamp field are returned.</param>
        /// <returns>A list of users.</returns>
        public static List<IUser> GetUsers(bool includeDeletedUsers = false)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                // All User table fields are as follows:
                // [AllowFallback],[CurrentVersion],[Department],[DisplayName],[Email],[Enabled],[EndTimestamp],[EULAccepted],[ExpirePassword],[FirstName],[Guest],[Image_ImageId],[InstanceAdminRoleId],
                // [InvalidLogonAttemptsNumber],[LastInvalidLogonTimeStamp],[LastName],[LastPasswordChangeTimestamp],[Login],[Password],[Source],[StartTimestamp],[Title],[UserId],[UserSALT]

                string query = I18NHelper.FormatInvariant("SELECT {0} FROM {1}", User.ALL_USER_FIELDS, User.USERS_TABLE);

                if (!includeDeletedUsers)
                {
                    query += " WHERE EndTimestamp is NULL";
                }

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                using (var reader = cmd.ExecuteReader())
                {
                    var users = new List<IUser>();

                    while (reader.Read())
                    {
                        UserSource source = (UserSource) DatabaseUtilities.GetValueOrDefault<int>(reader, "Source");
                        User user = (User)CreateUserOnly(source);

                        user.Department = DatabaseUtilities.GetValueOrDefault<string>(reader, "Department");
                        user.DisplayName = DatabaseUtilities.GetValueOrDefault<string>(reader, "DisplayName");
                        user.Email = DatabaseUtilities.GetValueOrDefault<string>(reader, "Email");
                        user.EncryptedPassword = DatabaseUtilities.GetValueOrDefault<string>(reader, "Password");
                        user.Enabled = DatabaseUtilities.GetValueOrDefault<bool>(reader, "Enabled");
                        user.FirstName = DatabaseUtilities.GetValueOrDefault<string>(reader, "FirstName");
                        // TODO: Get Group Membership list.
                        user.InstanceAdminRole = (InstanceAdminRole) DatabaseUtilities.GetValueOrDefault<int>(reader, "InstanceAdminRoleId");
                        user.LastName = DatabaseUtilities.GetValueOrDefault<string>(reader, "LastName");
                        //user.License = ??
                        //user.Password = ?? (can we decrypt the password?)
                        //user.Picture = ??
                        user.Picture = null;
                        user.Title = DatabaseUtilities.GetValueOrDefault<string>(reader, "Title");
                        user.Id = DatabaseUtilities.GetValueOrDefault<int>(reader, "UserId");
                        user.Username = DatabaseUtilities.GetValueOrDefault<string>(reader, "Login");

                        // These are properties not in IUser:
                        user.AllowFallback = DatabaseUtilities.GetValueOrNull<bool>(reader, "AllowFallback");
                        user.CurrentVersion = DatabaseUtilities.GetValueOrDefault<int>(reader, "CurrentVersion");
                        user.EndTimestamp = DatabaseUtilities.GetValueOrNull<DateTime>(reader, "EndTimestamp");
                        user.EULAccepted = DatabaseUtilities.GetValueOrDefault<bool>(reader, "EULAccepted");
                        user.ExpirePassword = DatabaseUtilities.GetValueOrNull<bool>(reader, "ExpirePassword");
                        user.Guest = DatabaseUtilities.GetValueOrDefault<bool>(reader, "Guest");
                        user.InvalidLogonAttemptsNumber = DatabaseUtilities.GetValueOrDefault<int>(reader, "InvalidLogonAttemptsNumber");
                        user.LastInvalidLogonTimeStamp = DatabaseUtilities.GetValueOrNull<DateTime>(reader, "LastInvalidLogonTimeStamp");
                        user.LastPasswordChangeTimestamp = DatabaseUtilities.GetValueOrNull<DateTime>(reader, "LastPasswordChangeTimestamp");
                        user.StartTimestamp = DatabaseUtilities.GetValueOrDefault<DateTime>(reader, "StartTimestamp");
                        user.UserSALT = DatabaseUtilities.GetValueOrDefault<Guid>(reader, "UserSALT");

                        Logger.WriteTrace(user.ToString());

                        users.Add(user);
                    }

                    return users;
                }
            }
        }
    }
}
