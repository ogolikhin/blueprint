namespace ArtifactStore.Repositories.VersionControl
{
    public interface IPublishRepositoriesContainer
    {
        IVersionControlRepository VersionControlRepo { get; }
        IPublishRepository PublishItemsRepo { get; }
        IPublishRepository PublishRelationshipsRepo { get; }
        IPublishRepository PublishPropertiesRepo { get; }
        IPublishRepository PublishAttachmentsRepo { get; }
        IPublishRepository PublishReuseProcessingRepo { get; }
        IPublishRepository PublishCollectionAssignmentsRepo { get; }
        IPublishRepository PublishJournalRepo { get; }
    }

    public class PublishRepositoriesContainer : IPublishRepositoriesContainer
    {
        public IVersionControlRepository VersionControlRepo { get; }
        public IPublishRepository PublishItemsRepo { get; }
        public IPublishRepository PublishRelationshipsRepo { get; }
        public IPublishRepository PublishPropertiesRepo { get; }
        public IPublishRepository PublishAttachmentsRepo { get; }
        public IPublishRepository PublishReuseProcessingRepo { get; }
        public IPublishRepository PublishCollectionAssignmentsRepo { get; }
        public IPublishRepository PublishJournalRepo { get; }

        public PublishRepositoriesContainer() : this(
            new SqlVersionControlRepository(),
            new SqlPublishItemsRepository(),
            new SqlPublishRelationshipsRepository(),
            new SqlPublishPropertiesRepository(),
            new SqlPublishAttachmentsRepository(),
            new SqlPublishReuseProcessingRepository(),
            new SqlPublishCollectionAssignmentsRepository(),
            new SqlJournalPublishRepository())
        {

        }

        public PublishRepositoriesContainer(IVersionControlRepository versionControlRepository, IPublishRepository publishItemsRepository, IPublishRepository publishRelationshipsRepo, IPublishRepository publishPropertiesRepo, IPublishRepository publishAttachmentsRepo, IPublishRepository publishReuseProcessingRepo, IPublishRepository publishCollectionAssignmentsRepo, IPublishRepository publishJournalRepo)
        {
            VersionControlRepo = versionControlRepository;
            PublishItemsRepo = publishItemsRepository;
            PublishRelationshipsRepo = publishRelationshipsRepo;
            PublishPropertiesRepo = publishPropertiesRepo;
            PublishAttachmentsRepo = publishAttachmentsRepo;
            PublishReuseProcessingRepo = publishReuseProcessingRepo;
            PublishCollectionAssignmentsRepo = publishCollectionAssignmentsRepo;
            PublishJournalRepo = publishJournalRepo;
        }
    }
}