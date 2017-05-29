using Model.Common.Enums;

namespace Model.InstanceAdminModel
{
    /// <summary>
    /// This class represents a row in the [Raptor].[dbo].[InstanceAdminRoles] table.
    /// </summary>
    public class CustomInstanceAdminRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InstanceAdminPrivileges Permissions { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
