using Model.Impl;
using Utilities.Factories;
using Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Model.Factories
{
    public static class ProjectRoleFactory
    {
        /// <summary>
        /// Creates new ProjectRole and adds it to the Database.
        /// </summary>
        /// <param name="project">Project where role will be created.</param>
        /// <param name="permissions">Permissions for new role. Use | to combine permissions.</param>
        /// <param name="name">(Optional) The name of the role. By default a random value is used.</param>
        /// <returns>The created Role.</returns>
        public static IProjectRole CreateProjectRole (IProject project, RolePermissions permissions, string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            string groupName;
            if (name == null)
            {
                groupName = RandomGenerator.RandomAlphaNumeric(7);
            }
            else
            {
                groupName = name;
            }
            string description = RandomGenerator.RandomAlphaNumeric(10);
            IProjectRole role = new ProjectRole(project.Id, groupName, description, permissions);
            role.AddRoleToDatabase();
            return role;
        }

        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
        public enum DeployedProjectRole
        {//these roles id are from DB - it will work only for project with Id=1 (what we have in DB after restoring from backup)
            Author = 1,
            Collaborator = 2,
            ProjectAdministrator = 3,
            Viewer = 4
        }

        /// <summary>
        /// Gets ProjectRole for the project with id=1. These roles we have after restore from backup.
        /// </summary>
        /// <param name="deployedRole">Role(Author/Collaborator/ProjectAdministrator/Viewer) to return.</param>
        /// <returns>The Role from DB. The only valid field is RoleId.</returns>
        public static IProjectRole GetDeployedProjectRole(DeployedProjectRole deployedRole)
        {
            //need to refactor!!!
            //it will work only for project with Id=1 (what we have in DB after restoring from backup)
            IProjectRole role = new ProjectRole(1, "Author", string.Empty, RolePermissions.Edit);
            switch (deployedRole)
            {
                case DeployedProjectRole.Author:
                    {
                        role.RoleId = 1;
                        return role;
                    }
                case DeployedProjectRole.Collaborator:
                    {
                        role.RoleId = 2;
                        return role;
                    }
                case DeployedProjectRole.ProjectAdministrator:
                    {
                        role.RoleId = 3;
                        return role;
                    }
                case DeployedProjectRole.Viewer:
                    {
                        role.RoleId = 4;
                        return role;
                    }
                default:
                    {
                        return role;
                    }
            }
        }
    }
}
