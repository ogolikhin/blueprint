using System.Diagnostics.CodeAnalysis;
// ReSharper disable InconsistentNaming

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
            public const string PROJECTS_id_                        = "api/v1/projects/{0}";
            public const string PROJECTS                            = "api/v1/projects";

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class Projects_id_
            {
                public const string ARTIFACTS_id_                   = "api/v1/projects/{0}/artifacts/{1}";
                public const string ARTIFACTS                       = "api/v1/projects/{0}/artifacts";

                public static class Artifacts_id_
                {
                    public const string ATTACHMENTS                 = "api/v1/projects/{0}/artifacts/{1}/attachments";
                    public const string TRACES                      = "api/v1/projects/{0}/artifacts/{1}/traces";

                    public static class SubArtifacts_id_
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

        [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
        public static class Svc
        {
            public const string STATUS                              = "svc/status";

            [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]       // Ignore this warning.
            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class AccessControl
            {
                public const string SESSIONS_id_                    = "svc/accesscontrol/sessions/{0}";
                public const string SESSIONS                        = "svc/accesscontrol/sessions";
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
                    public const string FOLDERS_id_                 = "svc/adminstore/instance/folders/{0}";
                    public const string PROJECTS_id_                = "svc/adminstore/instance/projects/{0}";

                    public static class Folders_id_
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

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class ArtifactStore
            {
                public const string ARTIFACTS                       = "svc/bpartifactstore/artifacts";      // XXX: For some reason they decided to put this call in blueprint-current!
                public const string ARTIFACTS_id_                   = "svc/bpartifactstore/artifacts/{0}";  // XXX: For some reason they decided to put this call in blueprint-current!
                public const string STATUS                          = "svc/artifactstore/status";

                public static class Artifacts
                {
                    public const string PUBLISH                     = "svc/bpartifactstore/artifacts/publish";  // XXX: For some reason they decided to put this call in blueprint-current!
                    public const string DISCARD                     = "svc/bpartifactstore/artifacts/discard";
                    public const string UNPUBLISHED                 = "svc/bpartifactstore/artifacts/unpublished";
                }

                public static class Artifacts_id_
                {
                    public const string ATTACHMENT                  = "svc/artifactstore/artifacts/{0}/attachment";
                    public const string DISCUSSIONS                 = "svc/artifactstore/artifacts/{0}/discussions";
                    public const string RELATIONSHIPS               = "svc/artifactstore/artifacts/{0}/relationships";
                    public const string RELATIONSHIP_DETAILS        = "svc/artifactstore/artifacts/{0}/relationshipdetails";
                    public const string VERSION                     = "svc/artifactstore/artifacts/{0}/version";
                    public const string SUBARTIFACTS                = "svc/artifactstore/artifacts/{0}/subartifacts";

                    public static class Discussions_id_
                    {
                        public const string REPLIES                 = "svc/artifactstore/artifacts/{0}/discussions/{1}/replies";
                    }
                }

                [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
                public static class Projects_id_
                {
                    public const string ARTIFACTS_id_               = "svc/artifactstore/projects/{0}/artifacts/{1}";
                    public const string CHILDREN                    = "svc/artifactstore/projects/{0}/children";

                    public static class Artifacts_id_
                    {
                        public const string CHILDREN                = "svc/artifactstore/projects/{0}/artifacts/{1}/children";
                    }

                    public static class Meta
                    {
                        public const string CUSTOM_TYPES            = "svc/artifactstore/projects/{0}/meta/customtypes";
                    }
                }

                public static class Status
                {
                    public const string UPCHECK                     = "svc/artifactstore/status/upcheck";
                }
            }

            public static class Components
            {
                public static class FileStore
                {
                    /// <summary>
                    /// Path to upload files to FileStore.  {0} = Filename.
                    /// </summary>
                    public const string FILES_filename_             = "svc/components/filestore/files/{0}";
                }

                public static class RapidReview
                {
                    public const string DIAGRAM_id_                 = "svc/components/RapidReview/diagram/{0}";
                    public const string GLOSSARY_id_                = "svc/components/RapidReview/glossary/{0}";
                    public const string USECASE_id_                 = "svc/components/RapidReview/usecase/{0}";

                    public static class Artifacts
                    {
                        public const string PROPERTIES              = "svc/components/RapidReview/artifacts/properties";
                    }

                    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
                    public static class Artifacts_id_
                    {
                        public const string DISCUSSIONS             = "svc/components/RapidReview/artifacts/{0}/discussions";
                        public const string DELETE_THREAD_ID        = "svc/components/RapidReview/artifacts/{0}/deletethread/{1}";
                        public const string DELETE_COMMENT_ID       = "svc/components/RapidReview/artifacts/{0}/deletecomment/{1}";


                        public static class Discussions_id_
                        {
                            public const string COMMENT             = "svc/components/RapidReview/artifacts/{0}/discussions/{1}";
                            public const string REPLY               = "svc/components/RapidReview/artifacts/{0}/discussions/{1}/reply";
                            public const string REPLY_ID            = "svc/components/RapidReview/artifacts/{0}/discussions/{1}/reply/{2}";
                        }
                    }

                    public static class Items_id_
                    {
                        public const string PROPERTIES              = "svc/components/RapidReview/items/{0}/properties";
                    }
                }

                public static class Storyteller
                {
                    public const string ARTIFACT_INFO_id_           = "svc/components/storyteller/artifactInfo/{0}";

                    /// <summary>
                    /// Get the Storyteller process for the specified Artifact ID.  {0} = artifactId.
                    /// </summary>
                    public const string PROCESSES_id_               = "svc/components/storyteller/processes/{0}";

                    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
                    public static class Projects_id_
                    {
                        public const string PROCESSES               = "svc/components/storyteller/projects/{0}/processes";

                        public static class ArtifactTypes
                        {
                            /// <summary>
                            /// Get the User Story Artifact type for the specified Project Id.  {0} = projectId
                            /// </summary>
                            public const string USER_STORY          = "svc/components/storyteller/projects/{0}/artifacttypes/userstory";
                        }

                        public static class Processes_id_
                        {
                            public const string USERSTORIES         = "svc/components/storyteller/projects/{0}/processes/{1}/userstories";
                        }
                    }
                }
            }

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class ConfigControl
            {
                public const string LOG                             = "svc/configcontrol/log";

                /// <summary>
                /// Gets config settings.  {0} is a bool for the allowRestricted parameter.
                /// </summary>
                public const string SETTINGS_bool_                  = "svc/configcontrol/settings/{0}";
                public const string STATUS                          = "svc/configcontrol/status";

                public static class Log
                {
                    public const string CLOG                        = "svc/configcontrol/log/CLog";
                    public const string STANDARD_LOG                = "svc/configcontrol/log/StandardLog";
                    public const string PERFORMANCE_LOG             = "svc/configcontrol/log/PerformanceLog";
                    public const string SQL_TRACE_LOG               = "svc/configcontrol/log/SQLTraceLog";
                    public const string GET_LOG                     = "svc/configcontrol/log/GetLog";
                }

                public static class Status
                {
                    public const string UPCHECK                     = "svc/configcontrol/status/upcheck";
                }
            }

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class FileStore
            {
                public const string FILES                           = "svc/filestore/files";

                public const string NOVAFILES                       = "svc/bpfilestore/files";
                /// <summary>
                /// Delete/Get/Head/Put files in FileStore.  {0} = File GUID.
                /// </summary>
                public const string FILES_id_                       = "svc/filestore/files/{0}";
                public const string STATUS                          = "svc/filestore/status";

                public const string NOVAFILE_id_                   = "svc/bpfilestore/file/{0}";

                public static class Status
                {
                    public const string UPCHECK                     = "svc/filestore/status/upcheck";
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
                public const string NAVIGATION_ids_             = "svc/shared/navigation/{0}";

                public static class Users
                {
                    public const string SEARCH                  = "svc/shared/users/search";
                }
            }

            public static class Status
            {
                public const string UPCHECK                     = "svc/status/upcheck";
            }
        }
    }
}
