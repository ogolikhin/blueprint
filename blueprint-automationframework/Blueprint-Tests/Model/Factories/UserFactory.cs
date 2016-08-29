using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Common;
using Model.Impl;
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
            IUser user = CreateUserOnly(source);
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
            IUser user = CreateUserOnly(username, password, source);
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
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public static IUser CreateUserOnly(string username, string password, UserSource source = UserSource.Database)
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
            user.DisplayName = I18NHelper.FormatInvariant("{0} {1}", user.FirstName, user.LastName);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IUser GetUserFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static List<IUser> GetUsers(bool includeDeletedUsers = false)
        {
            using (IDatabase database = DatabaseFactory.CreateDatabase())
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

                using (SqlCommand cmd = database.CreateSqlCommand(query))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<IUser> users = new List<IUser>();

                    while (reader.Read())
                    {
                        UserSource source = (UserSource)GetValueOrDefault<int>(reader, "Source");
                        User user = (User)CreateUserOnly(source);

                        user.Department = GetValueOrDefault<string>(reader, "Department");
                        user.DisplayName = GetValueOrDefault<string>(reader, "DisplayName");
                        user.Email = GetValueOrDefault<string>(reader, "Email");
                        user.EncryptedPassword = GetValueOrDefault<string>(reader, "Password");
                        user.Enabled = GetValueOrDefault<bool>(reader, "Enabled");
                        user.FirstName = GetValueOrDefault<string>(reader, "FirstName");
                        // TODO: Get Group Membership list.
                        user.InstanceAdminRole = (InstanceAdminRole)GetValueOrDefault<int>(reader, "InstanceAdminRoleId");
                        user.LastName = GetValueOrDefault<string>(reader, "LastName");
                        //user.License = ??
                        //user.Password = ?? (can we decrypt the password?)
                        //user.Picture = ??
                        user.Picture = null;
                        user.Title = GetValueOrDefault<string>(reader, "Title");
                        user.Id = GetValueOrDefault<int>(reader, "UserId");
                        user.Username = GetValueOrDefault<string>(reader, "Login");

                        // These are properties not in IUser:
                        user.AllowFallback = GetValueOrNull<bool>(reader, "AllowFallback");
                        user.CurrentVersion = GetValueOrDefault<int>(reader, "CurrentVersion");
                        user.EndTimestamp = GetValueOrNull<DateTime>(reader, "EndTimestamp");
                        user.EULAccepted = GetValueOrDefault<bool>(reader, "EULAccepted");
                        user.ExpirePassword = GetValueOrNull<bool>(reader, "ExpirePassword");
                        user.Guest = GetValueOrDefault<bool>(reader, "Guest");
                        user.InvalidLogonAttemptsNumber = GetValueOrDefault<int>(reader, "InvalidLogonAttemptsNumber");
                        user.LastInvalidLogonTimeStamp = GetValueOrNull<DateTime>(reader, "LastInvalidLogonTimeStamp");
                        user.LastPasswordChangeTimestamp = GetValueOrNull<DateTime>(reader, "LastPasswordChangeTimestamp");
                        user.StartTimestamp = GetValueOrDefault<DateTime>(reader, "StartTimestamp");
                        user.UserSALT = GetValueOrDefault<Guid>(reader, "UserSALT");

                        Logger.WriteTrace(user.ToString());

                        users.Add(user);
                    }

                    return users;
                }
            }
        }

        #region private functions

        /// <summary>
        /// Gets the specified field from the SqlDataReader stream, or returns the default value if the field is null.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="reader">The SqlDataReader that holds the query results.</param>
        /// <param name="name">The name of the field to get.</param>
        /// <returns>The field value or the default value for the specified type.</returns>
        private static T GetValueOrDefault<T>(SqlDataReader reader, string name)
        {
            ThrowIf.ArgumentNull(reader, nameof(reader));
            ThrowIf.ArgumentNull(name, nameof(name));

            try
            {
                int ordinal = reader.GetOrdinal(name);

                if (reader.IsDBNull(ordinal))
                {
                    return default(T);
                }

                return (T)reader.GetValue(ordinal);
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.WriteError("*** Caught a IndexOutOfRangeException with field: {0}\n{1}", name, e.Message);
                return default(T);
            }
        }

        /// <summary>
        /// Gets the specified field from the SqlDataReader stream, or null.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="reader">The SqlDataReader that holds the query results.</param>
        /// <param name="name">The name of the field to get.</param>
        /// <returns>The field value or null.</returns>
        private static Nullable<T> GetValueOrNull<T>(SqlDataReader reader, string name) where T : struct
        {
            ThrowIf.ArgumentNull(reader, nameof(reader));
            ThrowIf.ArgumentNull(name, nameof(name));

            try
            {
                int ordinal = reader.GetOrdinal(name);

                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return (T)reader.GetValue(ordinal);
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.WriteError("*** Caught a IndexOutOfRangeException with field: {0}\n{1}", name, e.Message);
                return default(T);
            }
        }

        #endregion private functions
    }
}
