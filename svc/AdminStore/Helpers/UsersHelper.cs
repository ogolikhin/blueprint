using System.Collections.Generic;
using System.Linq;
using AdminStore.Models;

namespace AdminStore.Helpers
{
    public static class UsersHelper
    {
        public static List<User> SortUsers(List<User> users, string sortString)
        {
            var sortedUsers = users;
            var sortArray = sortString.Split(',');
            foreach (var sort in sortArray)
            {
                switch (sort)
                {
                    case "source":
                        sortedUsers = users.OrderBy(u => u.Source).ToList();
                        break;
                    case "-source":
                        sortedUsers = users.OrderByDescending(u => u.Source).ToList();
                        break;
                    case "enabled":
                        sortedUsers = users.OrderBy(u => u.Enabled).ToList();
                        break;
                    case "-enabled":
                        sortedUsers = users.OrderByDescending(u => u.Enabled).ToList();
                        break;
                    case "license":
                        sortedUsers = users.OrderBy(u => u.LicenseType).ToList();
                        break;
                    case "-license":
                        sortedUsers = users.OrderByDescending(u => u.LicenseType).ToList();
                        break;
                }
            }
            return sortedUsers;
        }
    }
}