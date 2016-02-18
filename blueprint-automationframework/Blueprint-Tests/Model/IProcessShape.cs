using System.Runtime.Serialization;

namespace Model
{
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

    public interface IProcessShape
    {
        #region Properties

        /// <summary>
        /// Sub artifact Id for the shape
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the shape
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Parent Id of the shape
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Label for the shape
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Description for the shape
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Shape type for the shape
        /// </summary>
        ProcessShapeType ShapeType { get; set; }

        /// <summary>
        /// X coordinate for the shape
        /// </summary>
        double X { get; set; }

        /// <summary>
        /// Y coordinate for the shape
        /// </summary>
        double Y { get; set; }

        /// <summary>
        /// Width of the shape
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of the shape
        /// </summary>
        double Height { get; set; }

        #endregion Properties
    }
}
