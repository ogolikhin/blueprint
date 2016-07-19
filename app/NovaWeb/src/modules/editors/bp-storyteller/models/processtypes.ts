

export const enum ProcessType {
    None = 0,
    BusinessProcess = 1,
    UserToSystemProcess = 2,
    SystemToSystemProcess = 3
}

export const enum ProcessShapeType {
    None = 0,
    Start = 1,
    UserTask = 2,
    End = 3,
    SystemTask = 4,
    PreconditionSystemTask = 5,
    UserDecision = 6,
    SystemDecision = 7
}

export const enum MessageLevel {
    None = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

export const enum ItemTypePredefined {
    GroupMask = 61440,
    None = 0,
    PrimitiveArtifactGroup = 4096,
    Project = 4097,
    Baseline = 4098,
    Glossary = 4099,
    TextualRequirement = 4101,
    PrimitiveFolder = 4102,
    BusinessProcess = 4103,
    Actor = 4104,
    UseCase = 4105,
    DataElement = 4106,
    UIMockup = 4107,
    GenericDiagram = 4108,
    Document = 4110,
    Storyboard = 4111,
    DomainDiagram = 4112,
    UseCaseDiagram = 4113,
    Process = 4114,
    BaselineArtifactGroup = 256,
    BaselineFolder = 4353,
    ArtifactBaseline = 4354,
    ArtifactReviewPackage = 4355,
    CollectionArtifactGroup = 512,
    CollectionFolder = 4609,
    ArtifactCollection = 4610,
    SubArtifactGroup = 8192,
    GDConnector = 8193,
    GDShape = 8194,
    BPConnector = 8195,
    PreCondition = 8196,
    PostCondition = 8197,
    Flow = 8198,
    Step = 8199,
    Extension = 8200,
    Bookmark = 8213,
    BaselinedArtifactSubscribe = 8216,
    Term = 8217,
    Content = 8218,
    DDConnector = 8219,
    DDShape = 8220,
    BPShape = 8221,
    SBConnector = 8222,
    SBShape = 8223,
    UIConnector = 8224,
    UIShape = 8225,
    UCDConnector = 8226,
    UCDShape = 8227,
    PROShape = 8228,
    CustomArtifactGroup = 16384,
    ObsoleteArtifactGroup = 32768,
    DataObject = 32769
}

export const enum PropertyTypePredefined {
    GroupMask = 61440,
    None = 0,
    SystemGroup = 4096,
    ID = 4097,
    Name = 4098,
    Description = 4099,
    UseCaseLevel = 4100,
    ReadOnly = 4101,
    ItemLabel = 4102,
    RowLabel = 4103,
    ColumnLabel = 4104,
    DataObjectType = 4105,
    ExtensionType = 4106,
    Condition = 4107,
    BPObjectType = 4108,
    WidgetType = 4109,
    ReturnToStepName = 4110,
    RawData = 4111,
    ThreadStatus = 4112,
    ApprovalStatus = 4113,
    ClientType = 4114,
    Label = 4115,
    SharedViewPreferences = 4116,
    ValueType = 4117,
    IsSealedPublished = 4118,
    ALMIntegrationSettings = 4119,
    ALMExportInfo = 4120,
    ALMSecurity = 4121,
    StepOf = 4122,
    DataOperationSet = 4123,
    CreatedBy = 4124,
    CreatedOn = 4125,
    LastEditedBy = 4126,
    LastEditedOn = 4127,
    VisualizationGroup = 8192,
    X = 8193,
    Y = 8194,
    Width = 8195,
    Height = 8196,
    ConnectorType = 8197,
    TruncateText = 8198,
    BackgroundColor = 8199,
    BorderColor = 8200,
    BorderWidth = 8201,
    Image = 8202,
    Orientation = 8203,
    ClientRawData = 8204,
    Theme = 8205,
    Thumbnail = 8206,
    CustomGroup = 16384
}

export const enum PropertyType {
    PlainText = 0,
    RichText = 1,
    Number = 2,
    Date = 3,
    Choice = 4,
    User = 5
}

export const enum LockResult {
    Success = 0,
    AlreadyLocked = 1,
    DoesNotExist = 2,
    AccessDenied = 3,
    Failure = 4
}

export interface IVersionInfo {
    artifactId: number;
    utcLockedDateTime: Date;
    lockOwnerLogin: string;
    projectId: number;
    versionId: number;
    revisionId: number;
    baselineId: number;
    isVersionInformationProvided: boolean;
    isHeadOrSavedDraftVersion: boolean;
}

export interface IHashMapOfPropertyValues {
    [name: string]: IPropertyValueInformation
}

export interface IProcess {
    id: number;
    name: string;
    typePrefix: string;
    projectId: number;
    itemTypeId: number;
    baseItemTypePredefined: ItemTypePredefined;
    shapes: IProcessShape[];
    links: IProcessLink[];
    decisionBranchDestinationLinks: IProcessLink[];
    propertyValues: IHashMapOfPropertyValues;
    status: IItemStatus;
    requestedVersionInfo: IVersionInfo;
}

export interface IProcessShape {
    id: number;
    name: string;
    projectId: number;
    typePrefix: string;
    parentId: number;
    baseItemTypePredefined: ItemTypePredefined;
    propertyValues: IHashMapOfPropertyValues;
    associatedArtifact: IArtifactReference;
}

export interface IPropertyValueInformation {
    propertyName: string;
    typePredefined: PropertyTypePredefined;
    typeId: number;
    value: any;
}

export interface IArtifactReference {
    id: number;
    projectId: number;
    name: string;
    typePrefix: string;
    projectName: string;
    baseItemTypePredefined: ItemTypePredefined;
    link: string;
}

export interface IProcessLink {
    sourceId: number;
    destinationId: number;
    orderindex: number;
    label: string;
}

export interface IItemStatus {
    userId: number;
    lockOwnerId: number;
    revisionId: number;
    isDeleted: boolean;
    isLocked: boolean;
    isLockedByMe: boolean;
    isUnpublished: boolean;
    hasEverBeenPublished: boolean;
    hasReadOnlyReuse: boolean;
    hasReuse: boolean;
    isReadOnly: boolean;
    versionId: number;
}

export interface IUserTaskShape extends ITaskShape {
    flags: ITaskFlags;
}

export interface ITaskShape extends IProcessShape {
}

export interface ITaskFlags {
    hasComments: boolean;
    hasTraces: boolean;
}

export interface ISystemTaskShape extends ITaskShape {
    flags: ITaskFlags;
}

export interface IArtifactReferenceLink {
    sourceId: number;
    destinationId: number;
    orderindex: number;
    associatedReferenceArtifactId: number;
}

export interface IArtifactSearchResultItem {
    id: number;
    name: string;
    typePrefix: string;
    projectName: string;
}

export interface IUserStory extends IArtifact {
    processTaskId: number;
    isNew: boolean;
}

export interface IArtifact {
    id: number;
    name: string;
    projectId: number;
    typeId: number;
    typePrefix: string;
    typePredefined: ItemTypePredefined;
    systemProperties: IProperty[];
    customProperties: IProperty[];
}

export interface IProperty {
    name: string;
    value: any;
    propertyTypeId: number;
    propertyType: PropertyType;
}

export interface IFileResult {
    guid: string;
    uriToFile: string;
}

export interface IOperationMessageResult {
    level: MessageLevel;
    propertyTypeId: number;
    itemId: number;
    code: number;
    message: string;
}

export interface IProcessUpdateResult {
    messages: IOperationMessageResult[];
    result: IProcess;
}

export interface ILockResultInfo {
    result: LockResult;
    info: IVersionInfo;
}