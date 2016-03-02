using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography.X509Certificates;
using Common;
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

        private const string OutputParameters = "OutputParameters";

        private const string UserTaskId = "UserTaskId";

        private const string StoryLinks = "StoryLinks";

        private const string InputParameters = "InputParameters";

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

        public IProcessShape AddUserTask(int sourceId, int destinationId, double orderIndex)
        {
            // Add a user task
            var userTask = CreateUserTask("User", "", 0, 126.0, 150.0, 0, 0);
            Shapes.Add((ProcessShape)userTask);

            // Add a system task to be paired with the user task just created
            var systemTask = CreateSystemTask("", "User", "", 0, 126.0, 150.0, 0, userTask.Id, 0);
            Shapes.Add((ProcessShape)systemTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            UpdateDestinationIdOfPreviousLink(sourceId, destinationId, userTask.Id);

            // Add a new link between the new user task and the new system task
            Links.Add(new ProcessLink
            {
                DestinationId = systemTask.Id,
                Label = null,
                Orderindex = orderIndex,
                SourceId = userTask.Id 
            });

            // Add a new link between the new system task and the destination
            Links.Add(new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
                SourceId = systemTask.Id
            });

            return userTask;
        }

        public IProcessShape AddUserDecisionPoint(int sourceId, int destinationId, double orderIndex)
        {
            var userDecisionPoint = CreateUserDecisionPoint("User", "", 0, 126.0, 150.0, 0, 0, null);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            UpdateDestinationIdOfPreviousLink(sourceId, destinationId, userDecisionPoint.Id);

            // Add a new link after the new user decision point
            Links.Add(new ProcessLink
        {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
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
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns></returns>
        private IProcessShape CreateUserTask(string persona, string itemLabel, int associatedArtifact, double width, double height, int x, int y, int storyLinkId = 0)
        {
            const string userTaskNamePrefix = "UT";

            var userTask = CreateProcessShape(ProcessShapeType.UserTask, userTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y);

            userTask.PropertyValues.Add(InputParameters,
                new PropertyValueInformation
                {
                    PropertyName = InputParameters,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(InputParameters),
                    IsVirtual = true,
                    Value = null
                }
                );

            userTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(StoryLinks),
                    IsVirtual = true,
                    Value = new StoryLink
                    {
                        AssociatedReferenceArtifactId = storyLinkId,
                        DestinationId = storyLinkId,
                        Orderindex = 0,
                        SourceId = Id
                    }
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
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <param name="userTaskId">The usertask that this system task belongs to</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns></returns>
        private IProcessShape CreateSystemTask(string associatedImageUrl, string persona, string itemLabel, int associatedArtifact, double width, double height, int x, int y, int userTaskId, int storyLinkId = 0)
        {
            const string systemTaskNamePrefix = "ST";

            var systemTask = CreateProcessShape(ProcessShapeType.SystemTask, systemTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(AssociatedImageUrl),
                    IsVirtual = true,
                    Value = associatedImageUrl
                });

            systemTask.PropertyValues.Add(OutputParameters,
                new PropertyValueInformation
                {
                    PropertyName = OutputParameters,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(OutputParameters),
                    IsVirtual = true,
                    Value = null
                }
                );

            systemTask.PropertyValues.Add(UserTaskId,
                new PropertyValueInformation
                {
                    PropertyName = UserTaskId,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(UserTaskId),
                    IsVirtual = true,
                    Value = userTaskId
                }
                );

            systemTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(StoryLinks),
                    IsVirtual = true,
                    Value = new StoryLink
                    {
                        AssociatedReferenceArtifactId = storyLinkId,
                        DestinationId = storyLinkId,
                        Orderindex = 0,
                        SourceId = Id
                    }
                }
                );

            return systemTask;
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "userDecisionPoint")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private IProcessShape CreateUserDecisionPoint(string persona, string itemLabel, int associatedArtifact,
    double width, double height, int x, int y, List<LinkLabelInfo> linkLabels)
        {
            const string userTaskNamePrefix = "UD";

            var userDecisionPoint = CreateProcessShape(ProcessShapeType.UserDecision, userTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y);

            userDecisionPoint.PropertyValues.Add(LinkLabels,
                new PropertyValueInformation
                {
                    PropertyName = LinkLabels,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(LinkLabels),
                    IsVirtual = true,
                    Value = linkLabels
                }
                );

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
                    IsVirtual = true,
                    Value = processShapeType
                });

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = FindPropertyNameTypeId(Description),
                    IsVirtual = true,
                    // Create a random description
                    Value = RandomGenerator.RandomValueWithPrefix(Description, 4)
                });

            processShape.PropertyValues.Add(Height,
                new PropertyValueInformation
                {
                    PropertyName = Height,
                    TypePredefined = PropertyTypePredefined.Height,
                    TypeId = FindPropertyNameTypeId(Height),
                    IsVirtual = true,
                    Value = height
                });

            // This is also known as Objective
            processShape.PropertyValues.Add(ItemLabel,
                new PropertyValueInformation
                {
                    PropertyName = ItemLabel,
                    TypePredefined = PropertyTypePredefined.ItemLabel,
                    TypeId = FindPropertyNameTypeId(ItemLabel),
                    IsVirtual = true,
                    Value = itemLabel
                }
                );

            processShape.PropertyValues.Add(Label,
                new PropertyValueInformation
                {
                    PropertyName = Label,
                    TypePredefined = PropertyTypePredefined.Label,
                    TypeId = FindPropertyNameTypeId(Label),
                    IsVirtual = true,
                    Value = processShape.Name
                }
                );

            processShape.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = FindPropertyNameTypeId(Persona),
                    IsVirtual = true,
                    Value = persona
                }
                );

            processShape.PropertyValues.Add(Width,
                new PropertyValueInformation
                {
                    PropertyName = Width,
                    TypePredefined = PropertyTypePredefined.Width,
                    TypeId = FindPropertyNameTypeId(Width),
                    IsVirtual = true,
                    Value = width
                }
                );

            processShape.PropertyValues.Add(X,
                new PropertyValueInformation
                {
                    PropertyName = X,
                    TypePredefined = PropertyTypePredefined.X,
                    TypeId = FindPropertyNameTypeId(X),
                    IsVirtual = true,
                    Value = x
                }
                );

            processShape.PropertyValues.Add(Y,
                new PropertyValueInformation
                {
                    PropertyName = Y,
                    TypePredefined = PropertyTypePredefined.Y,
                    TypeId = FindPropertyNameTypeId(Y),
                    IsVirtual = true,
                    Value = y
                }
                );

            return processShape;
        }

        private int? FindPropertyNameTypeId(string propertyName)
        {
            // Must convert first charater of property name to lowercase in order to find the pproperty in the 
            // default process
            propertyName = propertyName.Substring(0, 1).ToLower(CultureInfo.CurrentCulture) + propertyName.Substring(1);
            var property = Shapes.Find(shape => shape.PropertyValues.ContainsKey(propertyName));
            return property.PropertyValues[propertyName].TypeId;
        }

        private void UpdateDestinationIdOfPreviousLink(int sourceId, int originalDestinationId, int newDestinationId)
        {
            var processLink = Links.ToList()
                .Find(l => l.SourceId == sourceId && l.DestinationId == originalDestinationId);

            Links.Remove(processLink);

            processLink.DestinationId = newDestinationId;

            Links.Add(processLink);
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

        public bool IsVirtual { get; set; }

        public object Value { get; set; }
    }

    public class StoryLink : IStoryLink
    {
        public int AssociatedReferenceArtifactId { get; set; }

        public int DestinationId { get; set; }

        public double Orderindex { get; set; }

        public int SourceId { get; set; }
    }

    public class LinkLabelInfo : ILinkLabelInfo
    {
        public int LinkId { get; set; }

        public string Label { get; set; }
    }
}
