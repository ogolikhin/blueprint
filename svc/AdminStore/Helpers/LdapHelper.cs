using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AdminStore.Models;

namespace AdminStore.Helpers
{
    public static class LdapHelper
    {
        public const string DefaultAccountNameAttribute = "samaccountname";

        public static string EscapeLdapSearchFilter(string searchFilter)
        {
            if (searchFilter == null)
            {
                return null;
            }

            //http://stackoverflow.com/questions/649149/how-to-escape-a-string-in-c-for-use-in-an-ldap-query/694915#694915
            StringBuilder escape = new StringBuilder();
            for (int i = 0; i < searchFilter.Length; ++i)
            {
                char current = searchFilter[i];
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

        public static string GetEffectiveDomainAttribute(this LdapSettings ldapSettings)
        {
            return ldapSettings.EnableCustomSettings
                ? (string.IsNullOrWhiteSpace(ldapSettings.DomainAttribute) ? null : ldapSettings.DomainAttribute)
                : null;
        }

        public static string GetEffectiveAccountNameAttribute(this LdapSettings ldapSettings)
        {
            return ldapSettings.EnableCustomSettings
                ? (string.IsNullOrWhiteSpace(ldapSettings.AccountNameAttribute) ? DefaultAccountNameAttribute : ldapSettings.AccountNameAttribute)
                : DefaultAccountNameAttribute;
        }

        public static bool MatchsUser(this LdapSettings ldapSettings, string domain)
        {
            const string pattern = @"(DC=[\w\s\-\&]+)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(ldapSettings.LdapAuthenticationUrl);

            if (match.Groups.Count < 1)
                return false;

            var dcMatch = match.Groups[0].Value.Trim();
            var ar = dcMatch.Split('=');

            if (ar.Length != 2)
                return false;

            var dc = ar[1].Trim();

            return string.Compare(domain.Trim(), dc, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static DirectoryEntry CreateDirectoryEntry(this LdapSettings ldapSettings)
        {
            return new DirectoryEntry(ldapSettings.LdapAuthenticationUrl
                                    , ldapSettings.BindUser
                                    , ldapSettings.BindPassword
                                    , ldapSettings.AuthenticationType);
        }
    }
}
