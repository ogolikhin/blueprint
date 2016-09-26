using System.Text;
using System.Text.RegularExpressions;
using AdminStore.Models;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public static class LdapHelper
    {
        public const string DefaultAccountNameAttribute = "samaccountname";
        public const string DefaultUserObjectCategoryAttribute = "user";

        public static string EscapeLdapSearchFilter(string searchFilter)
        {
            if (searchFilter == null)
            {
                return null;
            }

            //http://stackoverflow.com/questions/649149/how-to-escape-a-string-in-c-for-use-in-an-ldap-query/694915#694915
            StringBuilder escape = new StringBuilder();
            foreach (char current in searchFilter)
            {
                switch (current)
                {
                    case '\\':
                        escape.Append(@"\5c");
                        break;
                    case '*':
                        escape.Append(@"\2a");
                        break;
                    case '(':
                        escape.Append(@"\28");
                        break;
                    case ')':
                        escape.Append(@"\29");
                        break;
                    case '\u0000':
                        escape.Append(@"\00");
                        break;
                    case '/':
                        escape.Append(@"\2f");
                        break;
                    default:
                        escape.Append(current);
                        break;
                }
            }

            return escape.ToString();
        }

        public static string GetEffectiveAccountNameAttribute(this LdapSettings ldapSettings)
        {
            return ldapSettings.EnableCustomSettings
                ? (string.IsNullOrWhiteSpace(ldapSettings.AccountNameAttribute) ? DefaultAccountNameAttribute : ldapSettings.AccountNameAttribute)
                : DefaultAccountNameAttribute;
        }

        public static string GetEffectiveUserObjectCategoryAttribute(this LdapSettings ldapSettings)
        {
            return ldapSettings.EnableCustomSettings
                ? (string.IsNullOrWhiteSpace(ldapSettings.UserObjectCategoryAttribute) ? DefaultUserObjectCategoryAttribute : ldapSettings.UserObjectCategoryAttribute)
                : DefaultUserObjectCategoryAttribute;
        }

        public static bool MatchesDomain(this LdapSettings ldapSettings, string domain)
        {
            const string pattern = @"DC=([\w\s\-\&]+)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(ldapSettings.LdapAuthenticationUrl);

            if (match.Groups.Count < 2)
                return false;

            var dc = match.Groups[1].Value.Trim();

            return dc.EqualsOrdinalIgnoreCase(domain == null ? "" : domain.Trim());
        }
    }
}
