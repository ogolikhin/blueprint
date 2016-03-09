using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Utilities;
using Utilities.Factories;

namespace Model.Impl
{
    public class Process: IProcess
    {
        #region Constants

        public const string StartName = "Start";

        public const string DefaultPreconditionName = "Precondition";

        public const string DefaultUserTaskName = "User Task 1";

        public const string DefaultSystemTaskName = "System Task 1";

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

        public IProcessShape AddUserTask(IProcessLink processLink)
        {
            ThrowIf.ArgumentNull(processLink, nameof(processLink));

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
            UpdateDestinationIdOfLink(processLink, userTask.Id);

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

        public IProcessShape AddUserDecisionPoint(IProcessLink processLink)
        {
            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            var destinationId = processLink.DestinationId;

            // Using non-default values to ensure values are saved
            var userDecisionPoint = CreateUserDecisionPoint("Objective", null, 120.0, 155.0, 10, 10);
            Shapes.Add((ProcessShape)userDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            UpdateDestinationIdOfLink(processLink, userDecisionPoint.Id);

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

        public IProcessShape AddDecisionPointWithBranchBeforeShape(int idOfNextShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            // Find the incoming link for the next shape
            var processLink = FindIncomingLinkForShape(idOfNextShape);

            int branchEndPointId;

            if (idOfBranchMergePoint == null)
            {
                branchEndPointId = processLink.DestinationId;
            }
            else
            {
                branchEndPointId = (int)idOfBranchMergePoint;
            }

            // Add user decision point before next shape
            var userDecisionPoint = AddUserDecisionPoint(processLink);

            // Find outgoing process link for new user decision point
            var newprocesslink = FindOutgoingLinkForShape(userDecisionPoint.Id);

            // Add user/system task immediately after user decision point if next shape is the end shape or a user decision point
            if (idOfNextShape == FindProcessShapeByShapeName(EndName).Id || FindProcessShapeTypeById(idOfNextShape) == ProcessShapeType.UserDecision)
            {
                // Add new user/system task to branch
                AddUserTask(newprocesslink);
            }

            // Add new branch to user decision point
            AddBranchWithUserTaskToDecisionPoint(userDecisionPoint.Id, orderIndexOfBranch, branchEndPointId);

            return userDecisionPoint;
        }

        public IProcessShape AddDecisionPointWithBranchAfterShape(int idOfPreviousShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null)
        {
            // Find the outgoing link for the previous shape
            var processLink = FindOutgoingLinkForShape(idOfPreviousShape);

            int branchEndPointId;

            if (idOfBranchMergePoint == null)
            {
                branchEndPointId = processLink.DestinationId;
            }
            else
            {
                branchEndPointId = (int)idOfBranchMergePoint;
            }

            // Add user decision point after the previous shape
            var userDecisionPoint = AddUserDecisionPoint(processLink);

            // Find outgoing process link for new user decision point
            var newprocesslink = FindOutgoingLinkForShape(userDecisionPoint.Id);

            // Add user/system task immediately after user decision point only if next shape is the end shape
            if (newprocesslink.DestinationId == FindProcessShapeByShapeName(EndName).Id)
            {
                // Add new user/system task to branch
                AddUserTask(newprocesslink);
            }

            // Add new branch to user decision point
            AddBranchWithUserTaskToDecisionPoint(userDecisionPoint.Id, orderIndexOfBranch, branchEndPointId);

            return userDecisionPoint;
        }

        public IProcessShape AddBranchWithUserTaskToDecisionPoint(int decisionPointId, double orderIndex, int destinationId)
        {
            var processLink = AddLink(decisionPointId, destinationId, orderIndex);

            return AddUserTask(processLink);
        }

        public IProcessLink AddLink(int sourceId, int destinationId, double orderIndex)
        {
            var processLink = new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
                SourceId = sourceId
            };

            Links.Add(processLink);

            return processLink;
        }

        public IProcessLink FindIncomingLinkForShape(int shapeId)
        {
            var link = Links.ToList().Find(l => l.DestinationId == shapeId);

            return link;
        }

        public IProcessLink FindOutgoingLinkForShape(int shapeId)
        {
            var link = Links.ToList().Find(l => l.SourceId == shapeId);

            return link;
        }

        public IProcessShape FindProcessShapeByShapeName(string shapeName)
        {
            var shape = Shapes.ToList().Find(s => s.Name == shapeName);

            return shape;
        }

        public IProcessShape FindProcessShapeById(int shapeId)
        {
            var shape = Shapes.ToList().Find(s => s.Id == shapeId);

            return shape;
        }

        public ProcessShapeType FindProcessShapeTypeById(int shapeId)
        {
            var shape = Shapes.ToList().Find(s => s.Id == shapeId);

            var clientTypePropertyInformation =
                shape.PropertyValues.ToList()
                    .Find(p => string.Equals(p.Key, ClientType, StringComparison.CurrentCultureIgnoreCase))
                    .Value;

            var shapeType = Convert.ToInt32(clientTypePropertyInformation.Value, CultureInfo.InvariantCulture);

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
        private IProcessShape CreateUserTask(string persona, string itemLabel, IArtifactPathLink associatedArtifact, int? imageId, double width, double height, int x, int y, int storyLinkId = 0)
        {
            var userTask = CreateProcessShape(ProcessShapeType.UserTask, UserTaskNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            var storyLink = storyLinkId == 0 ? null : new StoryLink(userTask.Id, storyLinkId, 0, storyLinkId);

            userTask.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(Persona),
                    Value = persona
                }
                );

            userTask.PropertyValues.Add(ImageId,
                new PropertyValueInformation
                {
                    PropertyName = ImageId,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(ImageId),
                    Value = imageId
                }
                );

            userTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(StoryLinks),
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
        private IProcessShape CreateSystemTask(string associatedImageUrl, string persona, string itemLabel, IArtifactPathLink associatedArtifact, int? imageId, double width, double height, int x, int y, int storyLinkId = 0)
        {
            var systemTask = CreateProcessShape(ProcessShapeType.SystemTask, SystemTaskNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            var storyLink = storyLinkId == 0 ? null : new StoryLink(systemTask.Id, storyLinkId, 0, storyLinkId);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(AssociatedImageUrl),
                    Value = associatedImageUrl
                });


            systemTask.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(Persona),
                    Value = persona
                }
                );

            systemTask.PropertyValues.Add(ImageId,
                new PropertyValueInformation
                {
                    PropertyName = ImageId,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(ImageId),
                    Value = imageId
                }
                );

            systemTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(StoryLinks),
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
        private IProcessShape CreateUserDecisionPoint(string itemLabel, IArtifactPathLink associatedArtifact, double width, double height, int x, int y)
        {
             var userDecisionPoint = CreateProcessShape(ProcessShapeType.UserDecision, UserDecisionNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            userDecisionPoint.PropertyValues.Add(LinkLabels,
                new PropertyValueInformation
                {
                    PropertyName = LinkLabels,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(LinkLabels),
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
        private IProcessShape CreateProcessShape(ProcessShapeType processShapeType, string shapeNamePrefix, string itemLabel, IArtifactPathLink associatedArtifact, double width, double height, int x, int y)
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
                    TypeId = FindPropertyNameTypeId(ClientType),
                    Value = (int)processShapeType
                });

            

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = FindPropertyNameTypeId(Description),
                    // Create a random description
                    Value = AddHtmlTags(RandomGenerator.RandomValueWithPrefix(Description, 4))
                });

            processShape.PropertyValues.Add(Height,
                new PropertyValueInformation
                {
                    PropertyName = Height,
                    TypePredefined = PropertyTypePredefined.Height,
                    TypeId = FindPropertyNameTypeId(Height),
                    Value = height
                });

            // This is also known as Objective
            processShape.PropertyValues.Add(ItemLabel,
                new PropertyValueInformation
                {
                    PropertyName = ItemLabel,
                    TypePredefined = PropertyTypePredefined.ItemLabel,
                    TypeId = FindPropertyNameTypeId(ItemLabel),
                    Value = itemLabel + " for " + processShape.Name
                }
                );

            processShape.PropertyValues.Add(Label,
                new PropertyValueInformation
                {
                    PropertyName = Label,
                    TypePredefined = PropertyTypePredefined.Label,
                    TypeId = FindPropertyNameTypeId(Label),
                    Value = processShape.Name
                }
                );

            processShape.PropertyValues.Add(Width,
                new PropertyValueInformation
                {
                    PropertyName = Width,
                    TypePredefined = PropertyTypePredefined.Width,
                    TypeId = FindPropertyNameTypeId(Width),
                    Value = width
                }
                );

            processShape.PropertyValues.Add(X,
                new PropertyValueInformation
                {
                    PropertyName = X,
                    TypePredefined = PropertyTypePredefined.X,
                    TypeId = FindPropertyNameTypeId(X),
                    Value = x
                }
                );

            processShape.PropertyValues.Add(Y,
                new PropertyValueInformation
                {
                    PropertyName = Y,
                    TypePredefined = PropertyTypePredefined.Y,
                    TypeId = FindPropertyNameTypeId(Y),
                    Value = y
                }
                );

            return processShape;
        }

        /// <summary>
        /// Find Property Name Type Id
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The type id of the property</returns>
        private int? FindPropertyNameTypeId(string propertyName)
        {
            // Must convert first charater of property name to lowercase in order to find the pproperty in the 
            // default process
            propertyName = char.ToLower(propertyName[0], CultureInfo.InvariantCulture) + propertyName.Substring(1);

            var property = Shapes.Find(shape => shape.PropertyValues.ContainsKey(propertyName));

            return property?.PropertyValues[propertyName].TypeId;
        }

        /// <summary>
        /// Update Destination Id of Link
        /// </summary>
        /// <param name="processLink">The process lioink to update</param>
        /// <param name="newDestinationId">The new destination id of the link</param>
        private void UpdateDestinationIdOfLink(IProcessLink processLink, int newDestinationId)
        {
            var link = (ProcessLink) processLink;

            Links.Remove(link);

            processLink.DestinationId = newDestinationId;

            Links.Add(link);
        }

        /// <summary>
        /// Add HTML Tags to Text
        /// </summary>
        /// <param name="plainTextString">The plain text string to be modified</param>
        /// <returns>The plain text string surrounded by HTML tags</returns>
        private static string AddHtmlTags(string plainTextString)
        {
            string formatString = "<html>{0}</html>";

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

        public IArtifactPathLink AddAssociatedArtifact(IOpenApiArtifact artifact)
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

    public class ProcessLink: IProcessLink
    {
        public int SourceId { get; set; }

        public int DestinationId { get; set; }

        public double Orderindex { get; set; }

        public string Label { get; set; }
    }

    public class ArtifactPathLink : IArtifactPathLink
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Name { get; set; }

        public string TypePrefix { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        public string Link { get; set; }
    }

    public class PropertyValueInformation : IPropertyValueInformation
    {
        public string PropertyName { get; set; }

        public PropertyTypePredefined TypePredefined { get; set; }

        public int? TypeId { get; set; }

        public object Value { get; set; }
    }

    public class StoryLink : IStoryLink
    {
        public int AssociatedReferenceArtifactId { get; set; }

        public int DestinationId { get; set; }

        public double Orderindex { get; set; }

        public int SourceId { get; set; }

        public StoryLink(int sourceId, int destinationId, double orderIndex, int associatedReferenceId)
        {
            AssociatedReferenceArtifactId = associatedReferenceId;
            DestinationId = destinationId;
            Orderindex = orderIndex;
            SourceId = sourceId;
        }
    }

    public class LinkLabels : ILinkLabelInfo
    {
        public int LinkId { get; set; }
        public string Label { get; set; }
    }
}
