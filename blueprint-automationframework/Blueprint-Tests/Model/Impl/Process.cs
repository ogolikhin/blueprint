using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common;
using Newtonsoft.Json;
using Utilities;
using Utilities.Factories;

namespace Model.Impl
{
    public class Process: IProcess
    {
        #region Constants

        public const string DefaultPreconditionName = "Precondition";
        public const string DefaultUserTaskName = "User Task 1";
        public const string DefaultSystemTaskName = "System Task 1";

        #endregion Constants

        #region Private Properties

        private int tempId = 0;

        public static readonly string Description = PropertyTypePredefined.Description.ToString();

        public static readonly string Objective = PropertyTypePredefined.ItemLabel.ToString();

        public static readonly string Label = PropertyTypePredefined.Label.ToString();

        public static readonly string X = PropertyTypePredefined.X.ToString();

        public static readonly string Y = PropertyTypePredefined.Y.ToString();

        public static readonly string Height = PropertyTypePredefined.Height.ToString();

        public static readonly string Width = PropertyTypePredefined.Width.ToString();

        public static readonly string ClientType = PropertyTypePredefined.ClientType.ToString();

        public const string Include = "Include";

        public const string LinkLabels = "LinkLabels";

        public const string Persona = "Persona";

        public const string AssociatedImageUrl = "AssociatedImageUrl";

        public const string OutputParameters = "OutputParameters";

        public const string UserTaskId = "UserTaskId";

        public const string StoryLinks = "StoryLinks";

        public const string InputParameters = "InputParameters";

        public const string ItemLabel = "ItemLabel";

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
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Dictionary<string, PropertyValueInformation>>))]
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

        public void AddUserTask(int sourceId, int destinationId, int orderIndex)
        {
            IProcessShape userTask = CreateUserTask("User", "", 0, 126, 150, 0, 0);

            IProcessShape systemTask = CreateSystemTask("", "User", "", 0, 126, 150, 0, userTask.Id, 0);

            if (Links != null)
            {
                Links.First(l => l.Orderindex == orderIndex && l.SourceId == sourceId && l.DestinationId == destinationId)
                    .DestinationId = userTask.Id;

                Links.Add(new ProcessLink
                {
                    DestinationId = systemTask.Id,
                    Label = string.Empty,
                    Orderindex = orderIndex,
                    SourceId = userTask.Id 
                });

                Links.Add(new ProcessLink
                {
                    DestinationId = destinationId,
                    Label = string.Empty,
                    Orderindex = orderIndex,
                    SourceId = systemTask.Id
                });
            }
        }

        public void AddUserDecisionPoint(int sourceId, int destinationId, int orderIndex)
        {
            throw new NotImplementedException();
        }

        public void AddBranch(int sourceId, int destinationId, int orderIndex)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods

        #region Private Methods

        private IProcessShape CreateUserTask(string persona, string itemLabel, int include, int width, int height, int x, int y, int storyLinkId = 0)
        {
            const string userTaskNamePrefix = "UT";

            IProcessShape userTask = CreateProcessShape(userTaskNamePrefix, persona, itemLabel, include, width, height, x, y, storyLinkId);

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
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(InputParameters)).PropertyValues[InputParameters].TypeId,
                    IsVirtual = true,
                    Value = null
                }
                );

            return userTask;
        }

        private IProcessShape CreateSystemTask(string associatedImageUrl, string persona, string itemLabel, int include, int width, int height, int x, int y, int userTaskId, int storyLinkId = 0)
        {
            const string systemTaskNamePrefix = "ST";

            IProcessShape systemTask = CreateProcessShape(systemTaskNamePrefix, persona, itemLabel, include, width, height, x, y, storyLinkId);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = 0,
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
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(OutputParameters)).PropertyValues[OutputParameters].TypeId,
                    IsVirtual = true,
                    Value = null
                }
                );

            systemTask.PropertyValues.Add(UserTaskId,
                new PropertyValueInformation
                {
                    PropertyName = UserTaskId,
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(UserTaskId)).PropertyValues[UserTaskId].TypeId,
                    IsVirtual = true,
                    Value = userTaskId
                }
                );

            return systemTask;
        }

        private IProcessShape CreateProcessShape(string taskNamePrefix, string persona, string itemLabel, int include, int width, int height, int x, int y, int storyLinkId = 0)
        {
            IProcessShape processShape = new ProcessShape();

            processShape.BaseItemTypePredefined = ItemTypePredefined.PROShape;
            processShape.Id = --tempId;
            processShape.Name = taskNamePrefix + processShape.Id;
            processShape.ParentId = Id;
            processShape.ProjectId = ProjectId;
            processShape.TypePrefix = "PROS";

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Description)).PropertyValues[Description].TypeId,
                    IsVirtual = true,
                    Value = RandomValue(Description)
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

            processShape.PropertyValues.Add(Include,
                new PropertyValueInformation
                {
                    PropertyName = Include,
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Include)).PropertyValues[Include].TypeId,
                    IsVirtual = true,
                    Value = include
                }
                );

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

            processShape.PropertyValues.Add(OutputParameters,
                new PropertyValueInformation
                {
                    PropertyName = OutputParameters,
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(OutputParameters)).PropertyValues[OutputParameters].TypeId,
                    IsVirtual = true,
                    Value = null
                }
                );

            processShape.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(Persona)).PropertyValues[Persona].TypeId,
                    IsVirtual = true,
                    Value = persona
                }
                );

            processShape.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = 0,
                    TypeId = Shapes.First(shape => shape.PropertyValues.ContainsKey(StoryLinks)).PropertyValues[StoryLinks].TypeId,
                    IsVirtual = true,
                    Value = new StoryLink
                    {
                        AssociatedReferenceArtifactId = storyLinkId,
                        DestinationId = storyLinkId,
                        OrderIndex = 0,
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

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "userDecisionPoint")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private IProcessShape CreateDecisionPoint()
        {
            IProcessShape userDecisionPoint = new ProcessShape();

            throw new NotImplementedException();
        }

        private static string RandomValue(string prefix)
        {
            return I18NHelper.FormatInvariant("{0}_{1}", prefix, RandomGenerator.RandomAlphaNumericUpperAndLowerCase(4));
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

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Dictionary<string, PropertyValueInformation>>))]
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

        public int Orderindex { get; set; }

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

        public int OrderIndex { get; set; }

        public int SourceId { get; set; }
    }
}
