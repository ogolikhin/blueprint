using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Helpers
{
    public class ErrorCodes
    {
        public const int ServerIsNotAvailable = 1029;
        public const int LdapIsDisabled = 1030;
        public const int MaxLicenseLimitReached = 1031;
        public const int NoLicensePresent = 1032;
        public const int LicenseFeatureNotSupported = 1033;
        public const int LicenseIsInvalid = 1034;

        public const int PasswordExpired = 1177;
    }
}