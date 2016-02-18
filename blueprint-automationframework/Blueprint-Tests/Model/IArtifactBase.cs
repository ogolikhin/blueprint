namespace Model
{
    public enum BaseArtifactType
    {
        Actor,
        AgilePackEpic,
        AgilePackFeature,
        AgilePackScenario,
        AgilePackTheme,
        AgilePackUserStory,
        Baseline,
        BaselinesAndReviews,
        BaselinesAndReviewsFolder,
        BusinessProcessDiagram,
        Collection,
        CollectionFolder,
        Collections,
        Document,
        DomainDiagram,
        Folder,
        GenericDiagram,
        Glossary,
        Process,
        Project,
        Review,
        Storyboard,
        TextualRequirement,
        UIMockup,
        UseCase,
        UseCaseDiagram
    }

    public interface IArtifactBase
    {
        // TODO Find the way or wait for the API implementation which retrieve descrption
        // string Description { get; set; }
        BaseArtifactType ArtifactType { get; set; }
        int Id { get; set; }
        string Name { get; set; }
    }
}
