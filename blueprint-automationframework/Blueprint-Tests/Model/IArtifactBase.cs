
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
        string Description { get; set; }
        string Id { get; }
        string Name { get; set; }
    }
}
