using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Common;
using Model.Factories;
using TestConfig;
using Utilities;

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

        private List<IGroup> _GroupMembership = new List<IGroup>();

        #region Properties

        // All User table fields are as follows:
        // [AllowFallback],[CurrentVersion],[Department],[DisplayName],[Email],[Enabled],[EndTimestamp],[EULAccepted],[ExpirePassword],[FirstName],[Guest],[Image_ImageId],[InstanceAdminRoleId],
        // [InvalidLogonAttemptsNumber],[LastInvalidLogonTimeStamp],[LastName],[LastPasswordChangeTimestamp],[Login],[Password],[Source],[StartTimestamp],[Title],[UserId],[UserSALT]

        public string Department { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public string FirstName { get; set; }
        public List<IGroup> GroupMembership { get { return _GroupMembership; } }

        public InstanceAdminRole? InstanceAdminRole { get; set; }

        public string LastName { get; set; }
        public LicenseType License { get; set; }
        public string Password { get; set; }
        public IEnumerable<byte> Picture { get; set; }
        public virtual UserSource Source { get { return UserSource.Unknown; } }
        public string Title { get; set; }
        public IBlueprintToken Token { get; set; } = new BlueprintToken();
        public int Id { get; set; }
        public string Username { get; set; }

        // These are fields not included by IUser:
        public bool? AllowFallback { get; set; }
        public int CurrentVersion { get; set; }
        public string EncryptedPassword { get; set; }
        public DateTime? EndTimestamp { get; set; }
        public bool EULAccepted { get; set; }
        public bool? ExpirePassword { get; set; }
        public bool Guest { get; set; }
        public int InvalidLogonAttemptsNumber { get; set; }
        public DateTime? LastInvalidLogonTimeStamp { get; set; }
        public DateTime? LastPasswordChangeTimestamp { get; set; }
        public DateTime StartTimestamp { get; set; }
        public Guid UserSALT { get; set; }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected User()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Copy constructor.  Creates a deep copy of the specified user.
        /// </summary>
        /// <param name="user">The user to copy.</param>
        protected User(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            User userToCopy = user as User;

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

            GroupMembership.AddRange(userToCopy.GroupMembership);
        }

        /// <summary>
        /// Creates a new user on the Blueprint server.
        /// </summary>
        /// <param name="source">The source where this user is defined.</param>
        public abstract void CreateUser(UserSource source = UserSource.Database);

        /// <summary>
        /// Deletes a user from the Blueprint server.
        /// </summary>
        /// <param name="deleteFromDatabase">(optional) By default the user is only disabled by setting the EndTimestamp field.
        ///     Pass true to really delete the user from the database.</param>
        public abstract void DeleteUser(bool deleteFromDatabase = false);

        /// <summary>
        /// Sets the token for this user.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <exception cref="ArgumentException">If the specified token is invalid.</exception>
        public void SetToken(string token)
        {
            Token.SetToken(token);
        }

        /// <summary>
        /// Returns this object as a string.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            string str = I18NHelper.FormatInvariant("[User: Username = '{0}', Department = '{1}', DisplayName = '{2}', Email = '{3}', Enabled = '{4}', FirstName = '{5}', " +
                "InstanceAdminRole = '{6}', LastName = '{7}', License = '{8}', Password = '{9}', Picture = '{10}', Source = '{11}', Title = '{12}']",
                Username, toStringOrNull(Department), toStringOrNull(DisplayName), toStringOrNull(Email), Enabled, FirstName,
                 InstanceAdminRole, LastName, License, toStringOrNull(Password), (Picture != null) && (Picture.Any()), Source, toStringOrNull(Title));

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
        protected static string toStringOrNull(object value)
        {
            if (value == null) { return "NULL"; }

            return value.ToString();
        }

        #endregion Methods
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
            List<string> strList = new List<string>();

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
            using (IDatabase database = DatabaseFactory.CreateDatabase())
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

                using (SqlCommand cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            int userIdOrdinal = sqlDataReader.GetOrdinal("UserId");
                            Id = (int)(sqlDataReader.GetSqlInt32(userIdOrdinal));
                            //UserId = (int)(sqlDataReader.GetSqlInt32(0));
                        }
                    }
                    else
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a user from the Blueprint server.
        /// </summary>
        /// <param name="deleteFromDatabase">(optional) By default the user is only disabled by setting the EndTimestamp field.
        ///     Pass true to really delete the user from the database.</param>
        public override void DeleteUser(bool deleteFromDatabase = false)
        {
            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                int rowsAffected = 0;
                string query = null;
                DateTime? oldEndTimestamp = EndTimestamp;

                if (deleteFromDatabase)
                {
                    query = I18NHelper.FormatInvariant("DELETE FROM {0} WHERE Login='{1}'", USERS_TABLE, Username);
                }
                else
                {
                    EndTimestamp = DateTime.Now;
                    query = I18NHelper.FormatInvariant("UPDATE {0} SET EndTimestamp='{1}' WHERE Login='{2}' and EndTimestamp is NULL", USERS_TABLE, dateTimeToString(EndTimestamp.Value), Username);
                }

                Logger.WriteDebug("Running: {0}", query);

                try
                {
                    using (SqlCommand cmd = database.CreateSqlCommand(query))
                    {
                        rowsAffected = cmd.ExecuteNonQuery();
                    }

                    if (rowsAffected <= 0)
                    {
                        string msg = I18NHelper.FormatInvariant("No rows were affected when running: {0}", query);
                        Logger.WriteError(msg);
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
                toStringOrNull(AllowFallback), CurrentVersion, toStringOrNull(EndTimestamp), EULAccepted, toStringOrNull(ExpirePassword), Guest, InvalidLogonAttemptsNumber, toStringOrNull(LastInvalidLogonTimeStamp),
                toStringOrNull(LastPasswordChangeTimestamp), StartTimestamp, UserSALT);

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

        /// <summary>
        /// Deletes a user from the Blueprint server.
        /// </summary>
        /// <param name="deleteFromDatabase">(optional) By default the user is only disabled by setting the EndTimestamp field.
        ///     Pass true to really delete the user from the database.</param>
        public override void DeleteUser(bool deleteFromDatabase = false)
        {
            throw new NotImplementedException();
        }
    }
}
