namespace ServiceLibrary.Models
{
    // The content is copied from the Raptor solution, and Display attributes are removed.
    // KEEP IN SYNC!

    /// <summary>
    /// For any artifact, we use ItemTypePredefined to indicate what the "base" type should be, even when customers
    /// create their own sub-type.  This is the link between the object and the data in the database.
    /// For example, if an item has a PrimitiveItemType of 4105, it is a use case (4096 + 9 where it is GroupMask + UseCase)
    /// </summary>
    /// <remarks>
    /// Project                    = 4097
    /// Baseline                   = 4098
    /// Glossary                   = 4099
    /// TextualRequirement         = 4101
    /// PrimitiveFolder            = 4102
    /// BusinessProcess            = 4103
    /// Actor                      = 4104
    /// UseCase                    = 4105
    /// DataElement                = 4106
    /// UIMockup                   = 4107
    /// GenericDiagram             = 4108
    /// Document                   = 4110
    /// Storyboard                 = 4111
    /// DomainDiagram              = 4112
    /// UseCaseDiagram             = 4113
    /// Process                    = 4114
    /// BaselineFolder             = 4353
    /// ArtifactBaseline           = 4354
    /// ArtifactReviewPackage      = 4355
    /// GDConnector                = 8193
    /// GDShape                    = 8194
    /// BPConnector                = 8195
    /// PreCondition               = 8196
    /// PostCondition              = 8197
    /// Flow                       = 8198
    /// Step                       = 8199
    /// BaselinedArtifactSubscribe = 8216
    /// Term                       = 8217
    /// Content                    = 8218
    /// DDConnector                = 8219
    /// DDShape                    = 8220
    /// BPShape                    = 8221
    /// SBConnector                = 8222
    /// SBShape                    = 8223
    /// UIConnector                = 8224
    /// UIShape                    = 8225
    /// UCDConnector               = 8226
    /// UCDShape                   = 8227
    /// PROShape                   = 8228
    /// </remarks>
    public enum ItemTypePredefined
    {
        /// <summary>
        /// Predefined artifacts = 4096
        /// </summary>
        GroupMask = 0xF000,

        //None 
        None = 0x0000,

        /// <summary>
        /// Predefined primitive artifacts
        /// </summary>
        PrimitiveArtifactGroup = 0x1000,
        Project = PrimitiveArtifactGroup | 1,
        Baseline = PrimitiveArtifactGroup | 2,
        Glossary = PrimitiveArtifactGroup | 3,
        //Term                       = PrimitiveArtifactGroup | 4,
        TextualRequirement = PrimitiveArtifactGroup | 5,

        /// <summary>
        /// A project folder, which has other artifacts or folders within it
        /// </summary>
        PrimitiveFolder = PrimitiveArtifactGroup | 6,
        BusinessProcess = PrimitiveArtifactGroup | 7,
        Actor = PrimitiveArtifactGroup | 8,
        UseCase = PrimitiveArtifactGroup | 9,
        DataElement = PrimitiveArtifactGroup | 10,
        UIMockup = PrimitiveArtifactGroup | 11,
        GenericDiagram = PrimitiveArtifactGroup | 12,
        Document = PrimitiveArtifactGroup | 14,
        Storyboard = PrimitiveArtifactGroup | 15,
        DomainDiagram = PrimitiveArtifactGroup | 16,
        UseCaseDiagram = PrimitiveArtifactGroup | 17,
        Process = PrimitiveArtifactGroup | 18, // new after 12/18/2015

        /// <summary>
        /// Special Baselines and Reviews related artifacts
        /// BaselineArtifactGroup = 256
        /// </summary>
        /// <remarks>
        /// BaselineFolder = 4096+256+1 = 4353
        /// </remarks>
        BaselineArtifactGroup = 0x100,
        BaselineFolder = PrimitiveArtifactGroup | BaselineArtifactGroup | 1,
        ArtifactBaseline = PrimitiveArtifactGroup | BaselineArtifactGroup | 2,
        ArtifactReviewPackage = PrimitiveArtifactGroup | BaselineArtifactGroup | 3,

        /// <summary>
        /// Special Collections related artifacts
        /// CollectionArtifactGroup = 512
        /// CollectionFolder = 4609
        /// ArtifactCollection = 4610
        /// </summary>
        CollectionArtifactGroup = 0x200,
        CollectionFolder = PrimitiveArtifactGroup | CollectionArtifactGroup | 1,
        ArtifactCollection = PrimitiveArtifactGroup | CollectionArtifactGroup | 2,

        /// <summary>
        /// Predefined sub artifacts
        /// SubArtifactGroup=8192
        /// </summary>
        SubArtifactGroup = 0x2000,
        GDConnector = SubArtifactGroup | 1,
        GDShape = SubArtifactGroup | 2,
        BPConnector = SubArtifactGroup | 3,
        PreCondition = SubArtifactGroup | 4,
        PostCondition = SubArtifactGroup | 5,
        Flow = SubArtifactGroup | 6,
        Step = SubArtifactGroup | 7,
        Extension = SubArtifactGroup | 8,
        //Canvas                     = SubArtifactGroup | 9,
        //Widget                     = SubArtifactGroup | 10,
        //BP elements (BPObject replacement)
        //BPPool                     = SubArtifactGroup | 11,
        //BPLane                     = SubArtifactGroup | 12,
        //BPEvent                    = SubArtifactGroup | 13,
        //BPTask                     = SubArtifactGroup | 14,
        //BPGateway                  = SubArtifactGroup | 15,
        //BPDataObject               = SubArtifactGroup | 16,
        //BPGroup                    = SubArtifactGroup | 17,
        //BPAnnotation               = SubArtifactGroup | 18,
        //BPExpandedSubProcess       = SubArtifactGroup | 19,
        //BPLinkLabel                = SubArtifactGroup | 20,
        Bookmark = SubArtifactGroup | 21,
        //Callout                    = SubArtifactGroup | 22,
        //Frame                      = SubArtifactGroup | 23,
        BaselinedArtifactSubscribe = SubArtifactGroup | 24,
        Term = SubArtifactGroup | 25,
        Content = SubArtifactGroup | 26,
        DDConnector = SubArtifactGroup | 27,
        DDShape = SubArtifactGroup | 28,
        BPShape = SubArtifactGroup | 29,
        SBConnector = SubArtifactGroup | 30,
        SBShape = SubArtifactGroup | 31,
        UIConnector = SubArtifactGroup | 32,
        UIShape = SubArtifactGroup | 33,
        UCDConnector = SubArtifactGroup | 34, // new after 03/27/2012
        UCDShape = SubArtifactGroup | 35, // new after 03/27/2012
        PROShape = SubArtifactGroup | 36,

        /// <summary>
        /// Custom artifacts
        /// </summary>
        CustomArtifactGroup = 0x4000,

        /// <summary>
        /// Obsolete artifacts, for backward compatibility with previous versions
        /// </summary>
        ObsoleteArtifactGroup = 0x8000,

        DataObject = ObsoleteArtifactGroup | 1
    }
}
