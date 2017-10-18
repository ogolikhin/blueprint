using System.Collections.Generic;
using System.Linq;
using AdminStore.Models;

namespace AdminStore.Helpers
{
    public static class UserMapper
    {
        public static UserDto Map(User user)
        {
            var result =
                new UserDto()
                {
                    Id = user.Id,
                    Title = user.Title,
                    AllowFallback = user.AllowFallback,
                    CurrentVersion = user.CurrentVersion,
                    Department = user.Department,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Enabled = user.Enabled,
                    ExpirePassword = user.ExpirePassword,
                    FirstName = user.FirstName,
                    Guest = user.Guest,
                    Image_ImageId = user.Image_ImageId,
                    InstanceAdminRoleId = user.InstanceAdminRoleId,
                    Login = user.Login,
                    LastName = user.LastName,
                    Source = user.Source,
                    LicenseType = user.LicenseType
                };
            return result;
        }

        public static List<UserDto> Map(IEnumerable<User> users)
        {
            return users.Select(Map).ToList();
        }
    }
}