using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Controllers
{
    [TestClass]
    public class GroupsControllerTests
    {
        private Mock<IGroupRepository> _sqlGroupRepositoryMock;
        private Mock<IPrivilegesRepository> _privilegesRepository;
        private QueryResult<GroupDto> _groupsQueryDataResult;
        private GroupsController _controller;

        private const int SessionUserId = 1;
        private Pagination _groupsTabularPagination;
        private Sorting _groupsSorting;
        private const int UserId = 10;
        private GroupDto _group;
        private int _groupId = 10;

        [TestInitialize]
        public void Initialize()
        {
            var session = new Session { UserId = SessionUserId };

            _sqlGroupRepositoryMock = new Mock<IGroupRepository>();
            _privilegesRepository = new Mock<IPrivilegesRepository>();

            _controller = new GroupsController(_sqlGroupRepositoryMock.Object, _privilegesRepository.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");

            _groupsQueryDataResult = new QueryResult<GroupDto>() { Total = 1, Items = new List<GroupDto>() };
            _groupsTabularPagination = new Pagination() { Limit = 1, Offset = 0 };
            _groupsSorting = new Sorting() { Order = SortOrder.Asc, Sort = "Name" };
            _group = new GroupDto {Name = "Group1", Email = "TestEmail@test.com", GroupSource = UserGroupSource.Database, License = LicenseType.Collaborator};            
        }

        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new GroupsController();

            // Assert
            Assert.IsInstanceOfType(controller._privilegesManager, typeof(PrivilegesManager));
            Assert.IsInstanceOfType(controller._groupRepository, typeof(SqlGroupRepository));
        }

        #endregion

        #region GetGroups

        [TestMethod]
        public async Task GetGroups_AllRequirementsSatisfied_ReturnGroups()
        {
            //arrange         
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(_groupsQueryDataResult);

            //act
            var result = await _controller.GetGroups(_groupsTabularPagination, _groupsSorting) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(QueryResult<GroupDto>));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetGroups_PaginationParamsAreNotCorrect_BadRequestResult()
        {
            //arrange

            //act
            await _controller.GetGroups(new Pagination(), new Sorting(), string.Empty, UserId);

            //assert
            // Exception

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetGroups_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            //arrange
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(_groupsQueryDataResult);

            //act
            var result = await _controller.GetGroups(_groupsTabularPagination, _groupsSorting, string.Empty, UserId) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

            //assert
            // Exception
        }
        #endregion

        #region DeleteGroups

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task DeleteGroups_UserdDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            //arrange
            var scope = new OperationScope() { Ids = new List<int>() { 1, 2 } };
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            //act
            var result = await _controller.DeleteGroups(scope);

            //assert
            //Exception
        }

        [TestMethod]
        public async Task DeleteGroups_ScopeDoesNotProvided_TotalDeletedIsZero()
        {
            //arrange
            //act
            var result = await _controller.DeleteGroups(new OperationScope()) as OkNegotiatedContentResult<DeleteResult>;

            //assert
            Assert.AreEqual(0, result.Content.TotalDeleted);
        }

        [TestMethod]
        public async Task DeleteGroups_InvalidParameteres_BadRequest()
        {
            //arrange

            //act
            var result = await _controller.DeleteGroups(null) as BadRequestErrorMessageResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Message, ErrorMessages.InvalidDeleteGroupsParameters);
        }

        [TestMethod]
        public async Task DeleteGroups_ValidRequest_ReturnDeletedGroupsCount()
        {
            //arrange
            _privilegesRepository
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            var scope = new OperationScope() { SelectAll = false, Ids = new List<int>() { 2, 3 } };
            var returnResult = 3;
            _sqlGroupRepositoryMock.Setup(repo => repo.DeleteGroupsAsync(It.Is<OperationScope>(a => a.Ids != null), It.IsAny<string>())).ReturnsAsync(returnResult);

            //act
            var result = await _controller.DeleteGroups(scope, string.Empty) as OkNegotiatedContentResult<DeleteResult>;


            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.TotalDeleted);
        }

        #endregion

        #region GetGroupDetails

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetGroup_UserdDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            //arrange
            _privilegesRepository
              .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(InstanceAdminPrivileges.None);

            //act
            var result = await _controller.GetGroup(3);

            //assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetGroup_TheIsNoGroupForSuchAndId_ResourceNotFoundException()
        {
            //arrange
            _privilegesRepository
            .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
            .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            var group = new Group();

            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupDetailsAsync(It.IsAny<int>())).ReturnsAsync(group);

            //act
            var result = await _controller.GetGroup(3);

            //assert
            //Exception
        }

        [TestMethod]
        public async Task GetGroup_ValidRequest_ReturnGroup()
        {
            //arrange
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            var group = new Group() { Id = 3 };

            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupDetailsAsync(It.IsAny<int>())).ReturnsAsync(group);

            //act
            var result = await _controller.GetGroup(3) as OkNegotiatedContentResult<Group>; 

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.Id);
        }

        #endregion

        #region Create group
        [TestMethod]
        public async Task CreateGroup_SuccessfulCreationOfGroup_ReturnCreatedGroupIdResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            var result = await _controller.CreateGroup(_group);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task CreateGroup_NoManageGroupsPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_GroupNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _group.Name = string.Empty;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_GroupNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _group.Name = "123";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_EmailOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _group.Email = "1@1";
            _privilegesRepository
             .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
             .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_EmailWithoutAtSymbol_ReturnBadRequestResult()
        {
            // Arrange
            _group.Email = "testemail.com";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_EmailWithMultipleAtSymbols_ReturnBadRequestResult()
        {
            // Arrange
            _group.Email = "sp@rk@email.com";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_CreateWindowsGroup_ReturnBadRequestResult()
        {
            // Arrange
            _group.GroupSource = UserGroupSource.Windows;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_WithViewerLicense_ReturnBadRequestResult()
        {
            // Arrange
            _group.License = LicenseType.Viewer;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_CreationGroupWithScopeAndLicenseIdSimultaneously_ReturnBadRequestResult()
        {
            // Arrange
            _group.License = LicenseType.Collaborator;
            _group.ProjectId = 1;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>())).ReturnsAsync(_groupId);

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_GroupAlreadyExist_ReturnBadRequestResult()
        {
            // Arrange
            _group.License = LicenseType.Collaborator;
            _group.ProjectId = 1;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ThrowsAsync(new BadRequestException(ErrorMessages.GroupAlreadyExist));

            // Act
            await _controller.CreateGroup(_group);

            // Assert
            // Exception
        }

        #endregion
    }
}
