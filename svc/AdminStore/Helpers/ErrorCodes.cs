namespace AdminStore.Helpers
{
    public class ErrorCodes
    {
        //Configuration errors
        public const int LdapIsDisabled = 1000;
        public const int FallbackIsDisabled = 1001;

        public const int InvalidCredentials = 2000;
        public const int AccountIsLocked = 2001;
        public const int PasswordExpired = 2002;

		public const int ViewerLicenseLimit = 3001;
		public const int CollaboratorLicenseLimit = 3002;
		public const int AuthorLicenseLimit = 3003;
    }
}