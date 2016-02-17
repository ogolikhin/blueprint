namespace Model
{
    public enum ItemTypePredefined
    {
         Project                    = 4097,
         Baseline                   = 4098,
         Glossary                   = 4099,
         TextualRequirement         = 4101,
         PrimitiveFolder            = 4102,
         BusinessProcess            = 4103,
         Actor                      = 4104,
         UseCase                    = 4105,
         DataElement                = 4106,
         UIMockup                   = 4107,
         GenericDiagram             = 4108,
         Document                   = 4110,
         Storyboard                 = 4111,
         DomainDiagram              = 4112,
         UseCaseDiagram             = 4113,
         Process                    = 4114,
         BaselineFolder             = 4353,
         ArtifactBaseline           = 4354,
         ArtifactReviewPackage      = 4355,
         GDConnector                = 8193,
         GDShape                    = 8194,
         BPConnector                = 8195,
         PreCondition               = 8196,
         PostCondition              = 8197,
         Flow                       = 8198,
         Step                       = 8199,
         BaselinedArtifactSubscribe = 8216,
         Term                       = 8217,
         Content                    = 8218,
         DDConnector                = 8219,
         DDShape                    = 8220,
         BPShape                    = 8221,
         SBConnector                = 8222,
         SBShape                    = 8223,
         UIConnector                = 8224,
         UIShape                    = 8225,
         UCDConnector               = 8226,
         UCDShape                   = 8227,
         PROShape                   = 8228
    }

    public interface IArtifactReference
    {
        #region Properties

        /// <summary>
        /// The Id of the Artifact
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The Project Id for the artifact
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// The name of the artifact
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        string TypePreffix { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The link to navigate to the artifact
        /// </summary>
        string Link { get; set; }

        #endregion Properties
    }
}
