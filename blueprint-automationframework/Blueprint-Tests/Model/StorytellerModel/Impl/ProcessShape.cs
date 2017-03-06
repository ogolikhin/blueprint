using System;
using System.Collections.Generic;
using System.Globalization;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Enums;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;

namespace Model.StorytellerModel.Impl
{
    public class ProcessShape: IProcessShape
    {
        private const string StorytellerProcessPrefix = "SP";

        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int ProjectId { get; set; }

        public string TypePrefix { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        public ArtifactReference AssociatedArtifact { get; set; }

        public ArtifactReference PersonaReference { get; set; }

        public ProcessShape()
        {
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        /// <seealso cref="IProcessShape.AddAssociatedArtifact(INovaArtifactDetails)"/>
        public ArtifactReference AddAssociatedArtifact(INovaArtifactDetails artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            if (artifact.PredefinedType != null && artifact.ProjectId != null)
            {
                AssociatedArtifact = new ArtifactReference()
                {
                    Id = artifact.Id,
                    Link = null,
                    Name = artifact.Name,
                    ProjectId = artifact.ProjectId.Value,
                    TypePrefix = artifact.Prefix,
                    BaseItemTypePredefined = (ItemTypePredefined)artifact.PredefinedType.Value,
                    Version = artifact.Version
                };
            }

            return AssociatedArtifact;
        }

        /// <seealso cref="IProcessShape.AddPersonaReference(INovaArtifactDetails)"/>
        public ArtifactReference AddPersonaReference(INovaArtifactDetails artifact = null)
        {
            if (artifact?.PredefinedType != null && artifact.ProjectId != null)
            {
                PersonaReference = new ArtifactReference()
                {
                    Id = artifact.Id,
                    Link = null,
                    Name = artifact.Name,
                    ProjectId = artifact.ProjectId.Value,
                    TypePrefix = artifact.Prefix,
                    BaseItemTypePredefined = (ItemTypePredefined)artifact.PredefinedType.Value,
                    Version = artifact.Version
                };
            }

            return PersonaReference;
        }

        /// <seealso cref="IProcessShape.AddDefaultPersonaReference(ProcessShapeType)"/>
        public ArtifactReference AddDefaultPersonaReference(ProcessShapeType processShapeType)
        {
            if (processShapeType == ProcessShapeType.UserTask || processShapeType == ProcessShapeType.SystemTask ||
                processShapeType == ProcessShapeType.PreconditionSystemTask)
            {
                PersonaReference = new ArtifactReference()
                {
                    Id = processShapeType == ProcessShapeType.UserTask ? -1 : -2,
                    Link = null,
                    Name = processShapeType == ProcessShapeType.UserTask ? "User" : "System",
                    ProjectId = 0,
                    TypePrefix = null,
                    BaseItemTypePredefined = ItemTypePredefined.Actor,
                    Version = null
                };
            }
            return PersonaReference;
        }

        /// <seealso cref="IProcessShape.GetShapeType()"/>
        public ProcessShapeType GetShapeType()
        {
            string clientType = PropertyTypeName.ClientType.ToString();

            clientType = PropertyValues.ContainsKey(clientType) ? clientType : clientType.LowerCaseFirstCharacter();

            return (ProcessShapeType) Convert.ToInt32(PropertyValues[clientType].Value, CultureInfo.InvariantCulture);
        }

        /// <seealso cref="IProcessShape.IsTypeOf(ProcessShapeType)"/>
        public bool IsTypeOf(ProcessShapeType processShapeType)
        {
            return GetShapeType() == processShapeType;
        }

        /// <summary>
        /// Find a Process Shape by name in an list of Process Shapes
        /// </summary>
        /// <param name="shapeName">The name of the process shape to find</param>
        /// <param name="shapesToSearchThrough">The process shapes to search</param>
        /// <returns>The found process shape</returns>
        public static IProcessShape FindProcessShapeByName(string shapeName,
            List<ProcessShape> shapesToSearchThrough)
        {
            ThrowIf.ArgumentNull(shapesToSearchThrough, nameof(shapesToSearchThrough));

            var shapeFound = shapesToSearchThrough.Find(s => s.Name == shapeName);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Name {0}", shapeName);

            return shapeFound;
        }

        /// <summary>
        /// Find a Process Shape by id in an list of Process Shapes
        /// </summary>
        /// <param name="shapeId">The id of the process shape to find</param>
        /// <param name="shapesToSearchThrough">The process shapes to search</param>
        /// <returns>The found process shape</returns>
        public static IProcessShape FindProcessShapeById(int shapeId,
            List<ProcessShape> shapesToSearchThrough)
        {
            ThrowIf.ArgumentNull(shapesToSearchThrough, nameof(shapesToSearchThrough));

            var shapeFound = shapesToSearchThrough.Find(s => s.Id == shapeId);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Id {0}", shapeId);

            return shapeFound;
        }

        /// <summary>
        /// Assert that Process Shapes are equal
        /// </summary>
        /// <param name="shape1">The first Shape</param>
        /// <param name="shape2">The Shape being compared to the first</param>
        /// <param name="allowNegativeShapeIds">Allows for inequality of shape ids where a newly added shape has a negative id</param>
        /// <param name="isCopiedProcess">(optional) Flag indicating if the process being compared is a copied process</param>
        public static void AssertAreEqual(IProcessShape shape1, IProcessShape shape2, bool allowNegativeShapeIds, bool isCopiedProcess)
        {
            ThrowIf.ArgumentNull(shape1, nameof(shape1));
            ThrowIf.ArgumentNull(shape2, nameof(shape2));

            // Do not perform Id comparisons for copied processes
            if (!isCopiedProcess)
            {
                // Note that if a shape id of the first Process being compared is less than 0, then the first Process 
                // is a process that will be updated with proper id values at the back end.  If the shape id of
                // the first process being compared is greater than 0, then the shape ids should match.
                if (allowNegativeShapeIds && shape1.Id < 0)
                {
                    Assert.That(shape2.Id > 0, "Returned shape id was negative");
                }
                else if (allowNegativeShapeIds && shape2.Id < 0)
                {
                    Assert.That(shape1.Id > 0, "Returned shape id was negative");
                }
                else
                {
                    Assert.AreEqual(shape1.Id, shape2.Id, "Shape ids do not match");
                }
            }

            Assert.AreEqual(shape1.Name, shape2.Name, "Shape names do not match");
            Assert.AreEqual(shape1.BaseItemTypePredefined, shape2.BaseItemTypePredefined,
                "Shape base item types do not match");

            // Project and parent ids are not necessarily the same for copied processes
            if (!isCopiedProcess)
            {
                Assert.AreEqual(shape1.ProjectId, shape2.ProjectId, "Shape project ids do not match");
                Assert.AreEqual(shape1.ParentId, shape2.ParentId, "Shape parent ids do not match");
            }

            Assert.AreEqual(shape1.TypePrefix, shape2.TypePrefix, "Shape type prefixes do not match");

            // Assert associated artifacts are equal by checking artifact Id only
            ArtifactReference.AssertAreEqual(shape1.AssociatedArtifact, shape2.AssociatedArtifact, doDeepCompare: false);

            // Assert persona references are equal by checking artifact Id only
            ArtifactReference.AssertAreEqual(shape1.PersonaReference, shape2.PersonaReference, doDeepCompare: false);

            // Assert that Shape properties are equal
            foreach (var shape1Property in shape1.PropertyValues)
            {
                var shape2Property = PropertyValueInformation.FindPropertyValue(shape1Property.Key, shape2.PropertyValues);

                PropertyValueInformation.AssertAreEqual(shape1Property.Value, shape2Property.Value);
            }
        }
    }
}