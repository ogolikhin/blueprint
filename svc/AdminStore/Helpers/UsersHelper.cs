﻿namespace AdminStore.Helpers
{
    public static class UsersHelper
    {
        public static string SortUsers(string sortString)
        {
            var orderField = "displayName";
            var sortArray = sortString.Split(',');
            foreach (var sort in sortArray)
            {
                switch (sort)
                {
                    case "source":
                        orderField = "source";
                        break;
                    case "-source":
                        orderField = "-source";
                        break;
                    case "enabled":
                        orderField = "enabled";
                        break;
                    case "-enabled":
                        orderField = "-enabled";
                        break;
                    case "license":
                        orderField = "license";
                        break;
                    case "-license":
                        orderField = "-license";
                        break;
                    case "role":
                        orderField = "role";
                        break;
                    case "-role":
                        orderField = "-role";
                        break;
                    case "department":
                        orderField = "department";
                        break;
                    case "-department":
                        orderField = "-department";
                        break;
                    case "title":
                        orderField = "title";
                        break;
                    case "-title":
                        orderField = "-title";
                        break;
                    case "email":
                        orderField = "email";
                        break;
                    case "-email":
                        orderField = "-email";
                        break;
                    case "displayName":
                        orderField = "displayName";
                        break;
                    case "-displayName":
                        orderField = "-displayName";
                        break;
                    case "login":
                        orderField = "login";
                        break;
                    case "-login":
                        orderField = "-login";
                        break;
                }
            }
            return orderField;
        }
    }
}