namespace Model.Common.Constants
{
    public static class ApiBusinessErrorCodes
    {
        public const int ProjectNotFound = 101;
        public const int ArtifactTypeInProjectNotFound = 102;
        public const int InvalidCredentials = 103;
        public const int InvalidCastForProperty = 104;
        public const int ArtifactInProjectNotFound = 105;
        public const int BadRequest = 106;
        public const int InvalidTrace = 107;
        public const int InvalidFilterQuery = 108;
        public const int GroupNotFound = 109;
        public const int UserNotFound = 110;
        public const int AttachmentNotFound = 111;
        public const int InvalidAttachment = 112;
        public const int InvalidPaginationOffsetOrLimit = 113;
        public const int PaginationOffsetOutOfRange = 114;
        public const int RequiredArtifactHasNotBeenPublished = 115;
        public const int NewerArtifactRevisionExists = 116;
        public const int InvalidArtifactProperty = 117;
        public const int LockedByOtherUser = 118;
        public const int InvalidArtifactPropertyType = 119;
        public const int ArtifactHasNothingToPublish = 120;
        public const int ConcurrencyLockCannotBeAcquire = 121;
        public const int BadRequestDueToIncorrectQueryParameterValue = 122;
        public const int NoTracePremissionOnArtifact = 123;
        public const int NoDeletePremissionOnArtifact = 124;
        public const int NoEditPremissionOnArtifact = 125;
        public const int ImageIsNotSupportedForSpecifiedArtifact = 126;
        public const int ImageRenderingFailed = 127;
        public const int RequestEntityTooLarge = 128;
        public const int LengthRequired = 129;
        public const int UnsupportedMediaType = 130;
        public const int SubArtifactNotFoundForArtifact = 131;
        public const int AttachmentCouldNotBeUploaded = 132;
        public const int CannotUploadMoreThanOneAttachmentToDocument = 133;
        public const int NoCommentPremissionOnArtifact = 134;
        public const int ThreadNotExistsForRevision = 135;
        public const int CannotReplyOnClosedThread = 136;

        public const int ReplyDoesNotBelongToComment = 137;
        public const int CannotUpdateAlienReply = 138;
        public const int UserNotAuthorOfComment = 139;
        public const int CannotRateClosedThread = 140;
        public const int TooManyConcurrentThreads = 141;
        public const int TargetNotFound = 142;
        public const int JobNotFoundInAlmTarget = 143;
        public const int ChangeSummaryNotFound = 144;
        public const int ChangeSummaryIsNotCompleted = 145;
        public const int UserCannotAccessSpecifiedChangeSummary = 146;
        public const int AlmExportAlreadyInProgressToTarget = 147;
        public const int InitialPushIsRequiredBeforeDeltaPush = 148;
        public const int UnsupportedAlmTargetType = 149;
        public const int ChangeSummaryBelongsToOtherProject = 150;
        public const int ChangeSummaryBelongsToOtherTarget = 151;

        public const int AttachmentsAreReadOnly = 156;
        public const int RelationshipsAreReadOnly = 152;
        public const int SubArtifactsAreReadOnly = 153;
        public const int RelationshipsReadOnlyExceptionIgnored = 160;
        public const int AttachmentsUpdateAreReadOnlyIgnored = 161;
        public const int SubArtifactsUpdateAreReadOnlyIgnored = 162;
        public const int AttachmentsCreateAreReadOnlyIgnored = 163;
        public const int SubArtifactsCreateAreReadOnlyIgnored = 164;

        public const int ReviewInProjectNotFound = 154;
        public const int CollectionInProjectNotFound = 155;

        public const int PropertyTypeInProjectNotFound = 165;
        public const int Forbidden = 166;

        public const int ServerError = 500;
    }
}
