using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Common;
using Newtonsoft.Json;
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

        public IProcessShape AddUserTask(ProcessLink processLink)
        {
            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a user task
            // Using non-default values to ensure values are saved
            var userTask = CreateUserTask("NewUser", "Objective", null, null, 120.0, 160.0, 5, 5);
            Shapes.Add((ProcessShape)userTask);

            // Add a system task to be paired with the user task just created
            // Using non-default values to ensure values are saved
            var systemTask = CreateSystemTask(null, "NewSystem", "Objective", null, null, 120.0, 160.0, 5, 10);
            Shapes.Add((ProcessShape)systemTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            // Note: Maintains existing order index
            processLink.DestinationId = userTask.Id;

            // Add a new link between the new user task and the new system task
            Links.Add(new ProcessLink
            {
                DestinationId = systemTask.Id,
                Label = null,
                Orderindex = 1,
                SourceId = userTask.Id 
            });

            // Add a new link between the new system task and the destination
            Links.Add(new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = 1,
                SourceId = systemTask.Id
            });

            return userTask;
        }

        public IProcessShape AddUserDecisionPoint(ProcessLink processLink)
        {
            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a user decision point
            // Using non-default values to ensure values are saved
            var userDecisionPoint = CreateUserDecisionPoint("Objective", null, 120.0, 155.0, 10, 10);
            Shapes.Add((ProcessShape)userDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            // Note: Maintains existing order index
            processLink.DestinationId = userDecisionPoint.Id;

            // Add a new link after the new user decision point
            Links.Add(new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = 1,
                SourceId = userDecisionPoint.Id
            });

            return userDecisionPoint;
        }

        public IProcessShape AddUserDecisionPointWithBranchBeforeShape(int idOfNextShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            // Find the incoming link for the next shape
            var processLink = GetIncomingLinkForShape(idOfNextShape);

            // Determine the artifact Id of the branch end point
            int branchEndPointId;

            if (idOfBranchMergePoint == null)
            {
                // branch endpoint is the process link destination id if the branch merge point is null
                branchEndPointId = processLink.DestinationId;
            }
            else
            {
                // branch endpoint id the id of the branch merge point if the branch merge point is not null
                branchEndPointId = (int)idOfBranchMergePoint;
            }

            // Add user decision point before next shape
            var userDecisionPoint = AddUserDecisionPoint(processLink);

            // Find outgoing process link for new user decision point
            var newprocesslink = GetOutgoingLinkForShape(userDecisionPoint.Id);

            // Add user/system task immediately after user decision point if next shape is the end shape or a user decision point
            if (idOfNextShape == GetProcessShapeByShapeName(EndName).Id || GetProcessShapeTypeById(idOfNextShape) == ProcessShapeType.UserDecision)
            {
                // Add new user/system task to branch
                AddUserTask(newprocesslink);
            }

            // Add new branch to user decision point
            AddBranchWithUserTaskToUserDecisionPoint(userDecisionPoint.Id, orderIndexOfBranch, branchEndPointId);

            return userDecisionPoint;
        }

        public IProcessShape AddUserDecisionPointWithBranchAfterShape(int idOfPreviousShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            // Find the outgoing link for the previous shape
            var processLink = GetOutgoingLinkForShape(idOfPreviousShape);

            // Determine the artifact Id of the branch end point
            int branchEndPointId;

            if (idOfBranchMergePoint == null)
            {
                // branch endpoint is the process link destination id if the branch merge point is null
                branchEndPointId = processLink.DestinationId;
            }
            else
            {
                // branch endpoint id the id of the branch merge point if the branch merge point is not null
                branchEndPointId = (int)idOfBranchMergePoint;
            }

            // Add user decision point after the previous shape
            var userDecisionPoint = AddUserDecisionPoint(processLink);

            // Find outgoing process link for new user decision point
            var newprocesslink = GetOutgoingLinkForShape(userDecisionPoint.Id);

            // Add user/system task immediately after user decision point only if next shape is the end shape
            if (newprocesslink.DestinationId == GetProcessShapeByShapeName(EndName).Id)
            {
                // Add new user/system task to branch
                AddUserTask(newprocesslink);
            }

            // Add new branch to user decision point
            AddBranchWithUserTaskToUserDecisionPoint(userDecisionPoint.Id, orderIndexOfBranch, branchEndPointId);

            return userDecisionPoint;
        }

        public IProcessShape AddBranchWithUserTaskToUserDecisionPoint(int decisionPointId, double orderIndex, int destinationId)
        {
            // Add a process link to the user decision point
            var processLink = AddLink(decisionPointId, destinationId, orderIndex);

            // Add a user task to the branch and return the user task shape object
            return AddUserTask(processLink);
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
        /// <returns>A new user task</returns>
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
        /// <returns>A new system task</returns>
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
        /// <param name="itemLabel">The item label of the user task</param>
        /// <param name="associatedArtifact">The include of the user task</param>
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <returns>A new user decision point</returns>
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
