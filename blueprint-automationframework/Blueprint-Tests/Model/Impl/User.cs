using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Model.Factories;
using Utilities;
using NUnit.Framework;
using System.Data;
using System.Text.RegularExpressions;

namespace Model.Impl
{
    public abstract class User : IUser
    {
        public const string USERS_TABLE = "[dbo].[Users]";

        // Please keep these fields in alphabetical order.
        public const string ALL_USER_FIELDS =
            "[AllowFallback],[CurrentVersion],[Department],[DisplayName],[Email],[Enabled],[EndTimestamp]," +
            "[EULAccepted],[ExpirePassword],[FirstName],[Guest],[Image_ImageId],[InstanceAdminRoleId]," +
            "[InvalidLogonAttemptsNumber],[LastInvalidLogonTimeStamp],[LastName],[LastPasswordChangeTimestamp]," +
            "[Login],[Password],[Source],[StartTimestamp],[Title],[UserId],[UserSALT]";

        protected UserDataModel UserData { get; set; }
        protected bool IsDeletedFromDatabase { get; set; }

        #region Properties

        public bool IsDeleted { get { return (!IsDeletedFromDatabase && (EndTimestamp != null)); } }

        // All User table fields are as follows:
        // [AllowFallback],[CurrentVersion],[Department],[DisplayName],[Email],[Enabled],[EndTimestamp],[EULAccepted],[ExpirePassword],[FirstName],[Guest],[Image_ImageId],[InstanceAdminRoleId],
        // [InvalidLogonAttemptsNumber],[LastInvalidLogonTimeStamp],[LastName],[LastPasswordChangeTimestamp],[Login],[Password],[Source],[StartTimestamp],[Title],[UserId],[UserSALT]
        public LicenseType License { get; set; }
        public IEnumerable<byte> Picture { get; set; }
        public virtual UserSource Source { get { return UserSource.Unknown; } }
        public IBlueprintToken Token { get; set; } = new BlueprintToken();

        // These are fields not included by IUser:
        public bool? AllowFallback { get; set; }
        public int CurrentVersion { get; set; }
        public string EncryptedPassword { get; set; }
        public DateTime? EndTimestamp { get; set; }
        public bool EULAccepted { get; set; }
        public bool Guest { get; set; }
        public int InvalidLogonAttemptsNumber { get; set; }
        public DateTime? LastInvalidLogonTimeStamp { get; set; }
        public DateTime? LastPasswordChangeTimestamp { get; set; }
        public DateTime StartTimestamp { get; set; }
        public Guid UserSALT { get; set; }

        #endregion Properties

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
            get { return UserData.Groups; }
            set { UserData.Groups = value; }
        }
        public InstanceAdminRole? InstanceAdminRole
        {
            get { return ConvertStringToInstanceAdminRole(UserData.InstanceAdminRole); }
            set { UserData.InstanceAdminRole = ConvertInstanceAdminRoleToString(value); }
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

        #region Methods

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected User()
        {
            // Intentionally left blank.
            UserData = new UserDataModel();
        }

        protected User(UserDataModel userData)
        {
            UserData = userData;
        }

        /// <summary>
        /// Copy constructor.  Creates a deep copy of the specified user.
        /// </summary>
        /// <param name="user">The user to copy.</param>
        protected User(IUser user) : this()
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var userToCopy = user as User;

            AllowFallback = userToCopy.AllowFallback;
            CurrentVersion = userToCopy.CurrentVersion;
            Department = userToCopy.Department;
            DisplayName = userToCopy.DisplayName;
            Email = userToCopy.Email;
            Enabled = userToCopy.Enabled;
            EncryptedPassword = userToCopy.EncryptedPassword;
            EndTimestamp = userToCopy.EndTimestamp;
            EULAccepted = userToCopy.EULAccepted;
            ExpirePassword = userToCopy.ExpirePassword;
            FirstName = userToCopy.FirstName;
            Guest = userToCopy.Guest;
            InstanceAdminRole = userToCopy.InstanceAdminRole;
            InvalidLogonAttemptsNumber = userToCopy.InvalidLogonAttemptsNumber;
            LastInvalidLogonTimeStamp = userToCopy.LastInvalidLogonTimeStamp;
            LastName = userToCopy.LastName;
            LastPasswordChangeTimestamp = userToCopy.LastPasswordChangeTimestamp;
            License = userToCopy.License;
            Password = userToCopy.Password;
            Picture = userToCopy.Picture;
            StartTimestamp = userToCopy.StartTimestamp;
            Title = userToCopy.Title;
            Token = userToCopy.Token;
            Id = userToCopy.Id;
            Username = userToCopy.Username;
            UserSALT = userToCopy.UserSALT;

            if (userToCopy.GroupMembership != null)
            {
                GroupMembership.AddRange(userToCopy.GroupMembership);
            }
        }

        /// <summary>
        /// Creates a new user on the Blueprint server.
        /// </summary>
        /// <param name="source">The source where this user is defined.</param>
        public abstract void CreateUser(UserSource source = UserSource.Database);

        /// <seealso cref="IUser.DeleteUser(bool)"/>
        public abstract void DeleteUser(bool useSqlUpdate = true);  // TODO: Change useSqlUpdate = false when OpenAPI Delete call is working.

        /// <summary>
        /// Sets the token for this user.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <exception cref="ArgumentException">If the specified token is invalid.</exception>
        public void SetToken(string token)
        {
            if (Token == null)
            {
                Token = new BlueprintToken();
            }

            Token.SetToken(token);
        }

        /// <summary>
        ///  Adds an icon for user
        /// </summary>
        /// <param name="userId">User Id to which icon will be added</param>
        /// <param name="value">Icon row data</param>
        public void SetUserIcon(int userId, byte[] value)
        {
            string query = "INSERT INTO [dbo].[Images] (Content) VALUES (@Content)";
            int rowsAffected = ExecuteInsertBinarySqlQuery(query, value);
            Assert.IsTrue(rowsAffected == 1, "The record was not inserted!");

            query = "SELECT ImageId FROM [dbo].[Images] WHERE Content = @Content";
            int imageId = ExecuteSelectBinarySqlQuery(query, value);
            Assert.IsTrue(imageId > 0, "The record was not inserted!");

            query = I18NHelper.FormatInvariant("UPDATE [dbo].[Users] SET Image_ImageId = {0} WHERE UserId = {1}", imageId, userId);
            rowsAffected = ExecuteUpdateBinarySqlQuery(query);
            Assert.IsTrue(rowsAffected == 1, "Updated more than one row in Users table!");
        }

        public void ChangeLastPasswordChangeTimestamp(DateTime dateTime)
        {
            string updatedDateString = dateTime.ToStringInvariant("yyyy-MM-dd HH:mm:ss");

            string query = I18NHelper.FormatInvariant("UPDATE [dbo].[Users] SET LastPasswordChangeTimestamp = '{0}' WHERE UserId = {1}",
                updatedDateString, Id);
            int rowsAffected = ExecuteUpdateBinarySqlQuery(query);
            Assert.IsTrue(rowsAffected == 1, "Update more than one row in Users table!");
        }

        /// <summary>
        /// Returns this object as a string.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            string str = I18NHelper.FormatInvariant("[User: Username = '{0}', Department = '{1}', DisplayName = '{2}', Email = '{3}', Enabled = '{4}', FirstName = '{5}', " +
                "InstanceAdminRole = '{6}', LastName = '{7}', License = '{8}', Password = '{9}', Picture = '{10}', Source = '{11}', Title = '{12}']",
                Username, ToStringOrNull(Department), ToStringOrNull(DisplayName), ToStringOrNull(Email), Enabled, FirstName,
                 InstanceAdminRole, LastName, License, ToStringOrNull(Password), (Picture != null) && (Picture.Any()), Source, ToStringOrNull(Title));

            return str;
        }

        /// <summary>
        /// Updates the user on the Blueprint server with any changes that were made to this object.
        /// </summary>
        public void UpdateUser()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tests whether the specified IUser is equal to this one.
        /// </summary>
        /// <param name="user">The User to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        public bool Equals(IUser user) //TODO: add compare for license
        {
            if (user == null)
            {
                return false;
            }
            else
            {
                return ((this.Username == user.Username) & (this.DisplayName == user.DisplayName) &&
                      (this.Email == user.Email) && (this.FirstName == user.FirstName) &&
                      (this.InstanceAdminRole == user.InstanceAdminRole) && (this.LastName == user.LastName)
                      && (this.Source == user.Source));
            }
        }

        /// <summary>
        /// Returns the string version of the object or "NULL" if it's null.
        /// </summary>
        /// <param name="value">The object to convert to a string.</param>
        /// <returns>The object as a string or the "NULL".</returns>
        protected static string ToStringOrNull(object value)
        {
            if (value == null) { return "NULL"; }

            return value.ToString();
        }

        #endregion Methods

        #region Private functions

        /// <summary>
        /// Executes insert binary data into row and verifies the row was inserted
        /// Example: "INSERT INTO [Blueprint].[dbo].[Images] (Content) VALUES (@Content)"
        /// </summary>
        /// <param name="insertQuery">SQL query to insert data</param>
        /// <param name="value">Actual binary data to insert</param>
        /// <returns>Amount of records inserted.</returns>
        private static int ExecuteInsertBinarySqlQuery(string insertQuery, byte[] value)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", insertQuery);

                using (var cmd = database.CreateSqlCommand(insertQuery))
                {
                    var param = cmd.Parameters.Add("@Content", SqlDbType.VarBinary);
                    param.Value = value;

                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.RecordsAffected <= 0)
                        {
                            throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", insertQuery));
                        }

                        return sqlDataReader.RecordsAffected;
                    }
                }
            }
        }

        /// <summary>
        /// Executes select query using binary content to find out image id
        /// Example: "SELECT ImageId FROM [Blueprint].[dbo].[Images] WHERE Content = @Content"
        /// </summary>
        /// <param name="selectQuery">SQL select query</param>
        /// <param name="content">Binary content to request</param>
        /// <returns>Image id</returns>
        public static int ExecuteSelectBinarySqlQuery(string selectQuery, byte[] content)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", selectQuery);

                using (var cmd = database.CreateSqlCommand(selectQuery))
                {
                    var param = cmd.Parameters.Add("@Content", SqlDbType.VarBinary);
                    param.Value = content;

                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.Read())
                        {
                            return DatabaseUtilities.GetValueOrDefault<int>(sqlDataReader, "ImageId");
                        }

                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were found when running: {0}", selectQuery));
                    }
                }
            }
        }

        /// <summary>
        /// Executes update query and returns number of rows affected
        /// Example: "UPDATE [dbo].[Users] SET Image_ImageId = {0} WHERE UserId = {1}"
        /// </summary>
        /// <param name="updateQuery">SQL update query</param>
        /// <returns>Amount of records affected</returns>
        public static int ExecuteUpdateBinarySqlQuery(string updateQuery)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", updateQuery);

                using (var cmd = database.CreateSqlCommand(updateQuery))
                {
                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.RecordsAffected <= 0)
                        {
                            throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", updateQuery));
                        }

                        return sqlDataReader.RecordsAffected;
                    }
                }
            }
        }

        /// <summary>
        /// Converting instance admin role string into InstanceAdminRole enum
        /// </summary>
        /// <param name="adminRole">Instance admin string</param>
        /// <returns>InstanceAdminRole enum value</returns>
        private static InstanceAdminRole? ConvertStringToInstanceAdminRole(string adminRole)
        {
            if (adminRole == null)
            {
                return null;
            }

            string enumString = Regex.Replace(adminRole, @"[\s+]|,", "");

            return (InstanceAdminRole)Enum.Parse(typeof(InstanceAdminRole), enumString);
        }

        /// <summary>
        /// Converting InstanceAdminRole enum value into string
        /// </summary>
        /// <param name="role">InstanceAdminRole enum value</param>
        /// <returns>Instance admin role string</returns>
        private static string ConvertInstanceAdminRoleToString(InstanceAdminRole? role)
        {
            if (role == null)
            {
                return null;
            }

            // Creates pattern to replace capital letters with the new character and capital letter
            // from https://gist.github.com/rymoore99/9091263
            var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(role.ToString(), " ");
        }

        #endregion Private function
    }

    public class DatabaseUser : User, IDatabaseUser
    {
        #region Properties

        public override UserSource Source { get { return UserSource.Database; } }

        #endregion Properties

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DatabaseUser() : base()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Copy constructor.  Creates a deep copy of the specified user.
        /// </summary>
        /// <param name="user">The user to copy.</param>
        public DatabaseUser(IDatabaseUser user) : base(user)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Gets the date in a string format that MS SQL can use.
        /// </summary>
        /// <param name="date">The date to convert to a string.</param>
        /// <returns>A string version of the date.</returns>
        private static string dateTimeToString(DateTime date)
        {
            string dateString = date.ToStringInvariant("yyyy-MM-dd HH:mm:ss");
            return dateString;
        }

        /// <summary>
        /// Converts the array of objects into a list of strings that are properly formatted and quoted for MS SQL to use.
        /// </summary>
        /// <param name="objArray">The array of objects to convert.</param>
        /// <returns>A list of strings that MS SQL can use.</returns>
        private static List<string> objArraytoStringList(object[] objArray)
        {
            var strList = new List<string>();

            foreach (object obj in objArray)
            {
                if (obj is bool) { strList.Add((bool)obj ? "1" : "0"); }
                else if (obj is int) { strList.Add(obj.ToString()); }
                else if (obj is DateTime) { strList.Add("'" + dateTimeToString((DateTime)obj) + "'"); }
                else if (obj == null) { strList.Add("NULL"); }
                else { strList.Add("'" + obj + "'"); }
            }

            return strList;
        }

        /// <summary>
        /// Creates a new user on the Blueprint server.
        /// </summary>
        /// <param name="source">The source where this user is defined.</param>
        /// <exception cref="SqlQueryFailedException">If no rows were affected.</exception>
        public override void CreateUser(UserSource source = UserSource.Database)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                var fields = "[AllowFallback],[CurrentVersion],[Department],[DisplayName],[Email],[Enabled],[EndTimestamp]," +
                "[EULAccepted],[ExpirePassword],[FirstName],[Guest],[Image_ImageId],[InstanceAdminRoleId]," +
                "[InvalidLogonAttemptsNumber],[LastInvalidLogonTimeStamp],[LastName],[LastPasswordChangeTimestamp]," +
                "[Login],[Password],[Source],[StartTimestamp],[Title],[UserSALT]";  // [UserId] is the Primary Key, so it gets created by SQL Server.

                object[] valueArray =
                {
                    AllowFallback, CurrentVersion, Department, DisplayName, Email, Enabled, EndTimestamp,
                    EULAccepted, ExpirePassword, FirstName, Guest, Picture, (int?)InstanceAdminRole,
                    InvalidLogonAttemptsNumber, LastInvalidLogonTimeStamp, LastName, LastPasswordChangeTimestamp,
                    Username, EncryptedPassword, (int)Source, StartTimestamp, Title, UserSALT
                };

                string values = string.Join(",", objArraytoStringList(valueArray));
                string query = I18NHelper.FormatInvariant("INSERT INTO {0} ({1}) Output Inserted.UserId VALUES ({2})", USERS_TABLE, fields, values);

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            int userIdOrdinal = sqlDataReader.GetOrdinal("UserId");
                            Id = (int)(sqlDataReader.GetSqlInt32(userIdOrdinal));
                        }

                        IsDeletedFromDatabase = false;
                    }
                    else
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                    }
                }
            }
        }

        /// <seealso cref="IUser.DeleteUser(bool)"/>
        public override void DeleteUser(bool useSqlUpdate = true)   // TODO: Change useSqlUpdate = false when OpenAPI Delete call is working.
        {
            if (IsDeletedFromDatabase)
            {
                return;
            }

            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                int rowsAffected = 0;
                string query = null;
                DateTime? oldEndTimestamp = EndTimestamp;

                if (useSqlUpdate)
                {
                    EndTimestamp = DateTime.Now;
                    query = I18NHelper.FormatInvariant("UPDATE {0} SET EndTimestamp='{1}' WHERE Login='{2}' and EndTimestamp is NULL",
                        USERS_TABLE, dateTimeToString(EndTimestamp.Value), Username);
                }

                Logger.WriteDebug("Running: {0}", query);

                try
                {
                    using (var cmd = database.CreateSqlCommand(query))
                    {
                        rowsAffected = cmd.ExecuteNonQuery();
                    }

                    if (rowsAffected <= 0)
                    {
                        string msg = I18NHelper.FormatInvariant("No rows were affected when running: {0}", query);
                        Logger.WriteError(msg);
                    }
                    else
                    {
                        IsDeletedFromDatabase = true;
                    }
                }
                catch
                {
                    EndTimestamp = oldEndTimestamp;
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns this object as a string.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            string str = base.ToString();
            str += I18NHelper.FormatInvariant(" + [AllowFallback = '{0}', CurrentVersion = '{1}', EndTimestamp = '{2}', EULAccepted = '{3}', ExpirePassword = '{4}', Guest = '{5}', " +
                "InvalidLogonAttemptsNumber = '{6}', LastInvalidLogonTimeStamp = '{7}', LastPasswordChangeTimestamp = '{8}', StartTimestamp = '{9}', UserSALT = '{10}']",
                ToStringOrNull(AllowFallback), CurrentVersion, ToStringOrNull(EndTimestamp), EULAccepted, ToStringOrNull(ExpirePassword), Guest, InvalidLogonAttemptsNumber, ToStringOrNull(LastInvalidLogonTimeStamp),
                ToStringOrNull(LastPasswordChangeTimestamp), StartTimestamp, UserSALT);

            return str;
        }
    }

    public class WindowsUser : User, IWindowsUser
    {
        public override UserSource Source { get { return UserSource.Windows; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WindowsUser() : base()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Copy constructor.  Creates a deep copy of the specified user.
        /// </summary>
        /// <param name="user">The user to copy.</param>
        public WindowsUser(IWindowsUser user) : base(user)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Creates a new user on the Blueprint server.
        /// </summary>
        /// <param name="source">The source where this user is defined.</param>
        public override void CreateUser(UserSource source = UserSource.Database)
        {
            throw new NotImplementedException();
        }

        /// <seealso cref="IUser.DeleteUser(bool)"/>
        public override void DeleteUser(bool useSqlUpdate = false)
        {
            throw new NotImplementedException();
        }
    }
}
