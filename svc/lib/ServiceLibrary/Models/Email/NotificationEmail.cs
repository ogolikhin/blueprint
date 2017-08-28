﻿namespace ServiceLibrary.Models.Email
{
    public interface INotificationEmail
    {
        string ProjectName { get; }
        int ProjectId { get; }
        string ArtifactName { get; }
        int ArtifactId { get; }
        string ArtifactUrl { get; }
        string Body { get; }
        string Header { get; }
    }

    public class NotificationEmail : INotificationEmail
    {
        public string ProjectName { get; }
        public int ProjectId { get; }
        public string ArtifactName { get; }
        public int ArtifactId { get; }
        public string ArtifactUrl { get; }
        public string Body { get; }
        public string Header { get; }

        public NotificationEmail(int projectId, string projectName,  int artifactId, string artifactName, string artifactUrl, string body, string header)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            ArtifactId = artifactId;
            ArtifactName = artifactName;
            ArtifactUrl = artifactUrl;
            Body = body;
            Header = header;
        }
    }
}
