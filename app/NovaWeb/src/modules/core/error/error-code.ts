export enum ErrorCode {
    ItemNotFound = 101,
    ProjectNotFound = 102,
    IncorrectInputParameters = 103,
    ConcurrentSessions = 104,
    ImpactAnalysisInvalidLevel = 105,
    ImpactAnalysisInvalidSourceId = 106,
    // AcceptType is not supported
    NotAcceptable = 107,
    Forbidden = 108,
    ItemTypeNotFound = 109,
    UserStoryArtifactTypeNotFound = 110,
    LockedByOtherUser = 111,
    ArtifactNotPublished = 112,
    CannotPublish = 113,
    ValidationFailed = 114,
    UnexpectedLockException = 115,
    CannotSaveDueToReuseReadOnly = 116,
    ClientDataOutOfDate = 117,
    //Configuration has not been defined for Service hook (example SMB)
    ConfigurationNotSetForServiceHook = 118,
    //User story post processing failed for Service hook (example SMB)
    PostProcessingForServiceHookFailed = 119,
    CannotPublishOverDependencies = 120,
    CannotPublishOverValidationErrors = 121,
    CannotDiscardOverDependencies = 122,
    CannotSaveOverDependencies = 123,
    CycleRelationship = 124,
    CannotSaveConflictWithParent = 125,
    Ok = 200,
    NotFound = 404,
    UnhandledException = 500
}