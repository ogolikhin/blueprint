using System;

namespace ServiceLibrary.Models.Enums
{
    [Flags]
    public enum MessageActionType
    {
        None = 0,
        PropertyChange = 1,
        Notification = 2,
        GenerateChildren = 4,
        GenerateTests = 8,
        GenerateUserStories = 16,
        StateChange = 32,
        ArtifactsPublished = 64,
        All = PropertyChange | Notification | GenerateChildren | GenerateTests | GenerateUserStories | StateChange | ArtifactsPublished
    }
}
