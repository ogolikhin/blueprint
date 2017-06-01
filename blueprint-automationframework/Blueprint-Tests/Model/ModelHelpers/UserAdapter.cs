using System;
using System.Collections.Generic;
using Model.Common.Enums;
using Model.Impl;
using Model.InstanceAdminModel;

namespace Model.ModelHelpers
{
    public class UserAdapter : IUser
    {
        public InstanceUser InstanceUser { get; set; }

        public UserAdapter(InstanceUser instanceUser)
        {
            InstanceUser = instanceUser;
        }

        #region Properties

        public UserDataModel UserData
        {
            get { throw new NotSupportedException("This isn't a real IUser.  It's just wrapping an InstanceUser."); }
        }

        public CustomInstanceAdminRole CustomInstanceAdminRole { get; set; }

        public string Department
        {
            get { return InstanceUser.Department; }
            set { InstanceUser.Department = value; }
        }

        public string Email
        {
            get { return InstanceUser.Email; }
            set { InstanceUser.Email = value; }
        }

        public bool? Enabled
        {
            get { return InstanceUser.Enabled; }
            set { InstanceUser.Enabled = value ?? false; }
        }

        public string FirstName
        {
            get { return InstanceUser.FirstName; }
            set { InstanceUser.FirstName = value; }
        }

        public List<IGroup> GroupMembership { get; } = new List<IGroup>();

        public InstanceAdminRole? InstanceAdminRole
        {
            get { return (InstanceAdminRole?)CustomInstanceAdminRole?.Id ?? InstanceUser.InstanceAdminRoleId; }
            set { InstanceUser.InstanceAdminRoleId = value; }
        }

        public string LastName
        {
            get { return InstanceUser.LastName; }
            set { InstanceUser.LastName = value; }
        }

        public LicenseLevel? License
        {
            get { return InstanceUser.LicenseType; }
            set { InstanceUser.LicenseType = value; }
        }

        public string Password
        {
            get { return InstanceUser.Password; }
            set { InstanceUser.Password = value; }
        }

        public IEnumerable<byte> Picture { get; set; }

        public UserSource Source
        {
            get { return InstanceUser.Source.Value; }
        }

        public string Title
        {
            get { return InstanceUser.Title; }
            set { InstanceUser.Title = value; }
        }

        public IBlueprintToken Token { get; set; }

        public string Username
        {
            get { return InstanceUser.Login; }
            set { InstanceUser.Login = value; }
        }

        public bool? FallBack
        {
            get { return InstanceUser.AllowFallback; }
            set { InstanceUser.AllowFallback = value; }
        }

        public string DisplayName
        {
            get { return InstanceUser.DisplayName; }
            set { InstanceUser.DisplayName = value; }
        }

        public int Id
        {
            get { return InstanceUser.Id.Value; }
            set { InstanceUser.Id = value; }
        }

        public void CreateUser(UserSource source = UserSource.Database)
        {
            throw new NotImplementedException("This isn't a real IUser.  It's just wrapping an InstanceUser.");
        }

        public void DeleteUser(bool useSqlUpdate = true)
        {
            throw new NotImplementedException("This isn't a real IUser.  It's just wrapping an InstanceUser.");
        }

        public void SetUserIcon(int userId, byte[] value)
        {
            throw new NotImplementedException("This isn't a real IUser.  It's just wrapping an InstanceUser.");
        }

        public void ChangeLastPasswordChangeTimestamp(DateTime dateTime)
        {
            throw new NotImplementedException("This isn't a real IUser.  It's just wrapping an InstanceUser.");
        }

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

        public void UpdateUser()
        {
            throw new NotImplementedException("This isn't a real IUser.  It's just wrapping an InstanceUser.");
        }

        public bool Equals(IUser user)
        {
            if (user == null)
            {
                return false;
            }

            return user.Equals(this);
        }

        #endregion Properties
    }
}
