namespace Model.StorytellerModel.Enums
{
    /// <summary>
    /// Enumeration of Process Shape Types
    /// </summary>
    public enum ProcessShapeType
    {
        None = 0,
        Start = 1,
        UserTask = 2,
        End = 3,
        SystemTask = 4,
        PreconditionSystemTask = 5,
        UserDecision = 6,
        SystemDecision = 7
    }
}