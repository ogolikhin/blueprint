using Model.Impl;
using Utilities.Factories;
using Utilities;

namespace Model.Factories
{
    public static class ProjectRoleFactory
    {
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
    }
}
