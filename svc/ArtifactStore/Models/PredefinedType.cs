namespace ArtifactStore.Models
{
    public enum PredefinedType
    {
        Unknown = 0,

        // Artifacts
        Folder = 1,
        Actor = 2,
        Document = 3,
        DomainDiagram = 4,
        GenericDiagram = 5,
        Glossary = 6,
        Process = 7,
        Storyboard = 8,
        Requirement = 9,
        UiMockup = 10,
        UseCase = 11,
        UseCaseDiagram = 12,
        
        //BaseLines and Reviews
        BaselineReviewFolder = 13,
        Baleline = 14,
        Review = 15,

        //Collections
        CollectionFolder = 16,
        Collection = 17
    }
}