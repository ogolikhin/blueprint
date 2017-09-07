using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Models.VersionControl;
using System.Globalization;

namespace ServiceLibrary.Models.Workflow
{
    // NOTE: Helper classes are introduced to access (via inheritance) protected methods
    //       of the objects to test. This is to keep the original design intact.

    class PreopWorkflowEventTriggersHelper : PreopWorkflowEventTriggers
    {
        public async Task SavePropertyChangeActionsTest(IExecutionParameters executionParameters)
        {
            await base.InternalBatchExecute(executionParameters);
        }
    }

    class TextPropertyChangeActionHelper : PropertyChangeAction
    {
        public TextPropertyChangeActionHelper(int instancePropertyTypeId, string value)
        {
            InstancePropertyTypeId = instancePropertyTypeId;
            PropertyValue = value;
            PopulatePropertyLite(new WorkflowPropertyType
            {
                PrimitiveType = ProjectMeta.PropertyPrimitiveType.Text
            });
        }
    }

    class NumberPropertyChangeActionHelper : PropertyChangeAction
    {
        public NumberPropertyChangeActionHelper(int instancePropertyTypeId, decimal? value)
        {
            InstancePropertyTypeId = instancePropertyTypeId;
            PropertyValue = value.HasValue ? value.Value.ToString("N5", CultureInfo.InvariantCulture) : null;
            PopulatePropertyLite(new WorkflowPropertyType
            {
                PrimitiveType = ProjectMeta.PropertyPrimitiveType.Number
            });
        }
    }

    [TestClass]
    public class PreopWorkflowEventTriggersTest
    {
        const int NameInstancePropertyTypeId = 2;
        const int DescriptionInstancePropertyTypeId = 3;
        const int TextInstancePropertyTypeId = 30;
        const int NumberInstancePropertyTypeId = 40;
        const int UserId = 1;
        const int ArtifactVersion = 1;

        Mock<ISaveArtifactRepository> _saveRepositoryMock;
        Mock<IExecutionParameters> _executionParametersMock;

        [TestInitialize]
        public void Initialize()
        {
            _saveRepositoryMock = new Mock<ISaveArtifactRepository>();
            _saveRepositoryMock.Setup(r => r.UpdateArtifactName(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                null))
                .Returns(Task.FromResult(default(object))).Verifiable();

            _saveRepositoryMock.Setup(r => r.SavePropertyChangeActions(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<IPropertyChangeAction>>(),
                It.IsAny<IEnumerable<WorkflowPropertyType>>(),
                It.IsAny<VersionControlArtifactInfo>(),
                null))
                .Returns(Task.FromResult(default(object))).Verifiable();

            _executionParametersMock = new Mock<IExecutionParameters>();
            _executionParametersMock.Setup(p => p.SaveRepository).Returns(_saveRepositoryMock.Object);
            _executionParametersMock.Setup(p => p.UserId).Returns(UserId);
            _executionParametersMock.Setup(p => p.ArtifactInfo).Returns(new VersionControlArtifactInfo { Id = ArtifactVersion });
            
            // CustomPropertyTypes - are the current Artifact properties
            _executionParametersMock.Setup(p => p.CustomPropertyTypes).Returns(new List<WorkflowPropertyType>
            {
                // Each Artifact must have Name and Description
                new WorkflowPropertyType
                {
                    Predefined = ProjectMeta.PropertyTypePredefined.Name,
                    InstancePropertyTypeId = NameInstancePropertyTypeId
                },
                new WorkflowPropertyType
                {
                    Predefined = ProjectMeta.PropertyTypePredefined.Description,
                    InstancePropertyTypeId = DescriptionInstancePropertyTypeId
                },
                // Add a Custom Text property
                new WorkflowPropertyType
                {
                    Predefined = ProjectMeta.PropertyTypePredefined.CustomGroup,
                    InstancePropertyTypeId = TextInstancePropertyTypeId
                },
                // Add a Custom Number property
                new WorkflowPropertyType
                {
                    Predefined = ProjectMeta.PropertyTypePredefined.CustomGroup,
                    InstancePropertyTypeId = NumberInstancePropertyTypeId
                }
            });
        }

        [TestMethod]
        public async Task PreopTriggers_ChangeNameAndDescriptionAndCustom_Success()
        {
            // Arrange

            // Types of Artifact properties to modify on state change
            var eventTriggerHelper = new PreopWorkflowEventTriggersHelper
            {
                new WorkflowEventTrigger
                {
                    Action = new TextPropertyChangeActionHelper(NameInstancePropertyTypeId, "Artifact-Name")
                },
                new WorkflowEventTrigger
                {
                    Action = new TextPropertyChangeActionHelper(DescriptionInstancePropertyTypeId, "Artifact-Description")
                },
                new WorkflowEventTrigger
                {
                    Action = new TextPropertyChangeActionHelper(TextInstancePropertyTypeId, "Artifact-Standard-Text-Property")
                },
                new WorkflowEventTrigger
                {
                    Action = new NumberPropertyChangeActionHelper(NumberInstancePropertyTypeId, 100.00M)
                }
            };

            // Act
            await eventTriggerHelper.SavePropertyChangeActionsTest(_executionParametersMock.Object);

            // Assert
            _saveRepositoryMock.Verify(r => r.UpdateArtifactName(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                null), 
                Times.Once());

            _saveRepositoryMock.Verify(r => r.SavePropertyChangeActions(
                It.IsAny<int>(),
                It.Is<IEnumerable<IPropertyChangeAction>>(a => a.Count() == 4), // number of Artifact properties to modify
                It.IsAny<IEnumerable<WorkflowPropertyType>>(),
                It.IsAny<VersionControlArtifactInfo>(),
                null),
                Times.Once());
        }

        [TestMethod]
        public async Task PreopTriggers_ChangeDescriptionAndCustom_Success()
        {
            // Arrange

            // Types of Artifact properties to modify on state change
            var eventTriggerHelper = new PreopWorkflowEventTriggersHelper
            {
                new WorkflowEventTrigger
                {
                    Action = new TextPropertyChangeActionHelper(DescriptionInstancePropertyTypeId, "Artifact-Description")
                },
                new WorkflowEventTrigger
                {
                    Action = new TextPropertyChangeActionHelper(TextInstancePropertyTypeId, "Artifact-Standard-Text-Property")
                }
            };

            // Act
            await eventTriggerHelper.SavePropertyChangeActionsTest(_executionParametersMock.Object);

            // Assert
            _saveRepositoryMock.Verify(r => r.UpdateArtifactName(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                null),
                Times.Never());

            _saveRepositoryMock.Verify(r => r.SavePropertyChangeActions(
                It.IsAny<int>(),
                It.Is<IEnumerable<IPropertyChangeAction>>(a => a.Count() == 2), // number of Artifact properties to modify
                It.IsAny<IEnumerable<WorkflowPropertyType>>(),
                It.IsAny<VersionControlArtifactInfo>(),
                null),
                Times.Once());
        }

        [TestMethod]
        public async Task PreopTriggers_ChangeCustom_Success()
        {
            // Arrange

            // Types of Artifact properties to modify on state change
            var eventTriggerHelper = new PreopWorkflowEventTriggersHelper
            {
                new WorkflowEventTrigger
                {
                    Action = new TextPropertyChangeActionHelper(TextInstancePropertyTypeId, "Artifact-Standard-Text-Property")
                }
            };

            // Act
            await eventTriggerHelper.SavePropertyChangeActionsTest(_executionParametersMock.Object);

            // Assert
            _saveRepositoryMock.Verify(r => r.UpdateArtifactName(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                null),
                Times.Never());

            _saveRepositoryMock.Verify(r => r.SavePropertyChangeActions(
                It.IsAny<int>(),
                It.Is<IEnumerable<IPropertyChangeAction>>(a => a.Count() == 1), // number of Artifact properties to modify
                It.IsAny<IEnumerable<WorkflowPropertyType>>(),
                It.IsAny<VersionControlArtifactInfo>(),
                null),
                Times.Once());
        }
    }
}
