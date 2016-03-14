using System.Collections.Generic;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{
    public enum ProcessType
    {
        None = 0,
        BusinessProcess = 1,
        UserToSystemProcess = 2,
        SystemToSystemProcess = 3
    }

    public enum PropertyTypeName
    {
        associatedImageUrl,
        clientType,
        description,
        height,
        imageId,
        itemLabel,
        label,
        persona,
        storyLinks,
        width,
        x,
        y
    }
    public enum PropertyTypePredefined
    {
        None = 0,
        ID = 4097,
        Name = 4098,
        Description = 4099,
        UseCaseLevel = 4100,
        ReadOnly = 4101,
        ItemLabel = 4102,
        RowLabel = 4103,
        ColumnLabel = 4104,
        DataObjectType = 4105,
        ExtensionType = 4106,
        Condition = 4107,
        BPObjectType = 4108,
        WidgetType = 4109,
        ReturnToStepName = 4110,
        RawData = 4111,
        ThreadStatus = 4112,
        ApprovalStatus = 4113,
        ClientType = 4114,
        Label = 4115,
        SharedViewPreferences = 4116,
        ValueType = 4117,
        IsSealedPublished = 4118,
        ALMIntegrationSettings = 4119,
        ALMExportInfo = 4120,
        ALMSecurity = 4121,
        StepOf = 4122,
        DataOperationSet = 4123,
        CreatedBy = 4124,
        CreatedOn = 4125,
        LastEditedBy = 4126,
        LastEditedOn = 4127,

        X = 8193,
        Y = 8194,
        Width = 8195,
        Height = 8196,
        ConnectorType = 8197,
        TruncateText = 8198,
        BackgroundColor = 8199,
        BorderColor = 8200,
        BorderWidth = 8201,
        Image = 8202,
        Orientation = 8203,
        ClientRawData = 8204,
        Theme = 8205,
        Thumbnail = 8206,
        CustomGroup = 16384
    }

    public interface IProcess
    {
        #region Properties

        /// <summary>
        /// Project containing the Process
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// Artifact Id for the process
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name for the process
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Prefix of the process type
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// Base item type for the process artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// Sub-artifact shapes for the process
        /// </summary>
        List<ProcessShape> Shapes { get; }

        /// <summary>
        /// Sub-artifact links for the process
        /// </summary>
        List<ProcessLink> Links { get; }

        /// <summary>
        /// Artifact path links for the Process.  This supports breadcrumb navigation
        /// </summary>
        List<ArtifactPathLink> ArtifactPathLinks { get; }

        /// <summary>
        /// The property values for the Process artifact
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }
        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a User Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user task will be added</param>
        /// <returns>The user task that was added</returns>
        IProcessShape AddUserTask(ProcessLink processLink);

        /// <summary>
        /// Adds a User Decision Point to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user decision point will be added</param>
        /// <returns>The user decision point that was added</returns>
        IProcessShape AddUserDecisionPoint(ProcessLink processLink);

        /// <summary>
        /// Adds a Branch to a Decision Point
        /// </summary>
        /// <param name="decisionPointId">Id of the decision point</param>
        /// <param name="orderIndex">Order index of the added branch</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <returns>The user task created</returns>
        IProcessShape AddBranchWithUserTaskToDecisionPoint(int decisionPointId, double orderIndex, int destinationId);

        /// <summary>
        /// Adds a Link to a Process
        /// </summary>
        /// <param name="sourceId">The source Id of the link</param>
        /// <param name="destinationId">The destination Id of the link</param>
        /// <param name="orderIndex">The order index of the link</param>
        /// <returns></returns>
        ProcessLink AddLink(int sourceId, int destinationId, double orderIndex);

        /// <summary>
        /// Adds a User Decision Point with a Branch After an Existing Shape
        /// </summary>
        /// <param name="idOfPreviousShape">The Id of the shape before the insertion point</param>
        /// <param name="orderIndexOfBranch">The order index of the added branch</param>
        /// <param name="idOfBranchMergePoint">The of the shape where the branch terminates</param>
        /// <returns></returns>
        IProcessShape AddUserDecisionPointWithBranchAfterShape(int idOfPreviousShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null);

        /// <summary>
        /// Adds a User Decision Point with a Branch Before an Existing Shape
        /// </summary>
        /// <param name="idOfNextShape">The Id of the shape after the insertion point</param>
        /// <param name="orderIndexOfBranch">the order index of the added branch</param>
        /// <param name="idOfBranchMergePoint">The of the shape where the branch terminates</param>
        /// <returns></returns>
        IProcessShape AddUserDecisionPointWithBranchBeforeShape(int idOfNextShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null);

        /// <summary>
        /// Find the Process Shape by the Shape Name
        /// </summary>
        /// <param name="shapeName">The shape name</param>
        /// <returns>The process shape</returns>
        IProcessShape GetProcessShapeByShapeName(string shapeName);

        /// <summary>
        /// Find the Process Shape by the Artifact Id of the Shape
        /// </summary>
        /// <param name="shapeId">The artifact id of the shape</param>
        /// <returns>The process shape</returns>
        IProcessShape GetProcessShapeById(int shapeId);

        /// <summary>
        /// Find the Process Shape Type by the Artifact Id of the Shape
        /// </summary>
        /// <param name="shapeId">The artifact id of the shape</param>
        /// <returns>The process shape type</returns>
        ProcessShapeType GetProcessShapeTypeById(int shapeId);

        /// <summary>
        /// Find the Incoming Process Link for a Shape
        /// </summary>
        /// <param name="shapeId">The shape id</param>
        /// <returns>The process link</returns>
        ProcessLink GetIncomingLinkForShape(int shapeId);

        /// <summary>
        /// Find the Outgoing Process Link for a Shape
        /// </summary>
        /// <param name="shapeId">The shape id</param>
        /// <returns>The process link</returns>
        ProcessLink GetOutgoingLinkForShape(int shapeId);

        #endregion Methods
    }
}
