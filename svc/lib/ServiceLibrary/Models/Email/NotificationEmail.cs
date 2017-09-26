namespace ServiceLibrary.Models.Email
{
    public interface INotificationEmail
    {
        string ProjectName { get; }
        int ProjectId { get; }
        string ArtifactName { get; }
        int ArtifactId { get; }
        string ArtifactUrl { get; }
        string Message { get; }
        string Header { get; }
        string LogoImageSrc { get; }
        string BlueprintUrl { get; }
    }

    public class NotificationEmail : INotificationEmail
    {
        public string ProjectName { get; }
        public int ProjectId { get; }
        public string ArtifactName { get; }
        public int ArtifactId { get; }
        public string ArtifactUrl { get; }
        public string Message { get; }
        public string Header { get; }
        public string LogoImageSrc { get; }
        public string BlueprintUrl { get; }

        public NotificationEmail(int projectId, string projectName,  int artifactId, string artifactName, string artifactUrl, string message, string header, string logoImageSrc, string blueprintUrl)
        {
            //don't pass null values to the template
            ProjectId = projectId;
            ProjectName = projectName ?? string.Empty;
            ArtifactId = artifactId;
            ArtifactName = artifactName ?? string.Empty;
            ArtifactUrl = artifactUrl ?? string.Empty;
            Message = message ?? string.Empty;
            Header = header ?? string.Empty;
            LogoImageSrc = logoImageSrc ?? string.Empty;
            BlueprintUrl = blueprintUrl ?? string.Empty;
        }
    }
}
