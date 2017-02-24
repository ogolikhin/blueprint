using Model.Impl;

namespace Model.Factories
{
    public static class UserDataModelFactory
    {
        /// <summary>
        /// Creates a new empty UserDataModel with only the Type set to 'User'.
        /// </summary>
        /// <returns>A new UserDataModel.</returns>
        public static UserDataModel CreateUserDataModel()
        {
            var userData = new UserDataModel();

            return userData;
        }

        /// <summary>
        /// Creates a new empty UserDataModel with only the Type set to 'User' and the Username.
        /// </summary>
        /// <param name="username">The username to set.</param>
        /// <returns>A new UserDataModel.</returns>
        public static UserDataModel CreateUserDataModel(string username)
        {
            var userData = CreateUserDataModel();
            userData.Username = username;

            return userData;
        }

        public static UserDataModel CreateCopyOfUserDataModel(UserDataModel userDataToCopy)
        {
            var newUserData = new UserDataModel(userDataToCopy);
            return newUserData;
        }
    }
}
