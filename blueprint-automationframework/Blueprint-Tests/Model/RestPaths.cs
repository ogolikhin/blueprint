using System.Diagnostics.CodeAnalysis;

namespace Model
{
    // ********************************************************************************************
    // NOTE:  PLEASE keep everything here in alphabetical order and keep the literal strings
    //        lined up vertically so it's easy to see what here and what isn't.
    //        If Visual Studio screws up the formatting, fix it.
    // ********************************************************************************************
    public static class RestPaths
    {
        [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
        public static class OpenApi
        {
            public const string PROJECT                             = "api/v1/projects/{0}";
            public const string PROJECTS                            = "api/v1/projects";

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class Projects
            {
                public const string ARTIFACT                        = "api/v1/projects/{0}/artifacts/{1}";
                public const string ARTIFACTS                       = "api/v1/projects/{0}/artifacts";

                public static class Artifacts
                {
                    public const string ATTACHMENTS                 = "api/v1/projects/{0}/artifacts/{1}/attachments";

                    public static class SubArtifacts
                    {
                        public const string ATTACHMENTS             = "api/v1/projects/{0}/artifacts/{1}/subartifacts/{2}/attachments";
                    }
                }

                [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]   // Ignore this warning.
                public static class MetaData
                {
                    public const string ARTIFACT_TYPES              = "api/v1/projects/{0}/metadata/artifactTypes";
                }
            }

            public static class VersionControl
            {
                public const string PUBLISH                         = "api/v1/vc/publish";
                public const string DISCARD                         = "api/v1/vc/discard";
            }
        }

        public static class Svc
        {
            [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]       // Ignore this warning.
            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class AccessControl
            {
                public const string SESSIONS                        = "svc/accesscontrol/sessions/{0}";
                public const string SESSION                         = "svc/accesscontrol/sessions";
                public const string STATUS                          = "svc/accesscontrol/status";

                public static class Licenses
                {
                    public const string ACTIVE                      = "svc/accesscontrol/licenses/active";
                    public const string LOCKED                      = "svc/accesscontrol/licenses/locked";
                    public const string TRANSACTIONS                = "svc/accesscontrol/licenses/transactions";
                }

                public static class Sessions
                {
                    public const string SELECT                      = "svc/accesscontrol/sessions/select";
                }

                public static class Status
                {
                    public const string UPCHECK                     = "svc/accesscontrol/status/upcheck";
                }
            }

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class AdminStore
            {
                public const string LOG                             = "svc/adminstore/log";
                public const string SESSIONS                        = "svc/adminstore/sessions";
                public const string STATUS                          = "svc/adminstore/status";

                public static class Config
                {
                    public const string SETTINGS                    = "svc/adminstore/config/settings";
                    public const string CONFIG_JS                   = "svc/adminstore/config/config.js";
                }

                [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
                public static class Instance
                {
                    public const string FOLDERS                     = "svc/adminstore/instance/folders/{0}";
                    public const string PROJECTS                    = "svc/adminstore/instance/projects/{0}";

                    public static class Folders
                    {
                        public const string CHILDREN                = "svc/adminstore/instance/folders/{0}/children";
                    }
                }

                public static class Licenses
                {
                    public const string TRANSACTIONS                = "svc/adminstore/licenses/transactions";
                }

                public static class Sessions
                {
                    public const string SSO                         = "svc/adminstore/sessions/sso";
                }

                public static class Status
                {
                    public const string UPCHECK                     = "svc/adminstore/status/upcheck";
                }

                public static class Users
                {
                    public const string LOGINUSER                   = "svc/adminstore/users/loginuser";
                    public const string RESET                       = "svc/adminstore/users/reset";
                }
            }

            public static class Components
            {
                public static class FileStore
                {
                    /// <summary>
                    /// Path to upload files to FileStore.  {0} = Filename.
                    /// </summary>
                    public const string FILES                       = "svc/components/filestore/files/{0}";
                }

                public static class RapidReview
                {
                    public const string DIAGRAM                     = "svc/components/RapidReview/diagram/{0}";
                    public const string GLOSSARY                    = "svc/components/RapidReview/glossary/{0}";
                    public const string USECASE                     = "svc/components/RapidReview/usecase/{0}";

                    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
                    public static class Artifacts
                    {
                        public const string DISCUSSIONS             = "svc/components/RapidReview/artifacts/{0}/discussions";
                        public const string PROPERTIES              = "svc/components/RapidReview/artifacts/properties";

                        public static class Discussions
                        {
                            public const string REPLY               = "svc/components/RapidReview/artifacts/{0}/discussions/{1}/reply";
                        }
                    }

                    public static class Items
                    {
                        public const string PROPERTIES              = "svc/components/RapidReview/items/{0}/properties";
                    }
                }

                public static class Storyteller
                {
                    public const string ARTIFACT_INFO               = "svc/components/storyteller/artifactInfo/{0}";

                    /// <summary>
                    /// Get the Storyteller process for the specified Artifact ID.  {0} = artifactId.
                    /// </summary>
                    public const string PROCESSES                   = "svc/components/storyteller/processes/{0}";

                    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
                    public static class Projects
                    {
                        public const string PROCESSES               = "svc/components/storyteller/projects/{0}/processes";

                        public static class ArtifactTypes
                        {
                            /// <summary>
                            /// Get the User Story Artifact type for the specified Project Id.  {0} = projectId
                            /// </summary>
                            public const string USER_STORY          = "svc/components/storyteller/projects/{0}/artifacttypes/userstory";
                        }

                        public static class Processes
                        {
                            public const string USERSTORIES         = "svc/components/storyteller/projects/{0}/processes/{1}/userstories";
                        }
                    }
                }
            }

            public static class Shared
            {
                public static class Artifacts
                {
                    public const string LOCK                    = "svc/shared/artifacts/lock";
                    public const string SEARCH                  = "svc/shared/artifacts/search";
                    public const string DISCARD                 = "svc/shared/artifacts/discard";
                    public const string PUBLISH                 = "svc/shared/artifacts/publish";
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
