using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;

namespace Model.Impl
{
    public class UserDataModel
    {
        #region Properties

        // NOTE: Keep the copy constructor up to date if any properties are added or removed from this class!

        [JsonProperty("Type")]
        public string UserOrGroupType { get; set; }

        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
        public List<IGroup> GroupMembership { get; set; } = new List<IGroup>();
        public InstanceAdminRole? InstanceAdminRole { get; set; }
        public bool? ExpirePassword { get; set; }
        public bool Enabled { get; set; }

        #endregion Properties

        #region Constructors

        public UserDataModel()
        {
            UserOrGroupType = "User";
        }

        /// <summary>
        /// Copy constructor.  Creates an instance of this class with data copied from another UserDataModel.
        /// </summary>
        /// <param name="userDataToCopy">The UserData to be copied from.</param>
        public UserDataModel(UserDataModel userDataToCopy)
        {
            ThrowIf.ArgumentNull(userDataToCopy, nameof(userDataToCopy));

            UserOrGroupType = userDataToCopy.UserOrGroupType;
            Id = userDataToCopy.Id;
            Username = userDataToCopy.Username;
            DisplayName = userDataToCopy.DisplayName;
            FirstName = userDataToCopy.FirstName;
            LastName = userDataToCopy.LastName;
            Email = userDataToCopy.Email;
            Title = userDataToCopy.Title;
            Department = userDataToCopy.Department;
            Password = userDataToCopy.Password;
            GroupMembership = new List<IGroup>(userDataToCopy.GroupMembership);
            InstanceAdminRole = userDataToCopy.InstanceAdminRole;
            ExpirePassword = userDataToCopy.ExpirePassword;
            Enabled = userDataToCopy.Enabled;
        }

        #endregion Constructors

        /// <summary>
        /// Asserts that the properties of 2 UserDataModel's are equal.
        /// </summary>
        /// <param name="expectedUserData">The expected UserDataModel.</param>
        /// <param name="actualUserData">The actual UserDataModel.</param>
        public static void AssertAreEqual(UserDataModel expectedUserData, UserDataModel actualUserData)
        {
            ThrowIf.ArgumentNull(expectedUserData, nameof(expectedUserData));
            ThrowIf.ArgumentNull(actualUserData, nameof(actualUserData));

            Assert.AreEqual(expectedUserData.UserOrGroupType, actualUserData.UserOrGroupType, "'{0}' has a different value than expected!", nameof(UserOrGroupType));
            Assert.AreEqual(expectedUserData.Id, actualUserData.Id, "'{0}' has a different value than expected!", nameof(Id));
            Assert.AreEqual(expectedUserData.Username, actualUserData.Username, "'{0}' has a different value than expected!", nameof(Username));
            Assert.AreEqual(expectedUserData.DisplayName, actualUserData.DisplayName, "'{0}' has a different value than expected!", nameof(DisplayName));
            Assert.AreEqual(expectedUserData.FirstName, actualUserData.FirstName, "'{0}' has a different value than expected!", nameof(FirstName));
            Assert.AreEqual(expectedUserData.LastName, actualUserData.LastName, "'{0}' has a different value than expected!", nameof(LastName));
            Assert.AreEqual(expectedUserData.Email, actualUserData.Email, "'{0}' has a different value than expected!", nameof(Email));
            Assert.AreEqual(expectedUserData.Title, actualUserData.Title, "'{0}' has a different value than expected!", nameof(Title));
            Assert.AreEqual(expectedUserData.Department, actualUserData.Department, "'{0}' has a different value than expected!", nameof(Department));
            Assert.AreEqual(expectedUserData.Password, actualUserData.Password, "'{0}' has a different value than expected!", nameof(Password));
            Assert.AreEqual(expectedUserData.InstanceAdminRole, actualUserData.InstanceAdminRole, "'{0}' has a different value than expected!", nameof(InstanceAdminRole));
            Assert.AreEqual(expectedUserData.ExpirePassword, actualUserData.ExpirePassword, "'{0}' has a different value than expected!", nameof(ExpirePassword));
            Assert.AreEqual(expectedUserData.Enabled, actualUserData.Enabled, "'{0}' has a different value than expected!", nameof(Enabled));

            Assert.AreEqual(expectedUserData.GroupMembership.Count, actualUserData.GroupMembership.Count, "'{0}' has a different number of groups than expected!", nameof(GroupMembership));

            for (int i = 0; i < expectedUserData.GroupMembership.Count; ++i)
            {
                Assert.AreEqual(expectedUserData.GroupMembership[i], actualUserData.GroupMembership[i], "'{0}[{1}]' has a different value than expected!", nameof(GroupMembership), i);
            }
        }
    }
}
