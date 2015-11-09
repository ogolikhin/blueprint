using System.Linq;

namespace AdminStore.Helpers
{
    public struct LoginInfo
    {
        public string Domain { get; set; }

        public string UserName { get; set; }

        public string DomainUserName
        {
            get { return string.Format("{0}{1}", Domain, UserName); }
        }

        public static LoginInfo Parse(string login)
        {
            var loginInfo = new LoginInfo();
            if (!string.IsNullOrWhiteSpace(login))
            {
                var result = login.Split('\\');
                loginInfo.Domain = result.First();
                loginInfo.UserName = result.Reverse().LastOrDefault();
            }
            return loginInfo;
        }
    }
}