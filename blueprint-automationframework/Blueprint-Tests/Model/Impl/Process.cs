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

        public IProcessShape AddUserTask(int sourceId, int destinationId)
        {
            // Add a user task
            var userTask = CreateUserTask("User", "", 0, null, 126.0, 150.0, 0, 0);
            Shapes.Add((ProcessShape)userTask);

            // Add a system task to be paired with the user task just created
            var systemTask = CreateSystemTask("", "User", "", 0, null, 126.0, 150.0, 0, 0);
            Shapes.Add((ProcessShape)systemTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            UpdateDestinationIdOfLink(sourceId, destinationId, userTask.Id);

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

        public IProcessShape AddUserDecisionPoint(int sourceId, int destinationId)
        {
            var userDecisionPoint = CreateUserDecisionPoint("User", "", 0, 126.0, 150.0, 0, 0);
            Shapes.Add((ProcessShape)userDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            UpdateDestinationIdOfLink(sourceId, destinationId, userDecisionPoint.Id);

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

        public void AddBranch(int sourceId, int destinationId, double orderIndex)
        {
            // Adds a new link to decision point
            Links.Add(new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
                SourceId = sourceId
            });
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
        private IProcessShape CreateUserTask(string persona, string itemLabel, int associatedArtifact, int? imageId, double width, double height, int x, int y, int storyLinkId = 0)
        {
            const string userTaskNamePrefix = "UT";

            var userTask = CreateProcessShape(ProcessShapeType.UserTask, userTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y);

            var storyLink = storyLinkId == 0 ? null : CreateStoryLink(userTask.Id, storyLinkId, 0, storyLinkId);

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
        private IProcessShape CreateSystemTask(string associatedImageUrl, string persona, string itemLabel, int associatedArtifact, int? imageId, double width, double height, int x, int y, int storyLinkId = 0)
        {
            const string systemTaskNamePrefix = "ST";

            var systemTask = CreateProcessShape(ProcessShapeType.SystemTask, systemTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y);

            var storyLink = storyLinkId == 0 ? null : CreateStoryLink(systemTask.Id, storyLinkId, 0, storyLinkId);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(AssociatedImageUrl),
                    Value = associatedImageUrl
                });

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
        /// <param name="persona">The persona of the user task</param>
        /// <param name="itemLabel">The item label of the user task</param>
        /// <param name="associatedArtifact">The include of the user task</param>
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <returns>A new user decision point</returns>
        private IProcessShape CreateUserDecisionPoint(string persona, string itemLabel, int associatedArtifact, double width, double height, int x, int y)
        {
            const string userTaskNamePrefix = "UD";

            var userDecisionPoint = CreateProcessShape(ProcessShapeType.UserDecision, userTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y);

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
        /// <param name="persona">The persona of the process shape</param>
        /// <param name="itemLabel">The item label of the process shape</param>
        /// <param name="associatedArtifact">The user story artifact associated with the Process shape</param>
        /// <param name="width">The width of the process shape</param>
        /// <param name="height">The height of the process shape</param>
        /// <param name="x">The x coordinate of the process shape</param>
        /// <param name="y">The y coordinate of the process shape</param>
        /// <returns></returns>
        private IProcessShape CreateProcessShape(ProcessShapeType processShapeType, string shapeNamePrefix, string persona, string itemLabel, int associatedArtifact, double width, double height, int x, int y)
        {
            const string processShapeTypePrefix = "PROS";

            IProcessShape processShape = new ProcessShape();

            processShape.BaseItemTypePredefined = ItemTypePredefined.PROShape;
            // New process shapes require a unique negative ID before being sent to the backend
            // by the Storyteller REST API method 'UpdateProcess'
            processShape.Id = --_tempId;
            processShape.Name = shapeNamePrefix + Math.Abs(processShape.Id);
            processShape.ParentId = Id;
            processShape.ProjectId = ProjectId;
            processShape.TypePrefix = processShapeTypePrefix;
            processShape.AssociatedArtifact = associatedArtifact;

            processShape.PropertyValues.Add(ClientType,
                new PropertyValueInformation
                {
                    PropertyName = ClientType,
                    TypePredefined = PropertyTypePredefined.ClientType,
                    TypeId = FindPropertyNameTypeId(ClientType),
                    Value = processShapeType
                });

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = FindPropertyNameTypeId(Description),
                    // Create a random description
                    Value = RandomGenerator.RandomValueWithPrefix(Description, 4)
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
                    Value = itemLabel
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

            processShape.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(Persona),
                    Value = persona
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
            propertyName = propertyName.Substring(0, 1).ToLower(CultureInfo.CurrentCulture) + propertyName.Substring(1);
            var property = Shapes.Find(shape => shape.PropertyValues.ContainsKey(propertyName));

            return property?.PropertyValues[propertyName].TypeId;
        }

        /// <summary>
        /// Update Destination Id of Link
        /// </summary>
        /// <param name="sourceId">The source id of the link</param>
        /// <param name="originalDestinationId">The original destination id of the link</param>
        /// <param name="newDestinationId">The new destination id of the link</param>
        private void UpdateDestinationIdOfLink(int sourceId, int originalDestinationId, int newDestinationId)
        {
            var processLink = Links.ToList()
                .Find(l => l.SourceId == sourceId && l.DestinationId == originalDestinationId);

            Links.Remove(processLink);

            processLink.DestinationId = newDestinationId;

            Links.Add(processLink);
        }

        /// <summary>
        /// Create a Story Link
        /// </summary>
        /// <param name="sourceId">The source id of the link</param>
        /// <param name="destinationId">The destination id of the link</param>
        /// <param name="orderIndex">The order index of the story link</param>
        /// <param name="associatedReferenceId">The user story artifact id</param>
        /// <returns>The story link</returns>
        private static IStoryLink CreateStoryLink(int sourceId, int destinationId, double orderIndex, int associatedReferenceId)
        {
            return new StoryLink
            {
                AssociatedReferenceArtifactId = associatedReferenceId,
                DestinationId = destinationId,
                Orderindex = orderIndex,
                SourceId = sourceId
            };
        }

        #endregion Private Methods
    }

    public class ProcessShape: IProcessShape
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int ProjectId { get; set; }

        public string TypePrefix { get; set; }

        public int? AssociatedArtifact { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }


        public ProcessShape()
        {
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
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
    }

    public class LinkLabels : ILinkLabelInfo
    {
        public int LinkId { get; set; }
        public string Label { get; set; }
    }
}
