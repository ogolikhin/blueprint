
namespace Helper
{
    // This is the global error code being used for Blueprint/Services project, (interval API services)
    // This code will be updated based on the change from Bluerprint project 
    // Location of the source code: lib/ServiceLibraryHelpers/ErrorCodes.cs
    public static class ErrorCodes
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
        public const int ChangePasswordCooldownInEffect = 4003;
        public const int IncorrectSearchCriteria = 4004;

        //Authorization errors
        public const int UnauthorizedAccess = 5000;

        //Bad request
        public const int OutOfRangeParameter = 6000;
    }

    // This is the global error code being used for Blueprint-current/NoSilverlight project
    // This code will be updated based on the change from Bluerprint-current project 
    // Location of the source code: Server/BluePrintSys.RC.Businesss.Internal/Models/InternalApiErrorCodes.cs
    public static class InternalApiErrorCodes
    {
        public const int ItemNotFound = 101;
        public const int ProjectNotFound = 102;
        public const int IncorrectInputParameters = 103;
        public const int ConcurrentSessions = 104;
        public const int ImpactAnalysisInvalidLevel = 105;
        public const int ImpactAnalysisInvalidSourceId = 106;
        /// <summary>
        /// AcceptType is not supported
        /// </summary>
        public const int NotAcceptable = 107;
        public const int Forbidden = 108;
        public const int ItemTypeNotFound = 109;
        public const int UserStoryArtifactTypeNotFound = 110;
        public const int LockedByOtherUser = 111;
        public const int ArtifactNotPublished = 112;

        public const int CannotPublish = 113;

        public const int ValidationFailed = 114;

        public const int UnexpectedLockException = 115;

        public const int CannotSaveDueToReuseReadOnly = 116;

        public const int ClientDataOutOfDate = 117;

        //Configuration has not been defined for Service hook (example SMB)
        public const int ConfigurationNotSetForServiceHook = 118;

        //User story post processing failed for Service hook (example SMB)
        public const int PostProcessingForServiceHookFailed = 119;

        public const int CannotPublishOverDependencies = 120;
        public const int CannotPublishOverValidationErrors = 121;
        public const int CannotDiscardOverDependencies = 122;

        public const int CannotSaveOverDependencies = 123;

        public const int CycleRelationship = 124;

        public const int CannotSaveConflictWithParent = 125;

        public const int Ok = 200;

        public const int NotFound = 404;

        public const int UnhandledException = 500;
    }
}
