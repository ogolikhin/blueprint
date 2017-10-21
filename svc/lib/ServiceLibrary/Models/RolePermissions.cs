using System;

namespace ServiceLibrary.Models
{
    // This enum has been copied from the Raptor solution.
    [Flags]
    public enum RolePermissions : long
    {
        // No privileges
        None = 0x0,

        // Allows the viewing of an artifact
        Read = 0x1,

        // Allows the editing of an artifact. This includes deleting & adding children.
        Edit = 0x2,

        // Allows deleting an artifact.
        Delete = 0x40,

        // Allow tracing from/To an artifact project.
        // In addition to having the correct trace To/From Privileges on the two artifacts being a user must have edit privileges on at least one of the related artifacts .
        Trace = 0x4,

        // Allow the user to comment on an artifact.
        Comment = 0x8,

        // Allows a user to steal a lock on artifacts.
        StealLock = 0x10,

        // Do not use old ProjectAdmin flag
        // ProjectAdmin = 0x20,

        // Allows a user to report on the project.
        CanReport = 0x80,

        // Allows a user to share an artifact.
        Share = 0x100,

        // Allow reuse traces from/To an artifact project.
        // In addition to having the correct trace To/From Privileges on the two artifacts being a user must have edit privileges on at least one of the related artifacts .
        Reuse = 0x200,

        // Allows a user to perform Excel Update.
        ExcelUpdate = 0x400,

        // Allow the user to delete someone else's comment on an artifact.
        DeleteAnyComment = 0x800,

        // Allow the user to create/edit/save rapid review
        CreateRapidReview = 0x1000
    }
}