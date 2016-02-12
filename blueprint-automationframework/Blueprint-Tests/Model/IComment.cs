﻿namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")] // Ignore this warning.
    public interface IComment
    {

    }

    public interface IOpenApiComment : IComment
    {
        string LastModified { get; set; }
        bool IsClosed { get; set; }
        string Status { get; set; }
        int Id { get; set; }
        IAuthor Author { get; set; }
        int Version { get; set; }
        string Description { get; set; }
    }
}
