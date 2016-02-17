namespace Model
{
    public enum ArtifactType
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
        ArtifactType ArtifactType { get; set; }
        int Id { get; set; }
        string Name { get; set; }
    }
}
