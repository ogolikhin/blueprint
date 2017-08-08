using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.Reuse;
using ArtifactStore.Repositories.Reuse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Reuse;

namespace ArtifactStore.Helpers
{
    [TestClass]
    public class SensitivityCommonHelperTests
    {
        private ReuseSensitivityCollector _sensitivityCollector;
        private Mock<IReuseRepository> _reuseRepository;
        private SensitivityCommonHelper _sensitivityCommonHelper;

        [TestInitialize]
        public void Initialize()
        {
            _sensitivityCollector = new ReuseSensitivityCollector();
            _reuseRepository = new Mock<IReuseRepository>();
            _sensitivityCommonHelper = new SensitivityCommonHelper();
        }

        [TestMethod]
        public async Task FilterInsensitiveItems_SingleSensitiveProcessed_SuccessfullyRetrieveSingleSensitiveItem()
        {
            //Arrange
            _sensitivityCollector.ArtifactModifications.Add(1, new ReuseSensitivityCollector.ArtifactModification
            {
                ArtifactAspects = ItemTypeReuseTemplateSetting.Name
            });
            _sensitivityCollector.ArtifactModifications[1].RegisterArtifactPropertyModification(1, PropertyTypePredefined.Name);
            _reuseRepository.Setup(t => t.GetStandardTypeIdsForArtifactsIdsAsync(It.IsAny<ISet<int>>()))
                .ReturnsAsync(new Dictionary<int, SqlItemTypeInfo>
                {
                    {
                        1, new SqlItemTypeInfo()
                        {
                            InstanceTypeId = 3,
                            ItemTypePredefined = ItemTypePredefined.Actor,
                            ItemId = 1,
                            TypeId = 4
                        }
                    }
                });
            _reuseRepository.Setup(t => t.GetReuseItemTypeTemplatesAsyc(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, ItemTypeReuseTemplate>()
                {
                    {
                        3, new ItemTypeReuseTemplate()
                        {
                            ItemTypeId = 4
                        }
                    }
                });

            //Act
            var result = (await _sensitivityCommonHelper.FilterInsensitiveItems(new List<int>() {1}, _sensitivityCollector, _reuseRepository.Object))
                .ToList();

            //Assert
            Assert.IsTrue(result.Count == 1, "One item should have been retrieved as sensitive.");
            Assert.IsTrue(result[0] == 1, "One item should have been retrieved as sensitive.");
        }

        [TestMethod]
        public async Task FilterInsensitiveItems_MultipleItemProcessed_RetrieveSingleSensitiveItem()
        {
            //Arrange
            _sensitivityCollector.ArtifactModifications.Add(1, new ReuseSensitivityCollector.ArtifactModification
            {
                ArtifactAspects = ItemTypeReuseTemplateSetting.Name
            });
            _sensitivityCollector.ArtifactModifications[1].RegisterArtifactPropertyModification(1, PropertyTypePredefined.Name);
            _reuseRepository.Setup(t => t.GetStandardTypeIdsForArtifactsIdsAsync(It.IsAny<ISet<int>>()))
                .ReturnsAsync(new Dictionary<int, SqlItemTypeInfo>
                {
                    {
                        1, new SqlItemTypeInfo()
                        {
                            InstanceTypeId = 3,
                            ItemTypePredefined = ItemTypePredefined.Actor,
                            ItemId = 1,
                            TypeId = 4
                        }
                    }
                });
            _reuseRepository.Setup(t => t.GetReuseItemTypeTemplatesAsyc(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, ItemTypeReuseTemplate>()
                {
                    {
                        3, new ItemTypeReuseTemplate()
                        {
                            ItemTypeId = 4
                        }
                    }
                });

            //Act
            var result = (await _sensitivityCommonHelper.FilterInsensitiveItems(new List<int> { 1, 5, 8 }, _sensitivityCollector, _reuseRepository.Object)).ToList();

            //Assert
            Assert.IsTrue(result.Count == 1, "One item should have been retrieved as sensitive.");
            Assert.IsTrue(result[0] == 1, "One item should have been retrieved as sensitive.");
        }

        [TestMethod]
        public async Task FilterInsensitiveItems_MultipleItemsProcessed_NoSensitiveItem()
        {
            //Arrange
            _sensitivityCollector.ArtifactModifications.Add(1, new ReuseSensitivityCollector.ArtifactModification
            {
                ArtifactAspects = ItemTypeReuseTemplateSetting.Name
            });
            _sensitivityCollector.ArtifactModifications[1].RegisterArtifactPropertyModification(1, PropertyTypePredefined.Name);
            _reuseRepository.Setup(t => t.GetStandardTypeIdsForArtifactsIdsAsync(It.IsAny<ISet<int>>()))
                .ReturnsAsync(new Dictionary<int, SqlItemTypeInfo>
                {
                    {
                        1, new SqlItemTypeInfo()
                        {
                            InstanceTypeId = 3,
                            ItemTypePredefined = ItemTypePredefined.Actor,
                            ItemId = 1,
                            TypeId = 4
                        }
                    }
                });
            _reuseRepository.Setup(t => t.GetReuseItemTypeTemplatesAsyc(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, ItemTypeReuseTemplate>()
                {
                    {
                        3, new ItemTypeReuseTemplate()
                        {
                            ItemTypeId = 4
                        }
                    }
                });

            //Act
            var result = await _sensitivityCommonHelper.FilterInsensitiveItems(new List<int> { 4, 5, 8 }, _sensitivityCollector, _reuseRepository.Object);

            //Assert
            Assert.IsTrue(!result.Any(), "No item should have been retrieved as sensitive.");
        }
    }
}
