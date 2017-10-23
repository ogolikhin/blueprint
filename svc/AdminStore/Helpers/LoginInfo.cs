namespace AdminStore.Helpers
{
    public struct LoginInfo
    {
        public string LdapUrl { get; set; }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Login { get; internal set; }

        public static LoginInfo Parse(string login)
        {
            var loginInfo = new LoginInfo { Login = login };
            if (login != null)
            {
                var index = login.IndexOf('\\');
                if (index == -1)
                {
                    loginInfo.UserName = login;
                }
                else
                {
                    loginInfo.Domain = login.Substring(0, index);
                    loginInfo.UserName = login.Substring(index + 1);
                }
            }
            return loginInfo;
        }
    }
}
