using System.Linq;
using System.Net.Http;
using System.Web.Http;
using AdminStore.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;

namespace AdminStore
{
    [TestClass]
    public class WebApiConfigTests
    {
        [TestMethod]
        public void Register_Always_RegistersCorrectRoutes()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertTotalRoutes(76, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<ConfigController>("GetConfigSettings", HttpMethod.Get, "config/settings");
            config.AssertAction<ConfigController>("GetConfig", HttpMethod.Get, "config/config.js");
            config.AssertAction<ConfigController>("GetApplicationSettings", HttpMethod.Get, "config");
            config.AssertAction<ConfigController>("GetUserManagementSettings", HttpMethod.Get, "config/users");
            config.AssertAction<LicensesController>("GetLicenseTransactions", HttpMethod.Get, "licenses/transactions?days=1");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions?login=admin");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions?login=admin&force=true");
            config.AssertAction<SessionsController>("PostSessionSingleSignOn", HttpMethod.Post, "sessions/sso");
            config.AssertAction<SessionsController>("PostSessionSingleSignOn", HttpMethod.Post, "sessions/sso?force=true");
            config.AssertAction<SessionsController>("DeleteSession", HttpMethod.Delete, "sessions");
            config.AssertAction<SessionsController>("IsSessionAlive", HttpMethod.Get, "sessions/alive");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
            config.AssertAction<InstanceController>("GetInstanceFolder", HttpMethod.Get, "instance/folders/1");
            config.AssertAction<InstanceController>("GetInstanceFolderChildren", HttpMethod.Get, "instance/folders/1/children");
            config.AssertAction<InstanceController>("GetInstanceProject", HttpMethod.Get, "instance/projects/1");
            config.AssertAction<InstanceController>("GetProjectNavigationPath", HttpMethod.Get, "instance/projects/1/navigationPath");
            config.AssertAction<InstanceController>("GetInstanceRoles", HttpMethod.Get, "instance/roles");
            config.AssertAction<InstanceController>("CreateFolder", HttpMethod.Post, "instance/folder");
            config.AssertAction<JobsController>("GetLatestJobs", HttpMethod.Get, "jobs/");
            config.AssertAction<JobsController>("GetJob", HttpMethod.Get, "jobs/1");
            config.AssertAction<JobsController>("GetJobResultFile", HttpMethod.Get, "jobs/1/result/file");
            config.AssertAction<JobsController>("QueueGenerateProcessTestsJob", HttpMethod.Post, "jobs/process/testgen");
            config.AssertAction<UsersController>("GetLoginUser", HttpMethod.Get, "users/loginuser");
            config.AssertAction<UsersController>("GetUserIcon", HttpMethod.Get, "users/1/icon");
            config.AssertAction<UsersController>("PostRequestPasswordResetAsync", HttpMethod.Post, "users/passwordrecovery/request");
            config.AssertAction<UsersController>("PostPasswordResetAsync", HttpMethod.Post, "users/passwordrecovery/reset");
            config.AssertAction<UsersController>("GetUser", HttpMethod.Get, "users/1");
            config.AssertAction<UsersController>("GetUsers", HttpMethod.Get, "users?offset=0&limit=20&sort=login&order=asc");
            config.AssertAction<UsersController>("CreateUser", HttpMethod.Post, "users");
            config.AssertAction<UsersController>("UpdateUser", HttpMethod.Put, "users/1");
            config.AssertAction<UsersController>("PostReset", HttpMethod.Post, "users/reset?login=admin");
            config.AssertAction<UsersController>("DeleteUsers", HttpMethod.Post, "users/delete");
            config.AssertAction<UsersController>("InstanceAdminChangePassword", HttpMethod.Post, "users/changepassword");
            config.AssertAction<UsersController>("GetUserGroups", HttpMethod.Get, "/users/1074/groups?offset=0&limit=1&sort=name&order=desc&search=test");
            config.AssertAction<UsersController>("DeleteUserFromGroups", HttpMethod.Post, "/users/1074/groups");
            config.AssertAction<UsersController>("AddUserToGroups", HttpMethod.Put, "users/10/groups");
            config.AssertAction<GroupsController>("GetGroups", HttpMethod.Get, "groups?userid=10&offset=0&limit=1&sort=name&order=desc&search=test");
            config.AssertAction<GroupsController>("DeleteGroups", HttpMethod.Post, "/groups/delete");
            config.AssertAction<GroupsController>("CreateGroup", HttpMethod.Post, "groups");
            config.AssertAction<GroupsController>("GetGroup", HttpMethod.Get, "/groups/1");
            config.AssertAction<GroupsController>("UpdateGroup", HttpMethod.Put, "/groups/1");
            config.AssertAction<GroupsController>("GetGroupsAndUsers", HttpMethod.Get, "/groups/3/usersgroups?offset=0&limit=20&sort=email&order=asc");
            config.AssertAction<GroupsController>("GetGroupMembers", HttpMethod.Get, "/groups/10/members?offset=0&limit=20&sort=email&order=asc");
            config.AssertAction<GroupsController>("RemoveMembersFromGroup", HttpMethod.Post, "/groups/10/members");
            config.AssertAction<GroupsController>("AssignMembers", HttpMethod.Post, "/groups/1/assign?search=test");
            config.AssertAction<WorkflowController>("ImportWorkflowAsync", HttpMethod.Post, "workflow/import");
            config.AssertAction<WorkflowController>("GetImportWorkflowErrorsAsync", HttpMethod.Get, "workflow/import/errors?guid=abc");
            config.AssertAction<WorkflowController>("GetWorkflow", HttpMethod.Get, "workflow/1");
            config.AssertAction<WorkflowController>("GetWorkflows", HttpMethod.Get, "workflow?offset=0&limit=20&sort=name&order=asc");
            config.AssertAction<WorkflowController>("DeleteWorkflows", HttpMethod.Post, "/workflow/delete");
            config.AssertAction<WorkflowController>("UpdateStatus", HttpMethod.Put, "workflow/1/status");
            config.AssertAction<WorkflowController>("ExportWorkflow", HttpMethod.Get, "workflow/export/1");
            config.AssertAction<WorkflowController>("GetWorkflowAvailableProjects", HttpMethod.Get, "workflow/1/folders/1/availablechildren");
            config.AssertAction<WorkflowController>("GetProjectArtifactsAssignedtoWorkflowAsync", HttpMethod.Get, "workflow/1/projects/?offset=0&limit=20");
            config.AssertAction<WorkflowController>("AssignProjectsAndArtifactsToWorkflow", HttpMethod.Post, "workflow/1/assign");
            config.AssertAction<InstanceController>("SearchFolderByName", HttpMethod.Get, "instance/foldersearch?name=test");
            config.AssertAction<InstanceController>("DeleteInstanceFolder", HttpMethod.Delete, "instance/folders/1");
            config.AssertAction<InstanceController>("UpdateInstanceFolder", HttpMethod.Put, "instance/folders/1");
            config.AssertAction<WorkflowController>("UpdateWorkflowViaImport", HttpMethod.Put, "workflow/update/1");
            config.AssertAction<InstanceController>("DeleteProject", HttpMethod.Delete, "instance/projects/1");
            config.AssertAction<InstanceController>("GetProjectAdminPermissions", HttpMethod.Get, "instance/projects/1/privileges");
            config.AssertAction<InstanceController>("GetProjectRolesAsync", HttpMethod.Get, "instance/projects/1/roles");
            config.AssertAction<InstanceEmailSettingsController>("SendTestEmail", HttpMethod.Post, "instance/emailsettings/sendtestemail");
            config.AssertAction<InstanceEmailSettingsController>("TestConnection", HttpMethod.Post, "instance/emailsettings/testconnection");
            config.AssertAction<InstanceController>("GetProjectRoleAssignments", HttpMethod.Get, "instance/projects/1/rolesassignments");
            config.AssertAction<InstanceController>("DeleteRoleAssignment", HttpMethod.Post, "instance/projects/1/rolesassignments/delete");
            config.AssertAction<InstanceController>("CreateRoleAssignment", HttpMethod.Post, "instance/projects/1/rolesassignments");
            config.AssertAction<InstanceEmailSettingsController>("GetEmailSettings", HttpMethod.Get, "instance/emailsettings");
            config.AssertAction<InstanceController>("SearchProjectFolder", HttpMethod.Get, "instance/folderprojectsearch?offset=0&limit=20");
            config.AssertAction<InstanceController>("UpdateRoleAssignment", HttpMethod.Put, "instance/projects/1/rolesassignments/2");
            config.AssertAction<WorkflowController>("CreateWorkflow", HttpMethod.Post, "workflow/create");
            config.AssertAction<WorkflowController>("UnassignProjectsAndArtifactTypesFromWorkflowAsync", HttpMethod.Post, "workflow/1/unassign");
            config.AssertAction<WorkflowController>("AssignArtifactTypesToProjectInWorkflow", HttpMethod.Post, "workflow/1/project/1/assign");
        }

        [TestMethod]
        public void Register_GetAndHeadMethods_HaveNoCacheAttribute()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertMethodAttributes(attr => attr.Any(a => a is HttpGetAttribute || a is HttpHeadAttribute) == attr.Any(a => a is NoCacheAttribute),
                "{0} is missing NoCacheAttribute.");
        }

        [TestMethod]
        public void Register_AllHttpMethods_HaveSessionRequiredOrNoSessionRequiredAttribute()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertMethodAttributes(attr => attr.Any(a => a is SessionAttribute || a is NoSessionRequiredAttribute),
                "{0} is missing SessionAttribute or NoSessionRequiredAttribute.");
        }
    }
}
