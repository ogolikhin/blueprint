﻿using Model.Impl;
using Utilities;
using Utilities.Factories;

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
        public static IProjectRole CreateProjectRole(IProject project, RolePermissions permissions, string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (name == null)
            {
                name = RandomGenerator.RandomAlphaNumeric(7);
            }

            var description = RandomGenerator.RandomAlphaNumeric(10);
            var role = new ProjectRole(project.Id, name, description, permissions);
            role.AddRoleToDatabase();
            return role;
        }

        /// <summary>
        /// These roles id are from DB - it will work only for project with Id=1 (what we have in DB after restoring from backup).
        /// </summary>
        public enum DeployedProjectRole
        {
            None = 0,
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
            ThrowIf.ArgumentNull(deployedRole, nameof(deployedRole));
            var role = new ProjectRole(1, "Author", string.Empty, RolePermissions.Edit);
            role.RoleId = (int)deployedRole;
            return role;
        }
    }
}
