namespace ServiceLibrary.Helpers
{
    public class ErrorCodes
    {
        //Compatible with InternalApi exceptions from Blueprint-current
        public const int ItemNotFound = 101;
        public const int IncorrectInputParameters = 103;
        public const int CannotPublish = 113;
        public const int CannotPublishOverValidationErrors = 121;
        public const int CannotDiscardOverDependencies = 122;

        // Common error codes for blueprint and blueprint-current repositories
        public const int LockedByOtherUser = 111;

        //Generic errors
        public const int BadRequest = 400;
        public const int Conflict = 409;

        //Forbidden error
        public const int Forbidden = 403;

        //Configuration errors
        public const int LdapIsDisabled = 1000;
        public const int FallbackIsDisabled = 1001;
        public const int WorkflowDisabled = 1002;

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
        public const int PasswordSameAsLogin = 4005;
        public const int PasswordSameAsDisplayName = 4006;
        public const int PasswordAlreadyUsedPreviously = 4007;
        public const int PasswordResetTokenNotFound = 4008;
        public const int PasswordResetTokenNotLatest = 4009;
        public const int PasswordResetTokenExpired = 4010;
        public const int PasswordResetUserNotFound = 4011;
        public const int PasswordResetEmptyToken = 4012;
        public const int PasswordResetUserDisabled = 4013;
        public const int PasswordDecodingError = 4014;

        //Authorization errors
        public const int UnauthorizedAccess = 5000;

        //Bad request
        public const int OutOfRangeParameter = 6000;
        public const int InvalidWorkflowXml = 6001;

        //Timeout
        public const int Timeout = 7000;
        public const int SqlTimeoutNumber = -2;
        public const int SqlErrorInFtsSyntax = 7630;

        //Jobs Parameter Validation Errors
        public const int PageNullOrNegative = 8000;
        public const int PageSizeNullOrOutOfRange= 8001;
        public const int JobNotCompleted = 8002;
        public const int ResultFileNotSupported = 8003;
        public const int QueueJobProcessesInvalid = 8004;
        public const int QueueJobProjectIdInvalid = 8005;
        public const int QueueJobProjectNameEmpty = 8006;
        public const int QueueJobEmptyRequest = 8007;

        //Artifact Retrieval Errors
        public const int ArtifactNotFound = 9001;
        public const int SubartifactNotFound = 9002;

        //Review Artifact Errors
        public const int ApprovalRequiredIsReadonlyForReview = 10001;
        public const int ApprovalRequiredArtifactNotInReview = 10002;
        public const int ReviewClosed = 10003;
        public const int ReviewActive = 10004;

        //User Review Errors
        public const int UserDisabled = 11001;
        public const int UserNotInReview = 11002;
        
        //Action Handler Service Errors
        public const int TenantInfoNotFound = 12000;
        public const int UnsupportedActionType = 12001;
        public const int MessageHeaderValueNotFound = 12002;
    }
}
