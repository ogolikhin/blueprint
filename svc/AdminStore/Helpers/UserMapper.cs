using System.Collections.Generic;
using System.Linq;
using AdminStore.Dto;
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
                    UserId = user.UserId,
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
                    Source = user.Source
                };
            switch (user.LicenseType)
            {
                //0 and 1 - the same values, got it from spec
                case 0:
                    result.LicenseType = "Viewer";
                    break;
                case 1:
                    result.LicenseType = "Viewer";
                    break;
                case 2:
                    result.LicenseType = "Collaborator";
                    break;
                case 3:
                    result.LicenseType = "Author";
                    break;
            }
            return result;
        }

        public static List<UserDto> Map(IEnumerable<User> users)
        {
            return users.Select(Map).ToList();
        } 
    }
}