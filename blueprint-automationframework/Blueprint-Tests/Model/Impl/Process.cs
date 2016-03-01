using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Utilities;
using Utilities.Factories;

namespace Model.Impl
{
    public class Process: IProcess
    {
        #region Constants

        public const string DefaultPreconditionName = "Precondition";

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

        public void SetShapes(List<ProcessShape> shapes)
        {
            if (Shapes == null)
            {
                Shapes = new List<ProcessShape>();
            }
            Shapes = shapes;
        }

        public void SetLinks(List<ProcessLink> links)
        {
            if (Links == null)
            {
                Links = new List<ProcessLink>();
            }
            Links = links;
        }

        public void SetArtifactPathLinks(List<ArtifactPathLink> artifactPathLinks)
        {
            if (ArtifactPathLinks == null)
            {
                ArtifactPathLinks = new List<ArtifactPathLink>();
            }
            ArtifactPathLinks = artifactPathLinks;
        }

        public void SetPropertyValues(Dictionary<string, PropertyValueInformation> propertyValues)
        {
            if (PropertyValues == null)
            {
                PropertyValues = new Dictionary<string, PropertyValueInformation>();
            }
            PropertyValues = propertyValues;
        }

        public IProcessShape AddUserTask(int sourceId, int destinationId, int orderIndex)
        {
            var userTask = CreateUserTask("User", "", 0, 126.0, 150.0, 0, 0);

            var systemTask = CreateSystemTask("", "User", "", 0, 126.0, 150.0, 0, userTask.Id, 0);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            Links.First(l => (int)l.Orderindex == orderIndex && l.SourceId == sourceId && l.DestinationId == destinationId)
                .DestinationId = userTask.Id;

            // Add a new link between the new user task and the new system task
            Links.Add(new ProcessLink
            {
                DestinationId = systemTask.Id,
                Label = string.Empty,
                Orderindex = orderIndex,
                SourceId = userTask.Id 
            });

            // Add a new link between the new system task and the destination
            Links.Add(new ProcessLink
            {
                DestinationId = destinationId,
                Label = string.Empty,
                Orderindex = orderIndex,
                SourceId = systemTask.Id
            });

            return userTask;
        }

        public IProcessShape AddUserDecisionPoint(int sourceId, int destinationId, int orderIndex)
        {
            throw new NotImplementedException();
        }

        public IProcessShape AddBranch(int sourceId, int destinationId, int orderIndex)
        {
            throw new NotImplementedException();
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

            var userTask = CreateProcessShape(userTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y, storyLinkId);

            userTask.PropertyValues.Add(ClientType,
                new PropertyValueInformation
                {
                    PropertyName = ClientType,
                    TypePredefined = PropertyTypePredefined.ClientType,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(ClientType)).PropertyValues[ClientType].TypeId,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                });

            userTask.PropertyValues.Add(InputParameters,
                new PropertyValueInformation
                {
                    PropertyName = InputParameters,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(InputParameters)).PropertyValues[InputParameters].TypeId,
                    IsVirtual = true,
                    Value = null
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

            var systemTask = CreateProcessShape(systemTaskNamePrefix, persona, itemLabel, associatedArtifact, width, height, x, y, storyLinkId);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(AssociatedImageUrl)).PropertyValues[AssociatedImageUrl].TypeId,
                    IsVirtual = true,
                    Value = associatedImageUrl
                });

            systemTask.PropertyValues.Add(ClientType,
                new PropertyValueInformation
                {
                    PropertyName = ClientType,
                    TypePredefined = PropertyTypePredefined.ClientType,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(ClientType)).PropertyValues[ClientType].TypeId,
                    IsVirtual = true,
                    Value = ProcessShapeType.SystemTask
                });

            systemTask.PropertyValues.Add(OutputParameters,
                new PropertyValueInformation
                {
                    PropertyName = OutputParameters,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(OutputParameters)).PropertyValues[OutputParameters].TypeId,
                    IsVirtual = true,
                    Value = null
                }
                );

            systemTask.PropertyValues.Add(UserTaskId,
                new PropertyValueInformation
                {
                    PropertyName = UserTaskId,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(UserTaskId)).PropertyValues[UserTaskId].TypeId,
                    IsVirtual = true,
                    Value = userTaskId
                }
                );

            return systemTask;
        }

        /// <summary>
        /// Create a Generic Process Shape
        /// </summary>
        /// <param name="shapeNamePrefix">The prefix for both the shape name and the shape label</param>
        /// <param name="persona">The persona of the process shape</param>
        /// <param name="itemLabel">The item label of the process shape</param>
        /// <param name="width">The width of the process shape</param>
        /// <param name="height">The height of the process shape</param>
        /// <param name="x">The x coordinate of the process shape</param>
        /// <param name="y">The y coordinate of the process shape</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns></returns>
        private IProcessShape CreateProcessShape(string shapeNamePrefix, string persona, string itemLabel, int associatedArtifact, double width, double height, int x, int y, int storyLinkId = 0)
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

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Description)).PropertyValues[Description].TypeId,
                    IsVirtual = true,
                    // Create a random description
                    Value = RandomGenerator.RandomValueWithPrefix(Description, 4)
                });

            processShape.PropertyValues.Add(Height,
                new PropertyValueInformation
                {
                    PropertyName = Height,
                    TypePredefined = PropertyTypePredefined.Height,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Height)).PropertyValues[Height].TypeId,
                    IsVirtual = true,
                    Value = height
                });

            processShape.PropertyValues.Add(ItemLabel,
                new PropertyValueInformation
                {
                    PropertyName = ItemLabel,
                    TypePredefined = PropertyTypePredefined.ItemLabel,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(ItemLabel)).PropertyValues[ItemLabel].TypeId,
                    IsVirtual = true,
                    Value = itemLabel
                }
                );

            processShape.PropertyValues.Add(Label,
                new PropertyValueInformation
                {
                    PropertyName = Label,
                    TypePredefined = PropertyTypePredefined.Label,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Label)).PropertyValues[Label].TypeId,
                    IsVirtual = true,
                    Value = processShape.Name
                }
                );

            processShape.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Persona)).PropertyValues[Persona].TypeId,
                    IsVirtual = true,
                    Value = persona
                }
                );

            processShape.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(StoryLinks)).PropertyValues[StoryLinks].TypeId,
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

            processShape.PropertyValues.Add(Width,
                new PropertyValueInformation
                {
                    PropertyName = Width,
                    TypePredefined = PropertyTypePredefined.Width,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Width)).PropertyValues[Width].TypeId,
                    IsVirtual = true,
                    Value = width
                }
                );

            processShape.PropertyValues.Add(X,
                new PropertyValueInformation
                {
                    PropertyName = X,
                    TypePredefined = PropertyTypePredefined.X,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(X)).PropertyValues[X].TypeId,
                    IsVirtual = true,
                    Value = x
                }
                );

            processShape.PropertyValues.Add(Y,
                new PropertyValueInformation
                {
                    PropertyName = Y,
                    TypePredefined = PropertyTypePredefined.Y,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Y)).PropertyValues[Y].TypeId,
                    IsVirtual = true,
                    Value = y
                }
                );

            return processShape;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private IProcessShape CreateDecisionPoint()
        {
//            IProcessShape userDecisionPoint = new ProcessShape();

            throw new NotImplementedException();
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

        public void SetPropertyValues(Dictionary<string, PropertyValueInformation> propertyValues)
        {
            if (PropertyValues == null)
            {
                PropertyValues = new Dictionary<string, PropertyValueInformation>();
            }
            PropertyValues = propertyValues;
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
}
