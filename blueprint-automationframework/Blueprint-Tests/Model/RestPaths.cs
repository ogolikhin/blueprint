namespace Model
{
    // ********************************************************************************************
    // NOTE:  PLEASE keep everything here in alphabetical order and keep the literal strings
    //        lined up virtically so it's easy to see what here and what isn't.
    //        If Visual Studio screws up the formatting, fit it.
    // ********************************************************************************************
    public static class RestPaths
    {
        public static class OpenApi
        {
            public const string URL_PUBLISH = "api/v1/vc/publish";
            public const string URL_DISCARD = "api/v1/vc/discard";

            public static class Projects
            {
                public static class Artifacts
                {
                    public const string ATTACHMENTS                 = "api/v1/projects/{0}/artifacts/{1}/attachments";

                    public static class SubArtifacts
                    {
                        public const string ATTACHMENTS             = "api/v1/projects/{0}/artifacts/{1}/subartifacts/{2}/attachments";
                    }
                }
            }
        }

        public static class Svc
        {
            public static class Components
            {
                public static class RapidReview
                {
                    public const string ARTIFACT_DISCUSSIONS        = "svc/components/RapidReview/artifacts/{0}/discussions";
                    public const string ARTIFACT_DISCUSSION_REPLY   = "svc/components/RapidReview/artifacts/{0}/discussions/{1}/reply";
                    public const string DIAGRAM                     = "svc/components/RapidReview/diagram";
                    public const string GLOSSARY                    = "svc/components/RapidReview/glossary";
                    public const string USECASE                     = "svc/components/RapidReview/usecase";
                    public const string ARTIFACTPROPERTIES          = "svc/components/RapidReview/artifacts/properties";
                }

                public static class Storyteller
                {
                    public const string ARTIFACT_INFO               = "svc/components/storyteller/artifactInfo";

                    /// <summary>
                    /// Get the User Story Artifact type for the specified Project Id.  {0} = projectId
                    /// </summary>
                    public const string USER_STORY_ARTIFACT_TYPES   = "svc/components/storyteller/{0}/artifacttypes/userstory";

                    private const string SVC_PATH = "svc/components/storyteller";
                    private const string URL_PROJECTS = "projects";
                    private const string URL_PROCESSES = "processes";
                    private const string URL_USERSTORIES = "userstories";
                    private const string URL_ARTIFACTTYPES = "artifacttypes/userstory";

                    private const string SVC_UPLOAD_PATH = "svc/components/filestore/files";
                }
            }

            public static class Shared
            {
                public static class Artifacts
                {
                    public const string LOCK                    = "svc/shared/artifacts/lock";
                    public const string SEARCH                  = "svc/shared/artifacts/search";
                    public const string DISCARD                 = "svc/shared/artifacts/discard";
                }

                /// <summary>
                /// Get the Artifact Reference list (Breadcrumb) for the specified Artifact IDs.
                /// {0} = all the Artifact IDs in the breadcrumb.  Ex.  1/2/3/4
                /// </summary>
                public const string NAVIGATION                  = "svc/shared/navigation/{0}";
            }
        }
    }
}
