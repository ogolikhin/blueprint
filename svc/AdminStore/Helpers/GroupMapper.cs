using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models;
using AdminStore.Models.Enums;

namespace AdminStore.Helpers
{
    public static class GroupMapper
    {
        public static GroupDto Map(Group group)
        {
            var result =
                new GroupDto()
                {
                    Id = group.Id,
                    Name = group.Name,
                    Scope = group.Scope,
                    LicenseType = ((LicenseType) group.LicenseId).ToString(),
                    Source = ((UserGroupSource)group.Source).ToString()
                };
            return result;
        }

        public static List<GroupDto> Map(IEnumerable<Group> groups)
        {
            return groups.Select(Map).ToList();
        }
    }
}