namespace Model.Common.Constants
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.CrossCutting.Portable/BusinessLayerErrorCodes.cs
    public static class BusinessLayerErrorCodes
    {
        public const int DBConnectionError = 101;
        public const int DBNotUpdated = 102;
        public const int DBDeadlock = 103;
        public const int DBForeignKeyViolation = 104;

        public const int Ok = 200;
        public const int Created = 201;

        public const int ArtifactHasNothingToPublish = 1001;
        public const int DoesNotExistForRevision = 1002;
        public const int ProjectDoesNotExistForRevision = 1003;
        public const int ArtifactMetaInUse = 1004;
        public const int CannotMove = 1005;

        // very generic error indicating that is applied to anything that might have a name
        public const int NotUniqueName = 1006;

        #region review package error codes

        public const int NotInReviewPackage = 1007;
        public const int NotAssignedForReview = 1008;
        public const int ReviewClosed = 1016;

        #endregion

        public const int AccessDenied = 1009;
        public const int SessionExpired = 1011;
        public const int ConcurrentSessions = 1012;

        public const int ConcurrencyException = 1013;
        public const int LoginDoesNotExist = 1014;
        public const int ProjectDeleted = 1015;

        public const int InvalidOfficeTemplate = 1017;

        public const int FileNotUploaded = 1018;
        public const int FileNotFound = 1019;
        public const int FileSizeChanged = 1010;

        #region SMTP Email client error codes

        public const int SmtpMissingHostName = 1020;
        public const int SmtpInvalidPort = 1021;
        public const int SmtpInvalidHostName = 1022;
        public const int SmtpConnectionRefusedOnPort = 1023;
        public const int SmtpSslRequired = 1024;
        public const int SmtpInvalidCredentials = 1025;
        public const int SmtpMissingRecipient = 1026;
        public const int SmtpMissingSender = 1027;
        public const int SmtpError = 1028;

        #endregion

        public const int ServerIsNotAvailable = 1029;
        public const int LdapIsDisabled = 1030;
        public const int MaxLicenseLimitReached = 1031;
        public const int NoLicensePresent = 1032;
        public const int LicenseFeatureNotSupported = 1033;
        public const int LicenseIsInvalid = 1034;

        public const int InvalidOfficeTemplateParameters = 1035;

        public const int AlmArtifactNotMapped = 1036;
        public const int AlmExportAlreadyInProgressToTarget = 1038;

        public const int LockedByOtherUser = 1040;
        public const int AlmExportKeyNotAvailable = 1041;
        public const int AlmExportQCNotInstalled = 1042;
        public const int AlmExportUpdateTarget = 1043;

        public const int ContentFailedToSave = 1044;

        public const int DoesNotBelongToProject = 1045;

        public const int InvalidTraceConditionQuery = 1046;

        public const int MultipleParentTracesDetected = 1047;

        public const int DuplicateFilterKey = 1048;

        public const int BadArtifactQueryFilter = 1049;

        public const int LinkAlreadyExists = 1050;

        public const int PaginationOffsetOutOfRange = 1051;

        public const int FederatedAuthenticationDisabled = 1052;

        public const int FederatedAuthenticationUserNotDefined = 1053;

        public const int NewerArtifactRevisionExists = 1054;

        public const int RequiredArtifactHasNotBeenPublished = 1055;

        public const int NoPermissionOnArtifact = 1056;

        public const int InvalidArtifactProperty = 1057;

        public const int InvalidArtifactPropertyType = 1058;

        public const int CannotUpdateReadonlyProperty = 1059;

        public const int SpecifiedVersionExceedsTheLatestOne = 1060;

        public const int ConcurrencyLockCannotBeAcquire = 1061;

        public const int ArtifactHasNothingToDiscard = 1062;

        public const int RequiredArtifactHasNotBeenDiscarded = 1063;

        public const int AlmTfsAreaPathCouldNotBeRetrieved = 1064;

        public const int UserDoesNotHaveSufficientPrivilegesToCompleteThisAction = 1065;

        public const int CannotDeleteWhileReferencedByOtherItems = 1066;

        public const int ExceededExcelMaxCellLength = 1067;

        public const int InconsistentData = 1068;

        public const int IncorrectFolderStructure = 1070;

        public const int ArtifactNotFoundForUser = 1071;

        public const int TraceNotManual = 1072;

        public const int TraceNotFound = 1073;

        public const int NoTracePermissionOnArtifact = 1074;

        public const int DeleteTraceSystemError = 1075;

        public const int DeleteTraceSuccess = 1076;

        public const int AddTraceSystemError = 1077;

        public const int AddTraceSuccess = 1078;

        public const int AddTraceBadRequest = 1079;

        public const int TracedArtifactNotFound = 1080;

        public const int AddTraceFailed = 1081;

        public const int TraceCannotBeManaged = 1082;

        public const int HasChildren = 1083;

        public const int MissingArguments = 1084;

        public const int TracedSubArtifactDoesNotBelongToArtifact = 1085;

        public const int TraceToSelf = 1086;

        public const int NoEditPermissionOnArtifact = 1087;

        public const int AttachmentNotFound = 1088;

        public const int ImageIsNotSupportedForSpecifiedArtifact = 1089;

        public const int ImageRenderingFailed = 1090;

        public const int NewParentNotFoundForUser = 1091;

        public const int NewParentNotInProject = 1092;

        public const int ChangeParentSuccess = 1093;

        public const int UpdateParentSystemError = 1094;

        public const int NoEditPermissionOnNewParent = 1095;

        public const int NewParentIsCurrentOne = 1096;

        public const int AttachmentNotProvidedForUpload = 1097;

        public const int AttachmentCannotBeUploaded = 1098;

        public const int NewParentIsDescendantOrItself = 1099;

        public const int ArtifactNotFound = 1100;

        public const int NoCommentPermissionOnArtifact = 1101;

        public const int ThreadDoesNotExistForRevision = 1102;

        public const int CannotReplyOnClosedThread = 1103;

        public const int CommentDoesNotBelongToThread = 1104;

        public const int CannotUpdateAlienComment = 1105;

        public const int UserNotAuthorOfThread = 1106;

        public const int AlmExportNoSelectionMadeByUser = 1107;

        public const int CannotRateClosedThread = 1108;

        public const int HpAlmRestAuthenticationFailed = 1109;

        public const int HpAlmRestConnectionFailed = 1110;

        public const int PropertyProvidedNameNotUnique = 1111;

        public const int ItemTypeProvidedNameOrPrefixNotUnique = 1112;

        public const int InstancePropertyTypeNotFound = 1113;

        public const int CreateInstancePropertyConflicts = 1114;

        public const int PropertyTypeMismatch = 1115;

        public const int PropertyPromotionFailed = 1116;

        public const int QcLiteWebConnectionFailed = 1117;

        public const int NoStandardProperties = 1118;

        public const int ClientOutSync = 1119;

        public const int PropertyAlreadyPromoted = 1120;

        public const int CustomPropertyTypeNotFound = 1121;

        public const int NotUniquePrefix = 1122;

        public const int ProjectImportFailedDueToAlreadyRunningImport = 1123;

        public const int ProjectImportTaskNotFound = 1124;

        public const int ProjectExportFailed = 1125;

        public const int ProjectImportFailed = 1126;

        public const int ArtifactTypeNotFound = 1127;

        public const int CannotReviewArtifactAsItIsNotRequested = 1128;

        public const int InstanceTypeConflict = 1129;

        public const int NoConflicts = 1130;

        public const int TargetDoesNotExistForRevision = 1131;

        public const int PropertyValueUpdateFailed = 1132;

        public const int AddAttachmentFailed = 1133;

        public const int ChangeSummaryDoesNotExist = 1134;

        public const int ChangeSummaryIsNotCompleted = 1135;

        public const int InitialPushIsRequiredBeforeDeltaPush = 1136;

        public const int OnlyQcOrHpAlmTargetsAreSupported = 1137;

        public const int ChangeSummaryBelongsToOtherProject = 1138;

        public const int ChangeSummaryBelongsToOtherTarget = 1139;

        public const int ConflictResolutionRuleIsNotSpecifiedForDeltaPush = 1140;

        public const int CantChangeArtifactTypeIfThereAreReuseTraces = 1141;

        public const int CreateReuseTraceError = 1142;

        public const int NotAllArtifactsWereReviewed = 1143;

        public const int ArtifactHasIncomingReuseTrace = 1144;



        #region IMAP and POP Email server error codes

        public const int IncomingMailServer_MissingHostName = 10410;
        public const int IncomingMailServer_InvalidPort = 1041;
        public const int IncomingMailServer_InvalidHostName = 1042;
        public const int IncomingMailServer_ConnectionRefusedOnPort = 1043;
        public const int IncomingMailServer_SslRequired = 1044;
        public const int IncomingMailServer_InvalidCredentials = 1045;
        public const int IncomingMailServer_Error = 1046;

        #endregion

        #region Reuse

        public const int ReuseTemplateSettings_CannotDisableReadOnlySettingsOverride = 1147;

        public const int ReuseArtifactTypeNotFound = 1148;

        public const int ReuseSpecifiedVersionExceedsTheLatestOne = 1149;

        public const int Reuse_SelectiveReadonly = 1150;

        public const int ReuseReconcileNothingToReconcile = 1151;

        public const int ReuseReconcileCannotLockArtifacts = 1152;

        public const int ReuseReconcileNoEditPermissionOnArtifact = 1153;

        public const int ReuseReconcileArtifactNotFound = 1154;

        public const int ReuseReconcileDifferentVersions = 1155;

        public const int ReuseReconcileArtifactLockedOrHasDraft = 1156;

        public const int ReuseReconcililingSameArtifact = 1157;

        public const int ReconcileUnknownError = 1158;

        public const int AttachmentsAreReadOnly = 1162;

        public const int RelationshipsAreReadOnly = 1159;

        public const int SubArtifactsAreReadOnly = 1160;

        public const int ReuseCannotUpdateReadonlyProperty = 1161;

        public const int ReuseArtifactLimitExceeded = 1163;

        public const int RelationshipsReadOnlyExceptionIgnored = 1166;

        public const int SubArtifactReadOnlyExceptionIgnored = 1167;

        public const int ReuseCannotUpdateReadonlyPropertyIgnored = 1168;

        public const int AttachmentsAreReadOnlyIgnored = 1169;

        #endregion

        public const int NoArtifactProvided = 1165;

        public const int PhoneHomeDataWasNotCollected = 1170;

        public const int InvalidUser = 1171;

        public const int IncorrectSearchCriteria = 1172;

        public const int PhoneHomeEmailSettingsAreNotSet = 1173;

        public const int InvalidLdapUrl = 1174;

        public const int SystemError = 1175;

        public const int WinUserWithThisEmailAlreadyExists = 1176;

        public const int PasswordExpired = 1177;

        public const int ReviewerEmptyMoSAssignment = 1178;

        public const int RoleMoSAssignmentChanged = 1179;

        public const int RoleMosReviewNotProvidedWithSelectedMos = 1180;

        public const int RoleOrMosSignatureNotAvailable = 1181;

        public const int ReviewNoLongerAllowsESig = 1182;

        public const int HtmlMigrationNotPerformed = 1183;

        public const int InstanceFolderNotFound = 1184;

        public const int SubArtifactNotFound = 1185;

        public const int ChoicePropertyTypeNotFound = 1186;

        public const int CannotModifyStandardPropertyInProject = 1187;

        public const int RequiredChoicePropertyMustHaveNotEmptyValidValues = 1188;

        public const int CommentNotFound = 1189;

        public const int CannotCreateArtifactInvalidParent = 1190;

        public const int InvalidPersonaType = 1191;

        //Add, Update, Delete user
        public const int UserValidationFailed = 1192;

        public const int UserAddToGroupFailed = 1193;

        public const int UserAddInstanceAdminRoleFailed = 1194;

        // last used = 1194
    }
}
