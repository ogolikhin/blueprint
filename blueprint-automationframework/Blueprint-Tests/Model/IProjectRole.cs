namespace Model
{
    public interface IProjectRole
    {
        #region Properties
        int RoleId { get; set; }
        
        int ProjectId { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        RolePermissions Permissions { get; set; }

        bool IsDeleted { get; set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Adds role to the Database. Updates RoleId with id of newly created record in [dbo].[Roles].
        /// </summary>
        void AddRoleToDatabase();

        /// <summary>
        /// Deletes role. Set Deleted column value to 1 for the specified role.
        /// All RoleAssignment must be deleted to avoid inconsistency in Blueprint work!
        /// </summary>
        void DeleteRole();
        #endregion Methods
    }
}
