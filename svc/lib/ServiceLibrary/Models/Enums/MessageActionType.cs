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
        ArtifactsChanged = 128,
        ProjectsChanged = 256,
        UsersGroupsChanged = 512,
        WorkflowsChanged = 1024,
        PropertyItemTypesChanged = 2048,
        StatusCheck = 4096,
        All = PropertyChange | Notification | GenerateChildren | GenerateTests | GenerateUserStories | StateChange | ArtifactsPublished | ArtifactsChanged | ProjectsChanged | UsersGroupsChanged | WorkflowsChanged | PropertyItemTypesChanged | StatusCheck
    }
}
