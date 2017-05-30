﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Helpers
{
    public class GroupsHelper
    {
        public static string SortGroups(Sorting sorting)
        {
            var defaultSortColumn = "name";

            var sortableColumns = new HashSet<string>
            {
                defaultSortColumn,
                "scope",
                "licenseType",
                "email",
                "groupType"
            };
            
            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }      
    }
}