using System;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Flags]
    public enum MessageActionType
    {
        None = 0,
        Property = 1,
        Notification = 2,
        GenerateDescendants = 4,
        GenerateTests = 8,
        GenerateUserStories = 16,
        All = Property | Notification | GenerateDescendants | GenerateTests | GenerateUserStories
    }
}
