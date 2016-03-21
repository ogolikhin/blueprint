using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Common;
using Model.OpenApiModel;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;

namespace Model.StorytellerModel.Impl
{
    /// <summary>
    /// The Storyteller Process Model
    /// </summary>
    public class Process: IProcess
    {
        #region Constants

        public const string StartName = "Start";

        public const string DefaultPreconditionName = "Precondition";

        public const string DefaultUserTaskName = "<Start with a verb, i.e. select, run, view>";

        public const string DefaultSystemTaskName = "<Start with a verb, i.e. display, print, calculate>";

        public const string EndName = "End";


        private const double DefaultOrderIndex = 0;

        private static readonly string Description = PropertyTypePredefined.Description.ToString();

        private static readonly string Label = PropertyTypePredefined.Label.ToString();

        private static readonly string X = PropertyTypePredefined.X.ToString();

        private static readonly string Y = PropertyTypePredefined.Y.ToString();

        private static readonly string Height = PropertyTypePredefined.Height.ToString();

        private static readonly string Width = PropertyTypePredefined.Width.ToString();

        private static readonly string ClientType = PropertyTypePredefined.ClientType.ToString();

        private const string Persona = "Persona";

        private const string AssociatedImageUrl = "AssociatedImageUrl";

        private const string StoryLinks = "StoryLinks";

        private const string ItemLabel = "ItemLabel";

        private const string LinkLabels = "LinkLabels";

        public const string ImageId = "ImageId";

        private const string SystemTaskNamePrefix = "ST";

        private const string UserTaskNamePrefix = "UT";

        private const string ProcessShapeTypePrefix = "PROS";

        private const string UserDecisionNamePrefix = "UD";

        private const string SystemDecisionNamePrefix = "SD";

        #endregion Constants

        #region Private Properties

        private int _tempId;

        #endregion Private Properties

        #region Public Properties

        public int ProjectId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string TypePrefix { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ProcessShape>>))]
        public List<ProcessShape> Shapes { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ProcessLink>>))]
        public List<ProcessLink> Links { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ArtifactPathLink>>))]
        public List<ArtifactPathLink> ArtifactPathLinks { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        #endregion Public Properties

        #region Constructors

        public Process()
        {
            Shapes = new List<ProcessShape>();
            Links = new List<ProcessLink>();
            ArtifactPathLinks = new List<ArtifactPathLink>();
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        #endregion Constructors

        #region Public Methods

        public IProcessShape AddUserAndSystemTask(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[??]--

            It becomes this:
                --[??]--+--[UT]--+--[ST]--+--[??]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Add a user task
            var userTask = AddUserTask(processLink);

            var userLink = GetOutgoingLinkForShape(userTask.Id);

            // Add a system task to be paired with the user task just created
            AddSystemTask(userLink);

            return userTask;
        }

        public IProcessShape AddUserDecisionPointWithBranchBeforeShape(int idOfNextShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            /*
            If you start with this:
                --[UT]--+--[ST]--

            It becomes this:
                --<UD>--+--[UT]--+--[ST]--+--
                   |                      |
                   +-------[UT]--+--[ST]--+
            */

            // Find the incoming link for the next shape
            var processLink = GetIncomingLinkForShape(idOfNextShape);

            // Add user decision point before next shape
            var userDecisionPoint = AddUserDecisionPoint(processLink);

            // Add a branch to user decision point
            AddBranchToUserDecisionPoint(orderIndexOfBranch, idOfBranchMergePoint, userDecisionPoint);

            return userDecisionPoint;
        }

        public IProcessShape AddUserDecisionPointWithBranchAfterShape(int idOfPreviousShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            /*
            If you start with this:
                --[??]--+--

            It becomes this:
                --[??]--+--<UD>--+--[UT]--+--[ST]--+--
                            |                      |
                            +-------[UT]--+--[ST]--+
            */

            // Find the outgoing link for the previous shape
            var outgoingLinkForPreviousShape = GetOutgoingLinkForShape(idOfPreviousShape);

            var shapeAfterNewUserDecisionPoint = GetProcessShapeTypeById(outgoingLinkForPreviousShape.DestinationId);

            Assert.That(shapeAfterNewUserDecisionPoint != ProcessShapeType.UserDecision, "A user decision point cannot be inserted before an existing user decision point");

            // Add user decision point after the previous shape
            var userDecisionPoint = AddUserDecisionPoint(outgoingLinkForPreviousShape);

            // Add a branch to user decision point
            AddBranchToUserDecisionPoint(orderIndexOfBranch, idOfBranchMergePoint, userDecisionPoint);

            return userDecisionPoint;
        }

        public IProcessShape AddSystemDecisionPointWithBranchBeforeSystemTask(int idOfNextSystemTaskShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            /*
            If you start with this:
                --[UT]--+--[ST]--+--

            It becomes this:
                --[UT]--+--<SD>--+--[ST]--+--
                            |             |
                            +-------[ST]--+
            */

            // Find the outgoing link for the next system taskshape
            var outgoingProcessLink = GetOutgoingLinkForShape(idOfNextSystemTaskShape);

            // Determine the artifact Id of the branch end point
            int branchEndPointId = idOfBranchMergePoint ?? outgoingProcessLink.DestinationId;

            // Find the incoming link for the next system taskshape
            var incomingProcessLink = GetIncomingLinkForShape(idOfNextSystemTaskShape);

            // Add user decision point before next shape
            var systemDecisionPoint = AddSystemDecisionPoint(incomingProcessLink);

            // Add new branch to system decision point
            AddBranchWithSystemTaskToSystemDecisionPoint(systemDecisionPoint.Id, orderIndexOfBranch, branchEndPointId);

            return systemDecisionPoint;
        }

        public void AddBranchWithSystemTaskToSystemDecisionPoint(int decisionPointId, double orderIndex, int destinationId)
        {
            /*
            If you start with this:
                --[UT]--+--<SD>--+--[ST]--+--

            It becomes this:
                --[UT]--+--<SD>--+--[ST]--+--
                            |             |
                            +-------[ST]--+
            */

            // Add a process link to the system decision point
            var processLink = AddLink(decisionPointId, destinationId, orderIndex);

            // Add a system task to the branch and return the system task shape object
            AddSystemTask(processLink);
        }


        public IProcessShape AddBranchWithUserAndSystemTaskToUserDecisionPoint(int decisionPointId, double orderIndex, int destinationId)
        {
            /*
            If you start with this:
                --+--<UD>--+--[UT]--+--[ST]--+--

            It becomes this:
                --+--<UD>--+--[UT]--+--[ST]--+--
                      |                      |
                      +-------[UT]--+--[ST]--+
            */

            // Add a process link to the user decision point
            var processLink = AddLink(decisionPointId, destinationId, orderIndex);

            // Add a user task to the branch and return the user task shape object
            return AddUserAndSystemTask(processLink);
        }

        public ProcessLink AddLink(int sourceId, int destinationId, double orderIndex)
        {
            // Create a process link
            var processLink = new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
                SourceId = sourceId
            };

            // Add the process link to the list of links in the process
            Links.Add(processLink);

            return processLink;
        }

        public List<IProcessShape> GetProcessShapesByShapeType(ProcessShapeType processShapeType)
        {
            return Shapes.FindAll(p => (Convert.ToInt32(p.PropertyValues[PropertyTypeName.clientType.ToString()].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(processShapeType, CultureInfo.CurrentCulture))).ConvertAll(o => (IProcessShape)o);
        }

        public ProcessLink GetIncomingLinkForShape(int shapeId)
        {
            // Find the incoming link for the process shape
            var link = Links.ToList().Find(l => l.DestinationId == shapeId);

            return link;
        }

        public ProcessLink GetOutgoingLinkForShape(int shapeId)
        {
            // Find the outgoing link for the process shape
            var link = Links.ToList().Find(l => l.SourceId == shapeId);

            return link;
        }

        public IProcessShape GetNextShape(IProcessShape shape)
        {
            // Find the outgoing link for the process shape
            var outgoingLink = Links.ToList().Find(l => l.SourceId == shape.Id);

            // Return the next shape which is located via the link destination Id
            return GetProcessShapeById(outgoingLink.DestinationId);
        }

        public IProcessShape GetProcessShapeByShapeName(string shapeName)
        {
            // Find the process shape by the process shape name
            var shape = Shapes.ToList().Find(s => s.Name == shapeName);

            return shape;
        }

        public IProcessShape GetProcessShapeById(int shapeId)
        {
            // Find the process shape by the process shape artifact Id
            var shape = Shapes.ToList().Find(s => s.Id == shapeId);

            return shape;
        }

        public ProcessShapeType GetProcessShapeTypeById(int shapeId)
        {
            // Find the process shape by the process shape artifact Id
            var shape = Shapes.ToList().Find(s => s.Id == shapeId);

            // Get the property information value for the shape type
            var clientTypePropertyInformation =
                shape.PropertyValues.ToList()
                    .Find(p => string.Equals(p.Key, ClientType, StringComparison.CurrentCultureIgnoreCase))
                    .Value;

            // Get the integer representation of the process shape type
            var shapeType = Convert.ToInt32(clientTypePropertyInformation.Value, CultureInfo.InvariantCulture);

            // Return the process shape type
            return (ProcessShapeType)shapeType;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Create a User Task
        /// </summary>
        /// <param name="persona">The persona of the user task</param>
        /// <param name="itemLabel">The item label of the user task</param>
        /// <param name="associatedArtifact">The include of the user task</param>
        /// <param name="imageId">The id of the image in the user task</param>
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns>The new user task</returns>
        private IProcessShape CreateUserTask(string persona, string itemLabel, ArtifactPathLink associatedArtifact, int? imageId, double width, double height, int x, int y, int storyLinkId = 0)
        {
            // Create a user task
            var userTask = CreateProcessShape(ProcessShapeType.UserTask, UserTaskNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            // Create a story link for the user task if the story link Id was not 0
            var storyLink = storyLinkId == 0 ? null : new StoryLink(userTask.Id, storyLinkId, 0, storyLinkId);

            userTask.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(Persona),
                    Value = persona
                }
                );

            userTask.PropertyValues.Add(ImageId,
                new PropertyValueInformation
                {
                    PropertyName = ImageId,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(ImageId),
                    Value = imageId
                }
                );

            userTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(StoryLinks),
                    Value = storyLink
                }
                );

            return userTask;
        }

        /// <summary>
        /// Create a System Task
        /// </summary>
        /// <param name="associatedImageUrl">The url of the system task image</param>
        /// <param name="persona">The persona of the user task</param>
        /// <param name="itemLabel">The item label of the user task</param>
        /// <param name="associatedArtifact">The include of the user task</param>
        /// <param name="imageId">The id of the image in the system task</param>
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns>The new system task</returns>
        private IProcessShape CreateSystemTask(string associatedImageUrl, string persona, string itemLabel, ArtifactPathLink associatedArtifact, int? imageId, double width, double height, int x, int y, int storyLinkId = 0)
        {
            // Create a system task
            var systemTask = CreateProcessShape(ProcessShapeType.SystemTask, SystemTaskNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            // Create a story link for the system task if the story link Id was not 0
            var storyLink = storyLinkId == 0 ? null : new StoryLink(systemTask.Id, storyLinkId, 0, storyLinkId);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(AssociatedImageUrl),
                    Value = associatedImageUrl
                });


            systemTask.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(Persona),
                    Value = persona
                }
                );

            systemTask.PropertyValues.Add(ImageId,
                new PropertyValueInformation
                {
                    PropertyName = ImageId,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(ImageId),
                    Value = imageId
                }
                );

            systemTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(StoryLinks),
                    Value = storyLink
                }
                );

            return systemTask;
        }

        /// <summary>
        /// Create a User Decision Point
        /// </summary>
        /// <param name="itemLabel">The item label of the user decision point</param>
        /// <param name="associatedArtifact">The include of the user decision point</param>
        /// <param name="width">The width of the user decision point</param>
        /// <param name="height">The height of the user decision point</param>
        /// <param name="x">The x coordinate of the user decision pointtask</param>
        /// <param name="y">The y coordinate of the user decision point</param>
        /// <returns>The new user decision point</returns>
        private IProcessShape CreateUserDecisionPoint(string itemLabel, ArtifactPathLink associatedArtifact, double width, double height, int x, int y)
        {
            // Create a user decision point
            var userDecisionPoint = CreateProcessShape(ProcessShapeType.UserDecision, UserDecisionNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            userDecisionPoint.PropertyValues.Add(LinkLabels,
                new PropertyValueInformation
                {
                    PropertyName = LinkLabels,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(LinkLabels),
                    Value = null
                });

            return userDecisionPoint;
        }

        /// <summary>
        /// Create a System Decision Point
        /// </summary>
        /// <param name="itemLabel">The item label of the system decision point</param>
        /// <param name="associatedArtifact">The include of the system decision point</param>
        /// <param name="width">The width of the system decision point</param>
        /// <param name="height">The height of the system decision point</param>
        /// <param name="x">The x coordinate of the system decision pointtask</param>
        /// <param name="y">The y coordinate of the system decision point</param>
        /// <returns>The new system decision point</returns>
        private IProcessShape CreateSystemDecisionPoint(string itemLabel, ArtifactPathLink associatedArtifact, double width, double height, int x, int y)
        {
            // Create a system decision point
            var systemDecisionPoint = CreateProcessShape(ProcessShapeType.SystemDecision, SystemDecisionNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            systemDecisionPoint.PropertyValues.Add(LinkLabels,
                new PropertyValueInformation
                {
                    PropertyName = LinkLabels,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(LinkLabels),
                    Value = null
                });

            return systemDecisionPoint;
        }

        /// <summary>
        /// Create a Generic Process Shape
        /// </summary>
        /// <param name="processShapeType">The type of the process shape</param>
        /// <param name="shapeNamePrefix">The prefix for both the shape name and the shape label</param>
        /// <param name="itemLabel">The item label of the process shape</param>
        /// <param name="associatedArtifact">The user story artifact associated with the Process shape</param>
        /// <param name="width">The width of the process shape</param>
        /// <param name="height">The height of the process shape</param>
        /// <param name="x">The x coordinate of the process shape</param>
        /// <param name="y">The y coordinate of the process shape</param>
        /// <returns></returns>
        private IProcessShape CreateProcessShape(ProcessShapeType processShapeType, string shapeNamePrefix, string itemLabel, ArtifactPathLink associatedArtifact, double width, double height, int x, int y)
        {
            IProcessShape processShape = new ProcessShape();

            processShape.BaseItemTypePredefined = ItemTypePredefined.PROShape;
            // New process shapes require a unique negative ID before being sent to the backend
            // by the Storyteller REST API method 'UpdateProcess'
            processShape.Id = --_tempId;
            processShape.Name = shapeNamePrefix + Math.Abs(processShape.Id);
            processShape.ParentId = Id;
            processShape.ProjectId = ProjectId;
            processShape.TypePrefix = ProcessShapeTypePrefix;
            processShape.AssociatedArtifact = (ArtifactPathLink)associatedArtifact;

            processShape.PropertyValues.Add(ClientType,
                new PropertyValueInformation
                {
                    PropertyName = ClientType,
                    TypePredefined = PropertyTypePredefined.ClientType,
                    TypeId = GetPropertyNameTypeId(ClientType),
                    Value = (int)processShapeType
                });

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = GetPropertyNameTypeId(Description),
                    // Create a random description
                    Value = AddDivTags(RandomGenerator.RandomValueWithPrefix(Description, 4))
                });

            processShape.PropertyValues.Add(Height,
                new PropertyValueInformation
                {
                    PropertyName = Height,
                    TypePredefined = PropertyTypePredefined.Height,
                    TypeId = GetPropertyNameTypeId(Height),
                    Value = height
                });

            // This is also known as Objective
            processShape.PropertyValues.Add(ItemLabel,
                new PropertyValueInformation
                {
                    PropertyName = ItemLabel,
                    TypePredefined = PropertyTypePredefined.ItemLabel,
                    TypeId = GetPropertyNameTypeId(ItemLabel),
                    Value = itemLabel + " for " + processShape.Name
                }
                );

            processShape.PropertyValues.Add(Label,
                new PropertyValueInformation
                {
                    PropertyName = Label,
                    TypePredefined = PropertyTypePredefined.Label,
                    TypeId = GetPropertyNameTypeId(Label),
                    Value = processShape.Name
                }
                );

            processShape.PropertyValues.Add(Width,
                new PropertyValueInformation
                {
                    PropertyName = Width,
                    TypePredefined = PropertyTypePredefined.Width,
                    TypeId = GetPropertyNameTypeId(Width),
                    Value = width
                }
                );

            processShape.PropertyValues.Add(X,
                new PropertyValueInformation
                {
                    PropertyName = X,
                    TypePredefined = PropertyTypePredefined.X,
                    TypeId = GetPropertyNameTypeId(X),
                    Value = x
                }
                );

            processShape.PropertyValues.Add(Y,
                new PropertyValueInformation
                {
                    PropertyName = Y,
                    TypePredefined = PropertyTypePredefined.Y,
                    TypeId = GetPropertyNameTypeId(Y),
                    Value = y
                }
                );

            return processShape;
        }

        /// <summary>
        /// Add a User Decision Point to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user decision point will be added</param>
        /// <returns>The user decision point that was added</returns>
        private IProcessShape AddUserDecisionPoint(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[UT]--

            It becomes this:
                --[??]--+--<UD>--+--[UT]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a user decision point
            // Using non-default values to ensure values are saved
            var userDecisionPoint = CreateUserDecisionPoint("Objective", null, 120.0, 155.0, 10, 10);
            Shapes.Add((ProcessShape)userDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new user decision so
            // that the destination now points to the new user decision
            // Note: Maintains existing order index
            processLink.DestinationId = userDecisionPoint.Id;

            // Add a new link after the new user decision point
            AddLink(sourceId: userDecisionPoint.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            return userDecisionPoint;
        }

        /// <summary>
        /// Add a System Decision Point to the Process
        /// </summary>
        /// <param name="processLink">The process link where the system decision point will be added</param>
        /// <returns>The system decision point that was added</returns>
        private IProcessShape AddSystemDecisionPoint(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[UT]--+--[ST]--

            It becomes this:
                --[UT]--+--<SD>--+--[ST]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a system decision point
            // Using non-default values to ensure values are saved
            var systemDecisionPoint = CreateSystemDecisionPoint("Objective", null, 120.0, 155.0, 10, 10);
            Shapes.Add((ProcessShape)systemDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new system decision so
            // that the destination now points to the new system decision
            // Note: Maintains existing order index
            processLink.DestinationId = systemDecisionPoint.Id;

            // Add a new link after the new system decision point
            AddLink(sourceId: systemDecisionPoint.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            return systemDecisionPoint;
        }

        /// <summary>
        /// Add a System Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the system task will be added</param>
        /// <returns>The new System Task that was added</returns>
        private IProcessShape AddSystemTask(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[??]--

            It becomes this:
                --[??]--+--[ST]--+--[??]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a system task
            // Using non-default values to ensure values are saved
            var systemTask = CreateSystemTask(null, "NewSystem", "Objective", null, null, 120.0, 160.0, 5, 10);
            Shapes.Add((ProcessShape)systemTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            // Note: Maintains existing order index
            processLink.DestinationId = systemTask.Id;

            // Add a new link between the new system task and the destination
            AddLink(sourceId: systemTask.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            return systemTask;
        }

        /// <summary>
        /// Add a User Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user task will be added</param>
        /// <returns>The new User Task that was added</returns>
        private IProcessShape AddUserTask(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[??]--

            It becomes this:
                --[??]--+--[UT]--+--[??]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a user task
            // Using non-default values to ensure values are saved
            var userTask = CreateUserTask("NewUser", "Objective", null, null, 120.0, 160.0, 5, 5);
            Shapes.Add((ProcessShape)userTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            // Note: Maintains existing order index
            processLink.DestinationId = userTask.Id;

            // Add a new link between the new user task and the destination
            AddLink(sourceId: userTask.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            return userTask;
        }

        /// <summary>
        /// Add a Branch with User/System Tasks to a User Decision Point
        /// </summary>
        /// <param name="orderIndexOfBranch">The vertical order index of the branch</param>
        /// <param name="idOfBranchMergePoint">The id of the shape where the branch merges</param>
        /// <param name="userDecisionPoint">The user decision that will receive the new branch</param>
        private void AddBranchToUserDecisionPoint(
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint,
            IProcessShape userDecisionPoint)
        {
            // Find outgoing process link for new user decision point
            var linkAferUserDecisionPoint = GetOutgoingLinkForShape(userDecisionPoint.Id);

            // Find process shape immediately after the added user decision point
            var processShapeAfterUserDecisionPoint = GetProcessShapeTypeById(linkAferUserDecisionPoint.DestinationId);

            // Add user/system task immediately after user decision point only if next shape is the end shape
            // or another user decision
            if (processShapeAfterUserDecisionPoint == ProcessShapeType.End ||
                processShapeAfterUserDecisionPoint == ProcessShapeType.UserDecision)
            {
                /*  Special case:
                If next shape is (End):                         If next shape is <UD>:
                    --<UD>--+--(End)                                --<UD>--+--<UD>---

                It becomes this:                                It becomes this:
                    --+--<UD>--+--[UT]--+--[ST]--+--(End)           --+--<UD>--+--[UT]--+--[ST]--+--<UD>--
                          |                      |                        |                      |
                          +-------[UT]--+--[ST]--+                        +-------[UT]--+--[ST]--+
                */

                // Add new user/system task to branch
                AddUserAndSystemTask(linkAferUserDecisionPoint);

                // Find updated outgoing process link for user decision point
                linkAferUserDecisionPoint = GetOutgoingLinkForShape(userDecisionPoint.Id);

                // Find process shape immediately after the added user decision from the udated link
                // after the added user decision point
                processShapeAfterUserDecisionPoint = GetProcessShapeTypeById(linkAferUserDecisionPoint.DestinationId);
            }

            // Get the branch merge point following the user/system task combination only if
            // the id of the branch merge point was not defined in the passed parameter and
            // the following process shape is a user task
            if (idOfBranchMergePoint == null && processShapeAfterUserDecisionPoint == ProcessShapeType.UserTask)
            {
                var userTaskShape = GetProcessShapeById(linkAferUserDecisionPoint.DestinationId);
                var systemTaskShape = GetNextShape(userTaskShape);
                var shapeAfterSystemTaskShape = GetNextShape(systemTaskShape);

                idOfBranchMergePoint = shapeAfterSystemTaskShape.Id;
            }

            Assert.NotNull(idOfBranchMergePoint, "The Id of the branch merge point is null.");

            // Add a branch with a user/system task to user decision point
            AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecisionPoint.Id, orderIndexOfBranch,
                    (int)idOfBranchMergePoint);
        }

        /// <summary>
        /// get the Property Name Type Id
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The type id of the property (returns null if no such property type was found)</returns>
        private int? GetPropertyNameTypeId(string propertyName)
        {
            // Must convert first character of property name to lowercase in order to find the property in the 
            // default process
            propertyName = propertyName.LowerCaseFirstCharacter();

            // Find the property with name propertyName
            var property = Shapes.Find(shape => shape.PropertyValues.ContainsKey(propertyName));

            // Return the property type Id if found, otherwise return null
            return property?.PropertyValues[propertyName].TypeId;
        }

        /// <summary>
        /// Add Div Tags to Text
        /// </summary>
        /// <param name="plainTextString">The plain text string to be modified</param>
        /// <returns>The plain text string surrounded by DIV tags</returns>
        private static string AddDivTags(string plainTextString)
        {
            string formatString = "<div>{0}</div>";

            return I18NHelper.FormatInvariant(formatString, plainTextString);
        }

        #endregion Private Methods
    }

    public class ProcessShape: IProcessShape
    {
        private const string StorytellerProcessPrefix = "SP";

        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int ProjectId { get; set; }

        public string TypePrefix { get; set; }

        public ArtifactPathLink AssociatedArtifact { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        public ProcessShape()
        {
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        public ArtifactPathLink AddAssociatedArtifact(IOpenApiArtifact artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            AssociatedArtifact = new ArtifactPathLink()
            {
                BaseItemTypePredefined = artifact.BaseItemTypePredefined,
                Id = artifact.Id,
                Link = null,
                Name = artifact.Name,
                ProjectId = artifact.ProjectId,
                TypePrefix = StorytellerProcessPrefix
            };

            return AssociatedArtifact;
        }
    }

    public class ProcessLink
    {
        /// <summary>		
        /// Source Id for the process link		
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>		
        /// Destination Id for the process link		
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>		
        /// Order index for the process link (Order in which the links are drawn for decision points)		
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>		
        /// Label for the process link		
        /// </summary>
        public string Label { get; set; }
    }

    public class ArtifactPathLink
    {
        /// <summary>
        /// The Id of the Artifact
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Project Id for the artifact
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The name of the artifact
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        public string TypePrefix { get; set; }

        /// <summary>
        /// The base item type for the artifact
        /// </summary>
        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The link to navigate to the artifact
        /// </summary>
        public string Link { get; set; }
    }

    public class PropertyValueInformation
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The predefined property type
        /// </summary>
        public PropertyTypePredefined TypePredefined { get; set; }

        /// <summary>
        /// Property Type Id as defined in the blueprint project metadata
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// The value of the property
        /// </summary>
        public object Value { get; set; }
    }

    public class StoryLink
    {
        /// <summary>
        /// The Artifact Id of referenced User Story
        /// </summary>
        public int AssociatedReferenceArtifactId { get; set; }

        /// <summary>
        /// The Destination Id of the Story Link
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// The vertical order index
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>
        /// The Source Id of the Story Link
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Storylink Constructor
        /// </summary>
        /// <param name="sourceId">The source id of the story link</param>
        /// <param name="destinationId">The destination id of the story link</param>
        /// <param name="orderIndex">The vertical order index</param>
        /// <param name="associatedReferenceId">The artifact id of referenced user story</param>
        public StoryLink(int sourceId, int destinationId, double orderIndex, int associatedReferenceId)
        {
            AssociatedReferenceArtifactId = associatedReferenceId;
            DestinationId = destinationId;
            Orderindex = orderIndex;
            SourceId = sourceId;
        }
    }

    public class LinkLabels
    {
        /// <summary>
        /// The Id of the Process Link
        /// </summary>
        public int LinkId { get; set; }

        /// <summary>
        /// The Label of the Process Link
        /// </summary>
        public string Label { get; set; }
    }
}
