﻿namespace ServiceLibrary.Helpers
{
    public class ErrorCodes
    {
        // Compatible with InternalApi exceptions from Blueprint-current
        public const int ItemNotFound = 101;
        public const int IncorrectInputParameters = 103;
        public const int CannotPublish = 113;
        public const int CannotPublishOverValidationErrors = 121;
        public const int CannotDiscardOverDependencies = 122;
        public const int CannotSaveDueToReadOnly = 116;

        // Common error codes for blueprint and blueprint-current repositories
        public const int LockedByOtherUser = 111;
        public const int ExceedsLimit = 128;

        // Generic errors
        public const int BadRequest = 400;
        public const int Conflict = 409;

        // Forbidden error
        public const int Forbidden = 403;

        // Configuration errors
        public const int LdapIsDisabled = 1000;
        public const int FallbackIsDisabled = 1001;
        public const int WorkflowDisabled = 1002;
        public const int WorkflowLicenseUnavailable = 1003;
        public const int LicenseUnavailable = 1004;

        // Authentication errors
        public const int InvalidCredentials = 2000;
        public const int AccountIsLocked = 2001;
        public const int PasswordExpired = 2002;
        public const int EmptyCredentials = 2003;
        public const int FederatedAuthenticationException = 2004;

        // Resource errors
        public const int ResourceNotFound = 3000;

        // Password reset errors
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

        // Authorization errors
        public const int UnauthorizedAccess = 5000;

        // Bad request
        public const int OutOfRangeParameter = 6000;
        public const int InvalidWorkflowXml = 6001;
        public const int InvalidParameter = 6002;

        // Timeout
        public const int Timeout = 7000;
        public const int SqlTimeoutNumber = -2;
        public const int SqlErrorInFtsSyntax = 7630;

        // Jobs Parameter Validation Errors
        public const int PageNullOrNegative = 8000;
        public const int PageSizeNullOrOutOfRange = 8001;
        public const int JobNotCompleted = 8002;
        public const int ResultFileNotSupported = 8003;
        public const int QueueJobProcessesInvalid = 8004;
        public const int QueueJobProjectIdInvalid = 8005;
        public const int QueueJobProjectNameEmpty = 8006;
        public const int QueueJobEmptyRequest = 8007;

        // Artifact Retrieval Errors
        public const int ArtifactNotFound = 9001;
        public const int SubartifactNotFound = 9002;

        // Review Artifact Errors
        public const int ApprovalRequiredIsReadonlyForReview = 10001;
        public const int ApprovalRequiredArtifactNotInReview = 10002;
        public const int ReviewClosed = 10003;
        public const int ReviewActive = 10004;
        public const int ReviewStatusChanged = 10005;
        public const int NotAllArtifactsReviewed = 10006;
        public const int BaselineIsNotSealed = 10007;
        public const int ReviewExpired = 10008;
        public const int MeaningOfSignatureNotEnabled = 10009;
        public const int MeaningOfSignatureNotPossible = 10010;
        public const int BaselineIsAlreadyAttachedToReview = 10011;
        public const int LiveArtifactsReplacingWithBaseline = 10012;
        public const int MeaningOfSignatureNotChosen = 10013;
        public const int ReviewNeedsToMoveBackToDraftState = 10014;
        public const int LastApproverInActiveReview = 10015;
        public const int ReviewDraft = 10016;

        // User Review Errors
        public const int UserDisabled = 11001;
        public const int UserNotInReview = 11002;
        public const int ParticipantIsNotAnApprover = 11003;

        // Action Handler Service Errors
        public const int TenantInfoNotFound = 12000;
        public const int UnsupportedActionType = 12001;
        public const int MessageHeaderValueNotFound = 12002;
        public const int NServiceBusConnection = 12003;
        public const int BoundaryReached = 12004;
        public const int RabbitMqSend = 12005;
        public const int SqlServerSend = 12006;

        // Unexpected Errors
        public const int UnexpectedError = 13001;

        // Property Type errors
        public const int InvalidArtifactProperty = 14001;

        // Email Settings errors
        public const int OutgoingEmptyMailServer = 15001;
        public const int OutgoingPortOutOfRange = 15002;
        public const int IncomingEmptyMailServer = 15003;
        public const int IncomingPortOutOfRange = 15004;
        public const int EmptySmtpAdministratorUsername = 15005;
        public const int EmptySmtpAdministratorPassword = 15006;
        public const int UserHasNoEmail = 15007;
        public const int EmptyEmailUsername = 15008;
        public const int EmptyEmailPassword = 15009;
        public const int UnknownIncomingMailServerError = 15010;
        public const int IncomingMailServerInvalidHostname = 15011;
        public const int IncomingMailServerInvalidCredentials = 15012;
        public const int OutgoingMailError = 15013;
        public const int CannotEnableDiscussions = 15014;
        public const int EmptyEmailAddress = 15015;
        public const int InvalidEmailAddress = 15016;

        // Workflow
        public const int WorkflowInvalidPropertyChange = 16001;
        public const int WorkflowAlreadyExists = 16002;
        public const int GeneralErrorOfCreatingWorkflow = 16003;
        public const int WorkflowIsActive = 16004;
        public const int WorkflowProjectHasNoArtifactTypes = 16005;
        public const int WorkflowProjectHasNoLiveArtifactTypes = 16006;
        public const int WorkflowProjectHasNoLiveArtifactTypesAmongChecked = 16007;
        public const int WorkflowVersionsNotEqual = 16008;

        // Search
        public const int SearchEngineNotFound = 17001;
        public const int ElasticsearchConfigurationError = 17002;
        public const int ElasticsearchQueryError = 17003;

        // Collection
        public const int CollectionDoesNotExist = 18001;

    }
}
