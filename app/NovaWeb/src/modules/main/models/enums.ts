export enum ILayoutPanel {
    Left,
    Right
}

export enum PrimitiveType {
    ActorInheritance = -1, // for client use only

    DocumentFile = 4129, // for client use only

    Text = 0,
    Number = 1,
    Date = 2,
    User = 3,
    Choice = 4,
    Image = 5
}

export enum PropertyLookupEnum {
    None = 0,
    System = 1,
    Custom = 2,
    Special = 3,
}

export enum ItemTypePredefined {
    Project = -1,       // for client use only 
    Collections = -2,   // for client use only 

    None = 0,
    BaselineArtifactGroup = 256,
    CollectionArtifactGroup = 512,
    PrimitiveArtifactGroup = 4096,
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
    BaselineFolder = 4353,
    ArtifactBaseline = 4354,
    ArtifactReviewPackage = 4355,
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
    DataObject = 32769,
    GroupMask = 61440
}

export namespace ItemTypePredefined {
    export function canContainSubartifacts(predefinedType: ItemTypePredefined): boolean {
        return Boolean(this.getSubArtifactsContainerNodeTitle(predefinedType));
    }

    export function getSubArtifactsContainerNodeTitle(predefinedType: ItemTypePredefined): string {
        switch (predefinedType) {
            case ItemTypePredefined.Glossary:
                return "Terms"; //TODO localize
            case ItemTypePredefined.BusinessProcess:
            case ItemTypePredefined.UIMockup:
            case ItemTypePredefined.GenericDiagram:
            case ItemTypePredefined.Storyboard:
            case ItemTypePredefined.DomainDiagram:
            case ItemTypePredefined.UseCaseDiagram:
                return "Shapes"; //TODO localize
            case ItemTypePredefined.UseCase:
                return "Steps"; //TODO localize
            case ItemTypePredefined.Process:
                return "Tasks and Decisions"; //TODO localize
            default:
                return undefined;
        }
    }
}

export enum PropertyTypePredefined {
    DocumentFile = 4129,

    ItemTypeId = -1, // for client use only 

    None = 0,
    SystemGroup = 4096,
    Id = 4097,
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
    ActorInheritance = 4128,
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
    CustomGroup = 16384,
    GroupMask = 61440
}

export enum TraceType {
    /// <summary>
    /// Parent
    /// </summary>
    Parent = 0,
    /// <summary>
    /// Child
    /// </summary>
    Child = 1,
    /// <summary>
    /// Manual
    /// </summary>
    Manual = 2,
    /// <summary>
    /// All other traces including inherits from etc
    /// </summary>
    Other = 4,
    /// <summary>
    /// Reuse
    /// </summary>
    Reuse = 8
}


export enum RolePermissions {
    // No privileges
    None = 0,  //0

    // Allows the viewing of an artifact
    Read = 1 << 0, // = 1

    // Allows the editing of an artifact. This includes deleting & adding children.
    Edit = 1 << 1, // = 2

    // Allows deleting an artifact.
    Delete = 1 << 2, // = 4

    // Allow tracing from/To an artifact project.
    Trace = 1 << 3, // = 8

    // Allow the user to comment on an artifact.
    Comment = 1 << 4, // 16

    // Allows a user to steal a lock on artifacts.
    StealLock = 1 << 5, //= 32

    // Allows a user to report on the project.
    CanReport = 1 << 6, // = 64

    // Allows a user to share an artifact.
    Share = 1 << 7, // = 128

    // Allow reuse traces from/To an artifact project.
    Reuse = 1 << 8, // = 256

    // Allows a user to perform Excel Update.
    ExcelUpdate = 1 << 9, // = 512

    // Allow the user to delete someone else's comment on an artifact.
    DeleteAnyComment = 1 << 10, // = 1024

    // Allow the user to create/edit/save rapid review
    CreateRapidReview = 1 << 11 // = 2048
}


export enum ReuseSettings {
    None = 0,
    Name = 1 << 0, //1
    Description =  1 << 1, //2
    ActorImage =  1 << 2, //4 
    BaseActor = 1 << 3, //6 
    DocumentFile = 1 << 4, //16
    DiagramHeight = 1 << 5, //32
    DiagramWidth = 1 << 6, //64
    UseCaseLevel = 1 << 7, //128
    UIMockupTheme = 1 << 8, //256
    UseCaseDiagramShowConditions = 1 << 9, //512
    Attachments = 1 << 10, //1024
    DocumentReferences = 1 << 11, //2048
    Relationships = 1 << 12, //4096
    Subartifacts = 1 << 13, //8193
}



export enum TraceDirection {
    /// <summary>
    /// Child link always has direction To
    /// </summary>
    To, //Child link always has direction To
    /// <summary>
    /// Parent link always has direction From
    /// </summary>
    From, //Parent link always has direction From
    Bidirectional
}

export enum LicenseTypeEnum {
    None = 0,
    Viewer = 1,
    Collaborator = 2,
    Author = 3
}

export enum LockedByEnum {
    None = 0,
    CurrentUser = 1,
    OtherUser = 2 
}

export enum LockResultEnum {
    Success,
    AlreadyLocked,
    DoesNotExist,
    AccessDenied,
    Failure
}











































