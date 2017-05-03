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
        public static class ImpactAnalysis
        {
            public const string IMPACT_ANALYSIS                     = "ImpactAnalysis/api/{0}/{1}";
        }

        [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
        public static class OpenApi
        {
            public const string LOGIN                               = "authentication/v1/login";
            public const string PROJECTS_id_                        = "api/v1/projects/{0}";
            public const string PROJECTS                            = "api/v1/projects";
            public const string USERS                               = "api/v1/users";

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
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

                public static class ALM
                {
                    public const string TARGETS                     = "api/v1/projects/{0}/alm/targets";

                    public static class Targets_id_
                    {
                        public const string JOBS                    = "api/v1/projects/{0}/alm/targets/{1}/jobs";
                    }
                }

                [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]   // Ignore this warning.
                public static class MetaData
                {
                    public const string ARTIFACT_TYPES              = "api/v1/projects/{0}/metadata/artifactTypes";
                }
            }

            public static class Users
            {
                public const string GET_id_                         = "api/v1/users/{0}";
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
                    public const string USAGE                       = "svc/AccessControl/licenses/usage";
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
                public const string JOBS                            = "svc/adminstore/jobs";
                public const string JOBS_id_                        = "svc/adminstore/jobs/{0}";

                public static class Config
                {
                    public const string SETTINGS                    = "svc/adminstore/config/settings";
                    public const string CONFIG_JS                   = "svc/adminstore/config/config.js";
                }

                public static class Groups
                {
                    public const string GROUPS                      = "svc/adminstore/groups";
                    public const string GROUPS_id_                  = "svc/adminstore/groups/{0}";
                    public const string SEARCH                      = "svc/adminstore/groups/search"; 
                }

                public static class Groups_id_
                {
                    public const string USERS                       = "svc/adminstore/groups/{0}/users";
                    public const string CHILDREN                    = "svc/adminstore/groups/{0}/children";
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

                    public static class Projects_id_
                    {
                        public const string NAVIGATIONPATH          = "svc/adminstore/instance/projects/{0}/navigationPath";
                    }
                }

                public static class Jobs
                {
                    public static class Process
                    {
                        public const string TESTGEN                 = "svc/adminstore/jobs/process/testgen";
                    }
                }

                public static class Jobs_id_
                {
                    public static class Result
                    {
                        public const string FILE                    = "svc/adminstore/jobs/{0}/result/file";
                    }
                }

                public static class Licenses
                {
                    public const string TRANSACTIONS                = "svc/adminstore/licenses/transactions";
                }

                public static class Sessions
                {
                    public const string ALIVE                       = "svc/adminstore/sessions/alive";
                    public const string SSO                         = "svc/adminstore/sessions/sso";
                }

                public static class Status
                {
                    public const string UPCHECK                     = "svc/adminstore/status/upcheck";
                }

                public static class Users
                {
                    public const string INSTANCE_ROLES              = "svc/adminstore/users/instanceroles";
                    public const string LOGINUSER                   = "svc/adminstore/users/loginuser";
                    public const string RESET                       = "svc/adminstore/users/reset";
                    public const string SEARCH                      = "svc/adminstore/users/search";
                    public const string USERS                       = "svc/adminstore/users";
                    public const string USERS_id_                   = "svc/adminstore/users/{0}";


                    public static class PasswordRecovery
                    {
                        public const string REQUEST                 = "svc/adminstore/users/passwordrecovery/request";
                        public const string RESET                   = "svc/adminstore/users/passwordrecovery/reset";
                    }
                }

                public static class Users_id_
                {
                    public const string GROUPS                      = "svc/adminstore/users/{0}/groups";
                    public const string ICON                        = "svc/adminstore/users/{0}/icon";
                }
            }

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class ArtifactStore
            {
                public const string ACTORICON_id_                   = "svc/bpartifactstore/diagram/actoricon/{0}";
                public const string ARTIFACTS                       = "svc/bpartifactstore/artifacts";      // XXX: For some reason they decided to put this call in blueprint-current!
                public const string ARTIFACTS_id_                   = "svc/bpartifactstore/artifacts/{0}";  // XXX: For some reason they decided to put this call in blueprint-current!
                public const string BASELINE_id_                    = "svc/bpartifactstore/baselines/{0}"; // Baseline
                public const string COLLECTION_id_                  = "svc/bpartifactstore/collections/{0}"; // Collection
                public const string CONTAINERS_id_                  = "svc/artifactstore/containers/{0}";
                public const string DIAGRAM_id_                     = "svc/bpartifactstore/diagram/{0}";    // NovaDiagramController.GetDiagram
                public const string GLOSSARY_id_                    = "svc/bpartifactstore/glossary/{0}";   // NovaGlossaryController.GetGlossary
                public const string IMAGES                          = "svc/bpartifactstore/images";
                public const string IMAGES_id_                      = "svc/bpartifactstore/images/{0}";
                public const string PROCESS_id_                     = "svc/bpartifactstore/process/{0}";    // NovaProcessController.GetNovaProcess
                public const string PROCESSUPDATE_id_               = "svc/bpartifactstore/processupdate/{0}";  // NovaProcessController.UpdateNovaProcess
                public const string STATUS                          = "svc/artifactstore/status";
                public const string USECASE_id_                     = "svc/bpartifactstore/usecase/{0}";    // NovaUseCaseController.GetUseCase

                public static class Artifacts
                {
                    public const string AUTHOR_HISTORIES            = "/svc/artifactstore/artifacts/authorHistories";
                    public const string BASELINE_INFO               = "svc/artifactstore/artifacts/baselineInfo";
                    public const string CREATE                      = "svc/bpartifactstore/artifacts/create";   // XXX: For some reason they decided to put this call in blueprint-current!
                    public const string DISCARD                     = "svc/bpartifactstore/artifacts/discard";  // XXX: For some reason they decided to put this call in blueprint-current!
                    public const string PUBLISH                     = "svc/bpartifactstore/artifacts/publish";  // XXX: For some reason they decided to put this call in blueprint-current!
                    public const string UNPUBLISHED                 = "svc/bpartifactstore/artifacts/unpublished";
                    public const string VERSION_CONTROL_INFO_id_    = "svc/artifactstore/artifacts/versioncontrolinfo/{0}";
                }

                public static class Artifacts_id_
                {
                    public const string ATTACHMENT                  = "svc/artifactstore/artifacts/{0}/attachment";
                    public const string ATTACHMENT_id_              = "svc/bpartifactstore/artifacts/{0}/attachments/{1}";
                    public const string COPY_TO_id_                 = "svc/bpartifactstore/artifacts/{0}/copyTo/{1}";   // XXX: For some reason they decided to put this call in blueprint-current!
                    public const string DISCUSSIONS                 = "svc/artifactstore/artifacts/{0}/discussions";
                    public const string MOVE_TO_id_                 = "svc/bpartifactstore/artifacts/{0}/moveTo/{1}";   // XXX: For some reason they decided to put this call in blueprint-current!
                    public const string RELATIONSHIPS               = "svc/artifactstore/artifacts/{0}/relationships";
                    public const string RELATIONSHIP_DETAILS        = "svc/artifactstore/artifacts/{0}/relationshipdetails";
                    public const string REVIEWS                     = "svc/artifactstore/artifacts/{0}/reviews";
                    public const string VERSION                     = "svc/artifactstore/artifacts/{0}/version";
                    public const string SUBARTIFACTS                = "svc/artifactstore/artifacts/{0}/subartifacts";
                    public const string SUBARTIFACTS_id_            = "svc/bpartifactstore/artifacts/{0}/subartifacts/{1}";
                    public const string NAVIGATION_PATH             = "svc/artifactstore/artifacts/{0}/navigationPath";

                    public static class Discussions_id_
                    {
                        public const string REPLIES                 = "svc/artifactstore/artifacts/{0}/discussions/{1}/replies";
                    }
                }

                public static class Baselines_id_
                {
                    public const string CONTENT                     = "svc/bpartifactstore/baselines/{0}/content"; //  Add artifact to Baseline
                }

                public static class Collections_id_
                {
                    public const string CONTENT                     = "svc/bpartifactstore/collections/{0}/content"; //  Add artifact to Collection
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

                public static class Reviews_id_
                {
                    public const string CONTENT                     = "svc/artifactstore/containers/{0}/content";
                }

                public static class Status
                {
                    public const string UPCHECK                     = "svc/artifactstore/status/upcheck";
                }
            }

            [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
            public static class Components
            {
                public static class FileStore
                {
                    /// <summary>
                    /// Path to upload files to FileStore.  {0} = Filename.
                    /// </summary>
                    public const string FILES_filename_             = "svc/components/filestore/files/{0}";
                }

                [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
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
                        public const string DELETE_COMMENT_id_      = "svc/components/RapidReview/artifacts/{0}/deletecomment/{1}";
                        public const string DELETE_THREAD_id_       = "svc/components/RapidReview/artifacts/{0}/deletethread/{1}";
                        public const string DISCUSSIONS             = "svc/components/RapidReview/artifacts/{0}/discussions";

                        public static class Discussions_id_
                        {
                            public const string COMMENT             = "svc/components/RapidReview/artifacts/{0}/discussions/{1}";
                            public const string REPLY               = "svc/components/RapidReview/artifacts/{0}/discussions/{1}/reply";
                            public const string REPLY_id_           = "svc/components/RapidReview/artifacts/{0}/discussions/{1}/reply/{2}";
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

                public const string NOVAFILE_id_                    = "svc/bpfilestore/file/{0}";

                public static class Status
                {
                    public const string UPCHECK                     = "svc/filestore/status/upcheck";
                }
            }

            [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]   // Ignore this warning.
            public static class SearchService
            {
                public const string FULLTEXTSEARCH = "svc/searchservice/itemsearch/fulltext";

                public const string STATUS = "svc/searchservice/status";

                public static class FullTextSearch
                {
                    public const string METADATA = "svc/searchservice/itemsearch/fulltextmetadata";
                }

                public static class Status
                {
                    public const string UPCHECK = "svc/searchservice/status/upcheck";
                }

                public const string PROJECTSEARCH = "svc/searchservice/projectsearch/name";
                public const string ITEMNAMESEARCH = "svc/searchservice/itemsearch/name";
            }

            public static class Shared
            {
                public static class Artifacts
                {
                    public const string DISCARD                 = "svc/shared/artifacts/discard";
                    public const string LOCK                    = "svc/shared/artifacts/lock";
                    public const string PUBLISH                 = "svc/shared/artifacts/publish";
                    public const string SEARCH                  = "svc/shared/artifacts/search";
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
