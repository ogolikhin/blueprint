using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// copy of blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/RolePermissions.cs
    /// </summary>
    [Flags]
    public enum RolePermissions : long
    {
        /// <summary>
        /// No privileges
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Allows the viewing of an artifact
        /// </summary>
        Read = 0x1,

        /// <summary>
        /// Allows the editing of an artifact. This includes deleting & adding children.
        /// </summary>
        Edit = 0x2,

        /// <summary>
        /// Allows deleting an artifact.
        /// </summary>
        Delete = 0x40,

        /// <summary>
        /// Allow tracing from/To an artifact project.
        /// In addition to having the correct trace To/From Privileges on the two artifacts being a user must have edit privileges on at least one of the related artifacts .
        /// </summary>
        Trace = 0x4,

        /// <summary>
        /// Allow the user to comment on an artifact.
        /// </summary>
        Comment = 0x8,

        /// <summary>
        /// Allows a user to steal a lock on artifacts.
        /// </summary>
        StealLock = 0x10,

        // Do not use old ProjectAdmin flag
        //ProjectAdmin = 0x20,

        /// <summary>
        /// Allows a user to report on the project.
        /// </summary>
        CanReport = 0x80,

        /// <summary>
        /// Allows a user to share an artifact.
        /// </summary>
        Share = 0x100,

        /// <summary>
        /// Allow reuse traces from/To an artifact project.
        /// In addition to having the correct trace To/From Privileges on the two artifacts being a user must have edit privileges on at least one of the related artifacts .
        /// </summary>
        Reuse = 0x200,

        /// <summary>
        /// Allows a user to perform Excel Update.
        /// </summary>
        ExcelUpdate = 0x400,

        /// <summary>
        /// Allow the user to delete someone else's comment on an artifact.
        /// </summary>
        DeleteAnyComment = 0x800,

        /// <summary>
        /// Allow the user to create/edit/save rapid review
        /// </summary>
        CreateRapidReview = 0x1000
    }
}
