using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Repositories.ProjectMeta
{
    [TestClass]
    public class SqlProjectMetaRepositoryTests
    {
        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _repository = new SqlProjectMetaRepository(_cxn.Object);
        }

        #region GetCustomProjectTypesAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetCustomProjectTypesAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);

            // Act
            await repository.GetCustomProjectTypesAsync(-1, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetCustomProjectTypesAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);

            // Act
            await repository.GetCustomProjectTypesAsync(2, 0);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetCustomProjectTypesAsync_ProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;
            SqlProjectMetaRepository.ProjectVersion[] result = { };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);
            cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            await repository.GetCustomProjectTypesAsync(projectId, userId);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetCustomProjectTypesAsync_Unauthorized()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;
            SqlProjectMetaRepository.ProjectVersion[] result = { new SqlProjectMetaRepository.ProjectVersion { IsAccesible = false } };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);
            cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            await repository.GetCustomProjectTypesAsync(projectId, userId);

            // Assert
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_RemoveHiddenSubartifactTypes()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>();
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>
            {
                new SqlProjectMetaRepository.ItemTypeVersion { ItemTypeId = 10, Predefined = ItemTypePredefined.Content },
                new SqlProjectMetaRepository.ItemTypeVersion { ItemTypeId = 11, Predefined = ItemTypePredefined.BaselinedArtifactSubscribe },
                new SqlProjectMetaRepository.ItemTypeVersion { ItemTypeId = 12, Predefined = ItemTypePredefined.Extension },
                new SqlProjectMetaRepository.ItemTypeVersion { ItemTypeId = 13, Predefined = ItemTypePredefined.Flow },
                // Not hidden
                new SqlProjectMetaRepository.ItemTypeVersion { ItemTypeId = 13, Predefined = ItemTypePredefined.Step}
            };
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = ItemTypePredefined.Step;

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            Assert.AreEqual(0, actual.ArtifactTypes.Count);
            Assert.AreEqual(0, actual.PropertyTypes.Count);
            Assert.AreEqual(1, actual.SubArtifactTypes.Count);
            Assert.AreEqual(expected, actual.SubArtifactTypes[0].PredefinedType);
        }


        [TestMethod]
        public async Task GetCustomProjectTypesAsync_ArtifactTypes()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion { PropertyTypeId = 66 },
                new SqlProjectMetaRepository.PropertyTypeVersion { PropertyTypeId = 77 },
                new SqlProjectMetaRepository.PropertyTypeVersion { PropertyTypeId = 88 }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>
            {
                new SqlProjectMetaRepository.ItemTypeVersion
                {
                    ItemTypeId = 10,
                    Predefined = ItemTypePredefined.CustomArtifactGroup,
                    Name = "My Actor",
                    BasePredefined = ItemTypePredefined.Actor,
                    VersionId = 99,
                    IconImageId = 88,
                    Prefix = "AC",
                    InstanceItemTypeId = 77,
                    UsedInThisProject = true,
                    ItemTypeGroupId = 66
                }
            };
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>
            {
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 10, PropertyTypeId = 66 },
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 10, PropertyTypeId = 77 },
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 20, PropertyTypeId = 77 },
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 30, PropertyTypeId = 88 }
            };

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<ItemType>
            {
                new ItemType
                {
                    InstanceItemTypeId = itVersions[0].InstanceItemTypeId,
                    Prefix = itVersions[0].Prefix,
                    UsedInThisProject = itVersions[0].UsedInThisProject,
                    Id = itVersions[0].ItemTypeId,
                    Name = itVersions[0].Name,
                    VersionId = itVersions[0].VersionId,
                    IconImageId = itVersions[0].IconImageId,
                    ProjectId = _projectId,
                    PredefinedType = itVersions[0].BasePredefined,
                    CustomPropertyTypeIds = { itptMap[0].PropertyTypeId, itptMap[1].PropertyTypeId }
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.ArtifactTypes, out errorMessage), errorMessage);
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_SubartifactsTypes()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion { PropertyTypeId = 66 },
                new SqlProjectMetaRepository.PropertyTypeVersion { PropertyTypeId = 77 },
                new SqlProjectMetaRepository.PropertyTypeVersion { PropertyTypeId = 88 }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>
            {
                new SqlProjectMetaRepository.ItemTypeVersion
                {
                    ItemTypeId = 20,
                    Name = "My Shape",
                    Predefined = ItemTypePredefined.BPShape,
                    VersionId = 66,
                    Prefix = "SP",
                    UsedInThisProject = true
                }
            };
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>
            {
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 10, PropertyTypeId = 66 },
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 10, PropertyTypeId = 77 },
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 20, PropertyTypeId = 77 },
                new SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord { ItemTypeId = 30, PropertyTypeId = 88 }
            };

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<ItemType>
            {
                new ItemType
                {
                    Prefix = itVersions[0].Prefix,
                    UsedInThisProject = itVersions[0].UsedInThisProject,
                    Id = itVersions[0].ItemTypeId,
                    Name = itVersions[0].Name,
                    VersionId = itVersions[0].VersionId,
                    ProjectId = _projectId,
                    PredefinedType = itVersions[0].Predefined,
                    CustomPropertyTypeIds = { itptMap[2].PropertyTypeId }
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.SubArtifactTypes, out errorMessage), errorMessage);
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_TextPropertyType()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion
                {
                    PropertyTypeId = 66,
                    InstancePropertyTypeId = 77,
                    Name = "Text Property",
                    AllowMultiple = true,
                    VersionId = 88,
                    PrimitiveType = PropertyPrimitiveType.Text,
                    Required = false,
                    RichText = true,
                    StringDefaultValue = "My Text Default Value",
                    XmlInfo = "<CPS><CP Id=\"2269\" T=\"0\" N=\"ST-Non-Functional Requirements\" R=\"0\" AM=\"1\" AC=\"1\" SId=\"337\"><VVS/></CP></CPS>"
                }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>();
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<PropertyType>
            {
                new PropertyType
                {
                    PrimitiveType = ptVersions[0].PrimitiveType,
                    StringDefaultValue = ptVersions[0].StringDefaultValue,
                    Name = ptVersions[0].Name,
                    Id = ptVersions[0].PropertyTypeId,
                    VersionId = ptVersions[0].VersionId,
                    InstancePropertyTypeId = ptVersions[0].InstancePropertyTypeId,
                    IsMultipleAllowed = ptVersions[0].AllowMultiple,
                    IsRequired = ptVersions[0].Required,
                    IsRichText = ptVersions[0].RichText
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.PropertyTypes, out errorMessage), errorMessage);
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_NumberPropertyType()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion
                {
                    PropertyTypeId = 66,
                    InstancePropertyTypeId = 77,
                    Name = "Number Property",
                    VersionId = 88,
                    PrimitiveType = PropertyPrimitiveType.Number,
                    Required = true,
                    DecimalDefaultValue = new byte[] { 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
                    DecimalPlaces = 1,
                    MaxNumber = new byte[] { 19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
                    MinNumber = new byte[] { 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
                    Validate = true,
                    XmlInfo = "<CPS><CP Id=\"2269\" T=\"0\" N=\"ST-Non-Functional Requirements\" R=\"0\" AM=\"1\" AC=\"1\" SId=\"337\"><VVS/></CP></CPS>"
                }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>();
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<PropertyType>
            {
                new PropertyType
                {
                    PrimitiveType = ptVersions[0].PrimitiveType,
                    Name = ptVersions[0].Name,
                    Id = ptVersions[0].PropertyTypeId,
                    VersionId = ptVersions[0].VersionId,
                    InstancePropertyTypeId = ptVersions[0].InstancePropertyTypeId,
                    IsRequired = ptVersions[0].Required,
                    IsValidated = ptVersions[0].Validate,
                    DecimalPlaces = ptVersions[0].DecimalPlaces,
                    MaxNumber = PropertyHelper.ToDecimal(ptVersions[0].MaxNumber),
                    MinNumber = PropertyHelper.ToDecimal(ptVersions[0].MinNumber),
                    DecimalDefaultValue = PropertyHelper.ToDecimal(ptVersions[0].DecimalDefaultValue)
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.PropertyTypes, out errorMessage), errorMessage);
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_DatePropertyType()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion
                {
                    PropertyTypeId = 66,
                    InstancePropertyTypeId = 77,
                    Name = "Date Property",
                    VersionId = 88,
                    PrimitiveType = PropertyPrimitiveType.Date,
                    Required = true,
                    DateDefaultValue = DateTime.UtcNow,
                    MaxDate = DateTime.UtcNow.Add(TimeSpan.FromDays(10)),
                    MinDate = DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
                    Validate = true,
                    XmlInfo = "<CPS><CP Id=\"2269\" T=\"0\" N=\"ST-Non-Functional Requirements\" R=\"0\" AM=\"1\" AC=\"1\" SId=\"337\"><VVS/></CP></CPS>"
                }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>();
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<PropertyType>
            {
                new PropertyType
                {
                    PrimitiveType = ptVersions[0].PrimitiveType,
                    Name = ptVersions[0].Name,
                    Id = ptVersions[0].PropertyTypeId,
                    VersionId = ptVersions[0].VersionId,
                    InstancePropertyTypeId = ptVersions[0].InstancePropertyTypeId,
                    IsRequired = ptVersions[0].Required,
                    IsValidated = ptVersions[0].Validate,
                    DateDefaultValue = ptVersions[0].DateDefaultValue,
                    MaxDate = ptVersions[0].MaxDate,
                    MinDate = ptVersions[0].MinDate
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.PropertyTypes, out errorMessage), errorMessage);
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_ChoicePropertyType()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion
                {
                    PropertyTypeId = 66,
                    InstancePropertyTypeId = 77,
                    Name = "Choice Property",
                    VersionId = 88,
                    PrimitiveType = PropertyPrimitiveType.Choice,
                    Required = true,
                    Validate = false,
                    AllowMultiple = true,
                    XmlInfo = "<CPS><CP Id=\"66\" T=\"4\" N=\"Choice Property\" R=\"1\" AC=\"0\" AM=\"1\"><VVS><VV Id=\"6447\" S=\"0\" V=\"Low\" O=\"0\" /><VV Id=\"6448\" S=\"1\" V=\"Medium\" O=\"1\" /><VV Id=\"6449\" S=\"0\" V=\"High\" O=\"2\" /></VVS></CP></CPS>"
                }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>();
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<PropertyType>
            {
                new PropertyType
                {
                    PrimitiveType = ptVersions[0].PrimitiveType,
                    Name = ptVersions[0].Name,
                    Id = ptVersions[0].PropertyTypeId,
                    VersionId = ptVersions[0].VersionId,
                    InstancePropertyTypeId = ptVersions[0].InstancePropertyTypeId,
                    IsRequired = ptVersions[0].Required,
                    IsValidated = ptVersions[0].Validate,
                    IsMultipleAllowed = ptVersions[0].AllowMultiple,
                    ValidValues = new List<ValidValue>
                    {
                        new ValidValue { Id = 6447, Value = "Low" },
                        new ValidValue { Id = 6448, Value = "Medium" },
                        new ValidValue { Id = 6449, Value = "High" }
                    },
                    DefaultValidValueId = 6448
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.PropertyTypes, out errorMessage), errorMessage);
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_UserPropertyType()
        {
            // Arrange
            var ptVersions = new List<SqlProjectMetaRepository.PropertyTypeVersion>
            {
                new SqlProjectMetaRepository.PropertyTypeVersion
                {
                    PropertyTypeId = 66,
                    InstancePropertyTypeId = 77,
                    Name = "User Property",
                    VersionId = 88,
                    PrimitiveType = PropertyPrimitiveType.User,
                    Required = true,
                    UserDefaultValue = "3\ng1",
                    XmlInfo = "<CPS><CP Id=\"5619\" T=\"3\" N=\"User Property\" R=\"1\" AC=\"1\" AM=\"0\"><VVS /></CP></CPS>"
                }
            };
            var itVersions = new List<SqlProjectMetaRepository.ItemTypeVersion>();
            var itptMap = new List<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = new List<PropertyType>
            {
                new PropertyType
                {
                    PrimitiveType = ptVersions[0].PrimitiveType,
                    Name = ptVersions[0].Name,
                    Id = ptVersions[0].PropertyTypeId,
                    VersionId = ptVersions[0].VersionId,
                    InstancePropertyTypeId = ptVersions[0].InstancePropertyTypeId,
                    IsRequired = ptVersions[0].Required,
                    UserGroupDefaultValue = new List<UserGroup>
                    {
                        new UserGroup {Id = 3, IsGroup = false},
                        new UserGroup {Id = 1, IsGroup = true}
                    }
                }
            };

            // Act
            var actual = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            string errorMessage;
            Assert.IsTrue(Compare(expected, actual.PropertyTypes, out errorMessage), errorMessage);
        }

        #endregion

        #region GetApprovalStatusesAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetApprovalStatusesAsync_Should_Throw_If_ProjectId_Isnt_Valid()
        {   
            // Arrange
            var projectId = 0;
            var userId = 2;

            // Act
            await _repository.GetApprovalStatusesAsync(projectId, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetApprovalStatusesAsync_Should_Throw_If_Project_Doesnt_Exist()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync("GetInstanceProjectById", projectVersionParams, new List<SqlProjectMetaRepository.ProjectVersion>() { null });

            // Act
            await _repository.GetApprovalStatusesAsync(projectId, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetApprovalStatusesAsync_Should_Throw_If_User_Has_No_Permissions_For_Project()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = false } });

            // Act
            await _repository.GetApprovalStatusesAsync(projectId, userId);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Correct_Status_Text()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = true,
                        Setting = ";Test Approval Status"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual("Test Approval Status", approvalStatus.StatusText);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Correct_Approved_Status()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = true,
                        Setting = "true;test"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual(ApprovalType.Approved, approvalStatus.ApprovalType);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Correct_Disapproved_Status()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = true,
                        Setting = "False;test"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual(ApprovalType.Disapproved, approvalStatus.ApprovalType);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Correct_Not_Specified_Status()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = true,
                        Setting = ";test"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual(ApprovalType.NotSpecified, approvalStatus.ApprovalType);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Correct_IsPreset_Case_True()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = true,
                        Setting = "true;test"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual(true, approvalStatus.IsPreset);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Correct_IsPreset_Case_False()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = false,
                        Setting = "true;test"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual(false, approvalStatus.IsPreset);
        }

        [TestMethod]
        public async Task GetApprovalStatusesAsync_Should_Return_Pending_Status_Text_For_ReadOnly_Not_Specified_Approval_Setting()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var projectVersionParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@userId", userId }
            };

            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById", projectVersionParams, new[] { new SqlProjectMetaRepository.ProjectVersion() { IsAccesible = true } });

            var projectSettingsParams = new Dictionary<string, object>
            {
                { "@projectId", projectId },
                { "@propertyType", PropertyTypePredefined.ApprovalStatus },
                { "@includeDeleted", false }
            };

            _cxn.SetupQueryAsync("GetProjectSettings", projectSettingsParams, new[] {
                    new ProjectSetting()
                    {
                        Deleted = false,
                        OrderIndex = 1,
                        Predefined = PropertyTypePredefined.ApprovalStatus,
                        ProjectId = projectId,
                        ProjectSettingId = 3,
                        ReadOnly = true,
                        Setting = ";Not Specified"
                    }
                });

            // Act
            var approvalStatus = (await _repository.GetApprovalStatusesAsync(projectId, userId)).FirstOrDefault();

            // Assert
            Assert.AreEqual("Pending", approvalStatus.StatusText);
        }

        #endregion

        #region Private stuff

        private readonly int _projectId = 1;
        private readonly int _userId = 2;
        private SqlConnectionWrapperMock _cxn;
        private SqlProjectMetaRepository _repository;

        private void InitRepository(IEnumerable<SqlProjectMetaRepository.PropertyTypeVersion> ptVersions,
            IEnumerable<SqlProjectMetaRepository.ItemTypeVersion> itVersions,
            IEnumerable<SqlProjectMetaRepository.ItemTypePropertyTypeMapRecord> itptMap)
        {
            SqlProjectMetaRepository.ProjectVersion[] project = {new SqlProjectMetaRepository.ProjectVersion {IsAccesible = true}};
            _cxn.SetupQueryAsync<SqlProjectMetaRepository.ProjectVersion>("GetInstanceProjectById",
                new Dictionary<string, object> {{"projectId", _projectId}, {"userId", _userId}}, project);

            var mockResult = Tuple.Create(ptVersions, itVersions, itptMap);
            _cxn.SetupQueryMultipleAsync("GetProjectCustomTypes",
                new Dictionary<string, object> {{"projectId", _projectId}, {"revisionId", ServiceConstants.VersionHead}},
                mockResult);
        }

        private static bool Compare<T>(T obj1, T obj2, out string message)
        {
            message = null;
            if (obj1 == null && obj2 == null)
                return true;
            if (obj1 == null || obj2 == null)
            {
                message = "One of the objects is null.";
                return false;
            }

            var serializer = new XmlSerializer(obj1.GetType());
            var serialized1 = new StringWriter(CultureInfo.InvariantCulture);
            var serialized2 = new StringWriter(CultureInfo.InvariantCulture);
            serializer.Serialize(serialized1, obj1);
            serializer.Serialize(serialized2, obj2);
            var str1 = serialized1.ToString();
            var str2 = serialized2.ToString();
            var areEqual = str1 == str2;
            if (!areEqual)
                message = I18NHelper.FormatInvariant("object 1:\r\n{0}\r\nobject 2:\r\n{1}", str1, str2);
            return areEqual;
        }

        #endregion

        #region Properties Layout

        [TestMethod]
        public void OrderProperties_AdvancedSettingsNull_Success()
        {
            // Arrange
            var expectedOrder = new List<int> { 8, 10, 13, 17, 18, 20, 5, 9, 14, 15, 16, 19, 2, 4, 6, 12, 1, 3, 7, 11 };

            // Act and Assert
            TestOrderProperties(null, expectedOrder);
        }

        [TestMethod]
        public void OrderProperties_AdvancedSettingsNotNull_Success()
        {
            // Arrange
            var advancedSettings = new SqlProjectMetaRepository.AdvancedSettings
            {
                LayoutGroups = new List<SqlProjectMetaRepository.PropertyLayoutGroup>
                {
                    new SqlProjectMetaRepository.PropertyLayoutGroup
                    {
                        Type = SqlProjectMetaRepository.GroupType.General,
                        Properties = new List<SqlProjectMetaRepository.PropertyLayout>
                        {
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 17, OrderIndex = 0},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 16, OrderIndex = 1},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 20, OrderIndex = 2},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 14, OrderIndex = 3},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 11, OrderIndex = 3},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 3, OrderIndex = 4}
                        }
                    },
                    new SqlProjectMetaRepository.PropertyLayoutGroup
                    {
                        Type = SqlProjectMetaRepository.GroupType.Details,
                        Properties = new List<SqlProjectMetaRepository.PropertyLayout>
                        {
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 7, OrderIndex = 0},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 6, OrderIndex = 1},
                            new SqlProjectMetaRepository.PropertyLayout {PropertyTypeId = 12, OrderIndex = 2}
                        }
                    }
                }
            };
            var expectedOrder = new List<int> { 17, 16, 20, 14, 8, 10, 13, 18, 5, 9, 15, 19, 7, 6, 12, 11, 3, 2, 4, 1 };

            // Act and Assert
            TestOrderProperties(advancedSettings, expectedOrder);
        }

        private void TestOrderProperties(SqlProjectMetaRepository.AdvancedSettings advancedSettings, List<int> expectedOrder)
        {
            // Arrange
            var propertyTypes = new List<PropertyType>
            {
                new PropertyType {Id = 1, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = 101},
                new PropertyType {Id = 2, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = null},
                new PropertyType {Id = 3, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = 102},
                new PropertyType {Id = 4, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = null},
                new PropertyType {Id = 5, IsRichText = false, IsMultipleAllowed = true, InstancePropertyTypeId = 103},
                new PropertyType {Id = 6, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = null},
                new PropertyType {Id = 7, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = 104},
                new PropertyType {Id = 8, IsRichText = false, IsMultipleAllowed = true, InstancePropertyTypeId = null},
                new PropertyType {Id = 9, IsRichText = false, IsMultipleAllowed = true, InstancePropertyTypeId = 105},
                new PropertyType {Id = 10, IsRichText = false, IsMultipleAllowed = true, InstancePropertyTypeId = null},
                new PropertyType {Id = 11, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = 106},
                new PropertyType {Id = 12, IsRichText = true, IsMultipleAllowed = true, InstancePropertyTypeId = null},
                new PropertyType {Id = 13, InstancePropertyTypeId = null},
                new PropertyType {Id = 14, InstancePropertyTypeId = 107},
                new PropertyType {Id = 15, InstancePropertyTypeId = 108},
                new PropertyType {Id = 16, InstancePropertyTypeId = 109},
                new PropertyType {Id = 17, InstancePropertyTypeId = null},
                new PropertyType {Id = 18, InstancePropertyTypeId = null},
                new PropertyType {Id = 19, InstancePropertyTypeId = 110},
                new PropertyType {Id = 20, InstancePropertyTypeId = null}
            };

            var propertyTypeIds = new List<int>
            {
                20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
            };

            // Act
            var actualOrder = SqlProjectMetaRepository.OrderProperties(propertyTypeIds,
                propertyTypes, advancedSettings);

            // Assert
            CollectionAssert.AreEqual(expectedOrder, actualOrder);
        }

        #endregion
    }
}
