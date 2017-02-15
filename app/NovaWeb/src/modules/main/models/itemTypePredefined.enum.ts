export enum ItemTypePredefined {
    Project = -1,             // for client use only
    Collections = -2,         // for client use only
    BaselinesAndReviews = -3, // for client use only

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
