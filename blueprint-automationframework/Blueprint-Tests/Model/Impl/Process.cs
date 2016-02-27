using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Utilities;

namespace Model.Impl
{
    public class Process: IProcess
    {

        #region Constants

        public const string DefaultPreconditionName = "Precondition";
        public const string DefaultUserTaskName = "User Task 1";
        public const string DefaultSystemTaskName = "System Task 1";

        #endregion Constants


        #region Properties

        public int ProjectId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string TypePrefix { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<ProcessShape>>))]
        public List<ProcessShape> Shapes { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<ProcessLink>>))]
        public List<ProcessLink> Links { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<ArtifactPathLink>>))]
        public List<ArtifactPathLink> ArtifactPathLinks { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<Dictionary<string, PropertyValueInformation>>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        #endregion Properties


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

        public IProcess AddUserTask(IProcessLink sourceLink, IProcessLink destinationLink)
        {
            throw new NotImplementedException();
        }

        public IProcess AddUserDecisionPoint(IProcessLink sourceLink, IProcessLink destinationLink)
        {
            throw new NotImplementedException();
        }

        public IProcess AddBranch(IProcessLink sourceLink, IProcessLink destinationLink)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods

        #region Private Methods

        private IProcessShape CreateUserTask()
        {
            IProcessShape userTask = new ProcessShape();

            userTask.BaseItemTypePredefined = ItemTypePredefined.PROShape;
            userTask.Id = -1;
            userTask.Name = "User Task";
            userTask.ParentId = -1;
            userTask.ProjectId = -1;
            userTask.TypePrefix = "Prefix";

            userTask.PropertyValues.Add("clientType",
                new PropertyValueInformation
                {
                    PropertyName = "clientType",
                    TypePredefined = PropertyTypePredefined.ClientType,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                });

            userTask.PropertyValues.Add("description", 
                new PropertyValueInformation
                {
                    PropertyName = "description",
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                });

            userTask.PropertyValues.Add("height",
                new PropertyValueInformation
                {
                    PropertyName = "height",
                    TypePredefined = PropertyTypePredefined.Height,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                });

            userTask.PropertyValues.Add("include",
                new PropertyValueInformation
                {
                    PropertyName = "include",
                    TypePredefined = 0,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("inputParameters",
                new PropertyValueInformation
                {
                    PropertyName = "inputParameters",
                    TypePredefined = 0,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("itemLabel",
                new PropertyValueInformation
                {
                    PropertyName = "itemLabel",
                    TypePredefined = PropertyTypePredefined.ItemLabel,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("label",
                new PropertyValueInformation
                {
                    PropertyName = "label",
                    TypePredefined = PropertyTypePredefined.Label,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("persona",
                new PropertyValueInformation
                {
                    PropertyName = "persona",
                    TypePredefined = 0,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("storyLinks",
                new PropertyValueInformation
                {
                    PropertyName = "storyLinks",
                    TypePredefined = 0,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("width",
                new PropertyValueInformation
                {
                    PropertyName = "width",
                    TypePredefined = PropertyTypePredefined.Width,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("x",
                new PropertyValueInformation
                {
                    PropertyName = "x",
                    TypePredefined = PropertyTypePredefined.X,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            userTask.PropertyValues.Add("y",
                new PropertyValueInformation
                {
                    PropertyName = "y",
                    TypePredefined = PropertyTypePredefined.Y,
                    TypeId = null,
                    IsVirtual = true,
                    Value = ProcessShapeType.UserTask
                }
                );

            throw new NotImplementedException();
        }

        private IProcessShape CreateSystemtask()
        {
            IProcessShape systemTask = new ProcessShape();

            throw new NotImplementedException();
        }

        private IProcessShape CreateDecisionPoint()
        {
            IProcessShape userDecisionPoint = new ProcessShape();

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
}
