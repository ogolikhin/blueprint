export enum ILayoutPanel {
    Left,
    Right
}

export enum PrimitiveType {
    Text = 0,
    Number = 1,
    Date = 2,
    User = 3,
    Choice = 4,
    Image = 5
}


export enum ItemTypePredefined {
    None = 0,
    BaselineArtifactGroup = 256,
    CollectionArtifactGroup = 512,
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

export enum PropertyTypePredefined {
    none = 0,                                               //None = 0,
    systemgroup = 4096,                                     //SystemGroup = 4096,
    id = 4097,                                              //Id = 4097,
    name = 4098,                                            //Name = 4098,
    description = 4099,                                     //Description = 4099,
    usecaselevel = 4100,                                    //UseCaseLevel = 4100,
    readonly = 4101,                                        //ReadOnly = 4101,
    itemlabel = 4102,                                       //ItemLabel = 4102,
    rowlabel = 4103,                                        //RowLabel = 4103,
    columnlabel = 4104,                                     //ColumnLabel = 4104,
    dataobjecttype = 4105,                                  //DataObjectType = 4105,
    extensiontype = 4106,                                   //ExtensionType = 4106,
    condition = 4107,                                       //Condition = 4107,
    bpobjecttype = 4108,                                    //BPObjectType = 4108,
    widgettype = 4109,                                      //WidgetType = 4109,
    returntostepname = 4110,                                //ReturnToStepName = 4110,
    rawdata = 4111,                                         //RawData = 4111,
    threadstatus = 4112,                                    //ThreadStatus = 4112,
    approvalstatus = 4113,                                  //ApprovalStatus = 4113,
    clienttype = 4114,                                      //ClientType = 4114,
    label = 4115,                                           //Label = 4115,
    sharedviewpreferences = 4116,                           //SharedViewPreferences = 4116,
    valuetype = 4117,                                       //ValueType = 4117,
    issealedpublished = 4118,                               //IsSealedPublished = 4118,
    almintegrationsettings = 4119,                          //ALMIntegrationSettings = 4119,
    almexportinfo = 4120,                                   //ALMExportInfo = 4120,
    almsecurity = 4121,                                     //ALMSecurity = 4121,
    stepof = 4122,                                          //StepOf = 4122,
    dataoperationset = 4123,                                //DataOperationSet = 4123,
    createdby = 4124,                                       //CreatedBy = 4124,
    createdon = 4125,                                       //CreatedOn = 4125,
    lasteditedby = 4126,                                    //LastEditedBy = 4126,
    lasteditedon = 4127,                                    //LastEditedOn = 4127,
    visualizationgroup = 8192,                              //VisualizationGroup = 8192,
    x = 8193,                                               //X = 8193,
    y = 8194,                                               //Y = 8194,
    width = 8195,                                           //Width = 8195,
    height = 8196,                                          //Height = 8196,
    connectortype = 8197,                                   //ConnectorType = 8197,
    truncatetext = 8198,                                    //TruncateText = 8198,
    backgroundcolor = 8199,                                 //BackgroundColor = 8199,
    bordercolor = 8200,                                     //BorderColor = 8200,
    borderwidth = 8201,                                     //BorderWidth = 8201,
    image = 8202,                                           //Image = 8202,
    orientation = 8203,                                     //Orientation = 8203,
    clientrawdata = 8204,                                   //ClientRawData = 8204,
    theme = 8205,                                           //Theme = 8205,
    thumbnail = 8206,                                       //Thumbnail = 8206,
    customgroup = 16384,                                    //CustomGroup = 16384,
    groupmask = 61440                                       //GroupMask = 61440
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



















































