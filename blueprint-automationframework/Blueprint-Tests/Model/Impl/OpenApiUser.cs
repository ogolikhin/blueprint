﻿using System.Collections.Generic;
using Utilities;

namespace Model.Impl
{
    public class OpenApiUser
    {
        protected UserDataModel UserData { get; set; }

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
        /// Default constructor.
        /// </summary>
        public OpenApiUser()
        { 
        }

        /// <summary>
        /// Creates user with specified mandatory properties
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="firstname">User first name</param>
        /// <param name="lastname">User last name</param>
        /// <param name="password">User password</param>
        /// <param name="displayname">User display name</param>
        public OpenApiUser(string username, string firstname, string lastname, string password, string displayname)
        {
            Username = username;
            FirstName = firstname;
            LastName = lastname;
            Password = password;
            DisplayName = displayname;
        }

        /// <summary>
        /// Creates OpenApiUser object with user data model
        /// </summary>
        /// <param name="userData">User data model</param>
        public OpenApiUser(UserDataModel userData)
        {
            ThrowIf.ArgumentNull(userData, nameof(userData));

            UserData = userData;
        }

        #endregion Constructors
    }
}
