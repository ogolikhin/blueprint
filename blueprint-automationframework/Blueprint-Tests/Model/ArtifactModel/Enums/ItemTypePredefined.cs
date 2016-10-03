namespace Model.ArtifactModel.Enums
{
    /// <summary>
    /// Group masks used to calculate ItemTypePredefined values.
    /// </summary>
    enum ItemTypeEnumGroups
    {
        PrimitiveArtifactGroup = 0x1000,
        BaselineArtifactGroup = 0x100,
        CollectionArtifactGroup = 0x200,
        SubArtifactGroup = 0x2000,
        CustomArtifactGroup = 0x4000,
        ObsoleteArtifactGroup = 0x8000,

        /// <summary>
        /// Predefined artifacts = 4096
        /// </summary>
        GroupMask = 0xF000
    }

    /// <summary>
    /// This contains ALL ItemTypePredefined enums, for artifacts, sub-artifacts and special types like collections & baselines...
    /// For any artifact, we use ItemTypePredefined to indicate what the "base" type should be, even when customers
    /// create their own sub-type.  This is the link between the object and the data in the database.
    /// For example, if an item has a PrimitiveItemType of 4105, it is a use case (4096 + 9 where it is GroupMask + UseCase)
    /// </summary>
    public enum ItemTypePredefined
    {
        // Taken from:  blueprint-current/Source/BluePrintSys.RC.CrossCutting.Portable/Enums/ItemTypePredefined.cs

        /// <summary>
        /// Predefined artifacts = 4096
        /// </summary>

        None = 0x0000,

        /// <summary>
        /// Predefined primitive artifacts
        /// </summary>
        Project = ItemTypeEnumGroups.PrimitiveArtifactGroup | 1,
        Baseline = ItemTypeEnumGroups.PrimitiveArtifactGroup | 2,
        Glossary = ItemTypeEnumGroups.PrimitiveArtifactGroup | 3,
        //Term                       = ItemTypeEnumGroups.PrimitiveArtifactGroup | 4,
        TextualRequirement = ItemTypeEnumGroups.PrimitiveArtifactGroup | 5,

        /// <summary>
        /// A project folder, which has other artifacts or folders within it
        /// </summary>
        PrimitiveFolder = ItemTypeEnumGroups.PrimitiveArtifactGroup | 6,
        BusinessProcess = ItemTypeEnumGroups.PrimitiveArtifactGroup | 7,
        Actor = ItemTypeEnumGroups.PrimitiveArtifactGroup | 8,
        UseCase = ItemTypeEnumGroups.PrimitiveArtifactGroup | 9,
        DataElement = ItemTypeEnumGroups.PrimitiveArtifactGroup | 10,
        UIMockup = ItemTypeEnumGroups.PrimitiveArtifactGroup | 11,
        GenericDiagram = ItemTypeEnumGroups.PrimitiveArtifactGroup | 12,
        Document = ItemTypeEnumGroups.PrimitiveArtifactGroup | 14,
        Storyboard = ItemTypeEnumGroups.PrimitiveArtifactGroup | 15,
        DomainDiagram = ItemTypeEnumGroups.PrimitiveArtifactGroup | 16,
        UseCaseDiagram = ItemTypeEnumGroups.PrimitiveArtifactGroup | 17, // new after 03/27/2012
        Process = ItemTypeEnumGroups.PrimitiveArtifactGroup | 18, // new after 12/18/2015

        /// <summary>
        /// Special Baselines and Reviews related artifacts
        /// BaselineArtifactGroup = 256
        /// </summary>
        /// <remarks>
        /// BaselineFolder = 4096+256+1 = 4353
        /// </remarks>
        BaselineFolder = ItemTypeEnumGroups.PrimitiveArtifactGroup | ItemTypeEnumGroups.BaselineArtifactGroup | 1,
        ArtifactBaseline = ItemTypeEnumGroups.PrimitiveArtifactGroup | ItemTypeEnumGroups.BaselineArtifactGroup | 2,
        ArtifactReviewPackage = ItemTypeEnumGroups.PrimitiveArtifactGroup | ItemTypeEnumGroups.BaselineArtifactGroup | 3,

        /// <summary>
        /// Special Collections related artifacts
        /// CollectionArtifactGroup = 512
        /// CollectionFolder = 4609
        /// ArtifactCollection = 4610
        /// </summary>
        CollectionFolder = ItemTypeEnumGroups.PrimitiveArtifactGroup | ItemTypeEnumGroups.CollectionArtifactGroup | 1,
        ArtifactCollection = ItemTypeEnumGroups.PrimitiveArtifactGroup | ItemTypeEnumGroups.CollectionArtifactGroup | 2,

        /// <summary>
        /// Predefined sub artifacts
        /// SubArtifactGroup=8192
        /// </summary>
        GDConnector = ItemTypeEnumGroups.SubArtifactGroup | 1,
        GDShape = ItemTypeEnumGroups.SubArtifactGroup | 2,
        BPConnector = ItemTypeEnumGroups.SubArtifactGroup | 3,
        PreCondition = ItemTypeEnumGroups.SubArtifactGroup | 4,
        PostCondition = ItemTypeEnumGroups.SubArtifactGroup | 5,
        Flow = ItemTypeEnumGroups.SubArtifactGroup | 6,
        Step = ItemTypeEnumGroups.SubArtifactGroup | 7,
        Extension = ItemTypeEnumGroups.SubArtifactGroup | 8,
        //Canvas                     = ItemTypeEnumGroups.SubArtifactGroup | 9,
        //Widget                     = ItemTypeEnumGroups.SubArtifactGroup | 10,
        //BP elements (BPObject replacement)
        //BPPool                     = ItemTypeEnumGroups.SubArtifactGroup | 11,
        //BPLane                     = ItemTypeEnumGroups.SubArtifactGroup | 12,
        //BPEvent                    = ItemTypeEnumGroups.SubArtifactGroup | 13,
        //BPTask                     = ItemTypeEnumGroups.SubArtifactGroup | 14,
        //BPGateway                  = ItemTypeEnumGroups.SubArtifactGroup | 15,
        //BPDataObject               = ItemTypeEnumGroups.SubArtifactGroup | 16,
        //BPGroup                    = ItemTypeEnumGroups.SubArtifactGroup | 17,
        //BPAnnotation               = ItemTypeEnumGroups.SubArtifactGroup | 18,
        //BPExpandedSubProcess       = ItemTypeEnumGroups.SubArtifactGroup | 19,
        //BPLinkLabel                = ItemTypeEnumGroups.SubArtifactGroup | 20,
        Bookmark = ItemTypeEnumGroups.SubArtifactGroup | 21,
        //Callout                    = ItemTypeEnumGroups.SubArtifactGroup | 22,
        //Frame                      = ItemTypeEnumGroups.SubArtifactGroup | 23,
        BaselinedArtifactSubscribe = ItemTypeEnumGroups.SubArtifactGroup | 24,
        Term = ItemTypeEnumGroups.SubArtifactGroup | 25,
        Content = ItemTypeEnumGroups.SubArtifactGroup | 26,
        DDConnector = ItemTypeEnumGroups.SubArtifactGroup | 27,
        DDShape = ItemTypeEnumGroups.SubArtifactGroup | 28,
        BPShape = ItemTypeEnumGroups.SubArtifactGroup | 29,
        SBConnector = ItemTypeEnumGroups.SubArtifactGroup | 30,
        SBShape = ItemTypeEnumGroups.SubArtifactGroup | 31,
        UIConnector = ItemTypeEnumGroups.SubArtifactGroup | 32,
        UIShape = ItemTypeEnumGroups.SubArtifactGroup | 33,
        UCDConnector = ItemTypeEnumGroups.SubArtifactGroup | 34, // new after 03/27/2012
        UCDShape = ItemTypeEnumGroups.SubArtifactGroup | 35, // new after 03/27/2012
        PROShape = ItemTypeEnumGroups.SubArtifactGroup | 36,

        /// <summary>
        /// Obsolete artifacts, for backward compatibility with previous versions
        /// </summary>

        DataObject = ItemTypeEnumGroups.ObsoleteArtifactGroup | 1
    }

    /// <summary>
    /// Contains all regular artifact base types used in Nova ItemTypePredefined.
    /// </summary>
    public enum ArtifactTypePredefined
    {
        /// <summary>
        /// Predefined artifacts = 4096
        /// </summary>

        None = 0x0000,

        /// <summary>
        /// Predefined primitive artifacts
        /// </summary>
        Project = ItemTypePredefined.Project,
        Baseline = ItemTypePredefined.Baseline,
        Glossary = ItemTypePredefined.Glossary,
        //Term                       = ItemTypePredefined.Term,
        TextualRequirement = ItemTypePredefined.TextualRequirement,

        /// <summary>
        /// A project folder, which has other artifacts or folders within it
        /// </summary>
        PrimitiveFolder = ItemTypePredefined.PrimitiveFolder,
        BusinessProcess = ItemTypePredefined.BusinessProcess,
        Actor = ItemTypePredefined.Actor,
        UseCase = ItemTypePredefined.UseCase,
        DataElement = ItemTypePredefined.DataElement,   // Not used.
        UIMockup = ItemTypePredefined.UIMockup,
        GenericDiagram = ItemTypePredefined.GenericDiagram,
        Document = ItemTypePredefined.Document,
        Storyboard = ItemTypePredefined.Storyboard,
        DomainDiagram = ItemTypePredefined.DomainDiagram,
        UseCaseDiagram = ItemTypePredefined.UseCaseDiagram
    }

    /// <summary>
    /// Contains all special artifact base types (such as baselines and collections) used in Nova ItemTypePredefined.
    /// </summary>
    public enum BaselineAndCollectionTypePredefined
    {
        None = 0x0000,

        /// <summary>
        /// Special Baselines and Reviews related artifacts
        /// BaselineArtifactGroup = 256
        /// </summary>
        /// <remarks>
        /// BaselineFolder = 4096+256+1 = 4353
        /// </remarks>
        BaselineFolder = ItemTypePredefined.BaselineFolder,
        ArtifactBaseline = ItemTypePredefined.ArtifactBaseline,
        ArtifactReviewPackage = ItemTypePredefined.ArtifactReviewPackage,

        /// <summary>
        /// Special Collections related artifacts
        /// CollectionArtifactGroup = 512
        /// CollectionFolder = 4609
        /// ArtifactCollection = 4610
        /// </summary>
        CollectionFolder = ItemTypePredefined.CollectionFolder,
        ArtifactCollection = ItemTypePredefined.ArtifactCollection
    }

    /// <summary>
    /// Contains all sub-artifact base types used in Nova ItemTypePredefined.
    /// </summary>
    public enum SubArtifactTypePredefined
    {
        None = 0x0000,

        /// <summary>
        /// Predefined sub artifacts
        /// SubArtifactGroup=8192
        /// </summary>
        GDConnector = ItemTypePredefined.GDConnector,
        GDShape = ItemTypePredefined.GDShape,
        BPConnector = ItemTypePredefined.BPConnector,
        PreCondition = ItemTypePredefined.PreCondition,
        PostCondition = ItemTypePredefined.PostCondition,
        Flow = ItemTypePredefined.Flow,
        Step = ItemTypePredefined.Step,
        Extension = ItemTypePredefined.Extension,
        //Canvas                     = ItemTypePredefined.Canvas,
        //Widget                     = ItemTypePredefined.Widget,
        //BP elements (BPObject replacement)
        //BPPool                     = ItemTypePredefined.BPPool,
        //BPLane                     = ItemTypePredefined.BPLane,
        //BPEvent                    = ItemTypePredefined.BPEvent,
        //BPTask                     = ItemTypePredefined.BPTask,
        //BPGateway                  = ItemTypePredefined.BPGateway,
        //BPDataObject               = ItemTypePredefined.BPDataObject,
        //BPGroup                    = ItemTypePredefined.BPGroup,
        //BPAnnotation               = ItemTypePredefined.BPAnnotation,
        //BPExpandedSubProcess       = ItemTypePredefined.BPExpandedSubProcess,
        //BPLinkLabel                = ItemTypePredefined.BPLinkLabel,
        Bookmark = ItemTypePredefined.Bookmark,
        //Callout                    = ItemTypePredefined.Callout,
        //Frame                      = ItemTypePredefined.Frame,
        BaselinedArtifactSubscribe = ItemTypePredefined.BaselinedArtifactSubscribe,
        Term = ItemTypePredefined.Term,
        Content = ItemTypePredefined.Content,
        DDConnector = ItemTypePredefined.DDConnector,
        DDShape = ItemTypePredefined.DDShape,
        BPShape = ItemTypePredefined.BPShape,
        SBConnector = ItemTypePredefined.SBConnector,
        SBShape = ItemTypePredefined.SBShape,
        UIConnector = ItemTypePredefined.UIConnector,
        UIShape = ItemTypePredefined.UIShape,
        UCDConnector = ItemTypePredefined.UCDConnector,
        UCDShape = ItemTypePredefined.UCDShape
    }
}
