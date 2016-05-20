﻿namespace ServiceLibrary.Helpers
{
    public class ErrorCodes
    {
        //Configuration errors
        public const int LdapIsDisabled = 1000;
        public const int FallbackIsDisabled = 1001;

        //Authentication errors
        public const int InvalidCredentials = 2000;
        public const int AccountIsLocked = 2001;
        public const int PasswordExpired = 2002;
        public const int EmptyCredentials = 2003;
        public const int FederatedAuthenticationException = 2004;

        //Resource errors
        public const int ResourceNotFound = 3000;

        //Password reset errors
        public const int EmptyPassword = 4000;
        public const int SamePassword = 4001;
        public const int TooSimplePassword = 4002;

        //Authorization errors
        public const int UnauthorizedAccess = 5000;
    }
}