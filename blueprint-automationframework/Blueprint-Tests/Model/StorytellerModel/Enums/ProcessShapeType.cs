namespace Model.StorytellerModel.Enums
{
    /// <summary>
    /// Enumeration of Process Shape Types
    /// 
    /// Found in: blueprint-current\Source\BluePrintSys.RC.CrossCutting.Portable\Enums\ProcessShapeType.cs
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