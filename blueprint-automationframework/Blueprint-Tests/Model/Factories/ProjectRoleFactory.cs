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
        /// <param name="project">(Optional) The name of the group.  By default a random value is used.</param>
        /// <param name="permissions">(Optional) The E-mail address of the group.  By default a random value is used.</param>
        /// <param name="name">(Optional) The description of the group.  By default a random value is used.</param>
        /// <returns>The created Group.</returns>
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
        {
            Author = 1,
            Collaborator = 2,
            ProjectAdministrator = 3,
            Viewer = 4
        }

        public static IProjectRole GetDeployedProjectRole(DeployedProjectRole deployedRole)
        {
            //need to refactor!!!
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
