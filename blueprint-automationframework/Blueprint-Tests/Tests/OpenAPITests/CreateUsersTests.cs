using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities.Factories;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class CreateUsersTests : TestBase
    {
//        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
//        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
//            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.OpenApiToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase(2)]
        [TestRail(0)]
        [Description("Create user with all properties")]
        public void CreateUser_VerifyUserCreated(int userCount)
        {
            GenerateListOfUserModelsWithRequiredValues(userCount);
/*
        public int Id { get; set; }
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
        
        // Setup:
        UserDataModel userModel = new UserDataModel()
            {
                Username = RandomGenerator.RandomAlphaNumeric(10),
                DisplayName = RandomGenerator.RandomAlphaNumeric(10),
                FirstName = RandomGenerator.RandomAlphaNumeric(10),
                LastName = RandomGenerator.RandomAlphaNumeric(10),
                Email = I18NHelper.FormatInvariant("{0}@{1}.com", RandomGenerator.RandomAlphaNumeric(5), RandomGenerator.RandomAlphaNumeric(5)),
                Title = RandomGenerator.RandomAlphaNumeric(10),
            Password = RandomGenerator.RandomAlphaNumeric(10),

                
                
                Department = RandomGenerator.RandomAlphaNumeric(10),
                LastName = RandomGenerator.RandomAlphaNumeric(10),
                Password = RandomGenerator.RandomAlphaNumeric(10),
                DisplayName = RandomGenerator.RandomAlphaNumeric(10),
            };



            OpenApiUser user = new OpenApiUser(username, firstname, lastname, password, displayname);

            user.


            // Execute: change the user's password.
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.ResetPassword(_adminUser, newPassword);
            }, "Password reset failed when we passed a valid username & password!");

            // Verify: make sure user can login with the new password.
            VerifyLogin(Helper, _adminUser.Username, newPassword); */
        }

        #region Private methods

        private static List<UserDataModel> GenerateListOfUserModelsWithRequiredValues(int userCount)
        {
            List<UserDataModel> userModels = null;

            for (int i = 1; i < userCount; i++)
            {
                UserDataModel userModel = new UserDataModel()
                {
                    Username = RandomGenerator.RandomAlphaNumeric(10),
                    DisplayName = RandomGenerator.RandomAlphaNumeric(10),
                    FirstName = RandomGenerator.RandomAlphaNumeric(10),
                    LastName = RandomGenerator.RandomAlphaNumeric(10),
                    Password = RandomGenerator.RandomAlphaNumeric(10),
                };

                userModels.Add(userModel);
            }
            return userModels;
        }
        #endregion Private methods
    }
}


