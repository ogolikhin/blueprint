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
        void AddRoleToDatabase();

        void DeleteRole();
        #endregion Methods
    }
}
