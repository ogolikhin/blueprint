using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

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
        private AssignScope _assignScope;

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
            _group = new GroupDto { Name = "Group1", Email = "TestEmail@test.com", Source = UserGroupSource.Database, LicenseType = LicenseType.Collaborator };
            _assignScope = new AssignScope
            {
                SelectAll = true,
                Members = new List<KeyValuePair<int, UserType>> { new KeyValuePair<int, UserType>(1, UserType.User) }
            };
        }

        #region GetGroups

        [TestMethod]
        public async Task GetGroups_AllRequirementsSatisfied_ReturnGroups()
        {
            // arrange         
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(_groupsQueryDataResult);

            // act
            var result = await _controller.GetGroups(_groupsTabularPagination, _groupsSorting) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(QueryResult<GroupDto>));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetGroups_PaginationParamsAreNotCorrect_BadRequestResult()
        {
            // arrange

            // act
            await _controller.GetGroups(new Pagination(), new Sorting(), string.Empty, UserId);

            // assert
            // Exception

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetGroups_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(_groupsQueryDataResult);

            // act
            var result = await _controller.GetGroups(_groupsTabularPagination, _groupsSorting, string.Empty, UserId) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

            // assert
            // Exception
        }
        #endregion

        #region DeleteGroups

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task DeleteGroups_UserdDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            var scope = new OperationScope() { Ids = new List<int>() { 1, 2 } };
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            // act
            var result = await _controller.DeleteGroups(scope);

            // assert
            // Exception
        }

        [TestMethod]
        public async Task DeleteGroups_InvalidParameteres_BadRequest()
        {
            // arrange

            // act
            var result = await _controller.DeleteGroups(null) as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Message, ErrorMessages.InvalidDeleteGroupsParameters);
        }

        [TestMethod]
        public async Task DeleteGroups_ValidRequest_ReturnDeletedGroupsCount()
        {
            // arrange
            _privilegesRepository
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            var scope = new OperationScope() { SelectAll = false, Ids = new List<int>() { 2, 3 } };
            var returnResult = 3;
            _sqlGroupRepositoryMock.Setup(repo => repo.DeleteGroupsAsync(It.Is<OperationScope>(a => a.Ids != null), It.IsAny<string>())).ReturnsAsync(returnResult);

            // act
            var result = await _controller.DeleteGroups(scope, string.Empty) as OkNegotiatedContentResult<DeleteResult>;


            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.TotalDeleted);
        }

        #endregion

        #region GetGroupDetails

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetGroup_UserdDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            _privilegesRepository
              .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(InstanceAdminPrivileges.None);

            // act
            var result = await _controller.GetGroup(3);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetGroup_TheIsNoGroupForSuchAndId_ResourceNotFoundException()
        {
            // arrange
            _privilegesRepository
            .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
            .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            var group = new GroupDto();

            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupDetailsAsync(It.IsAny<int>())).ReturnsAsync(group);

            // act
            var result = await _controller.GetGroup(3);

            // assert
            // Exception
        }

        [TestMethod]
        public async Task GetGroup_ValidRequest_ReturnGroup()
        {
            // arrange
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            var group = new GroupDto() { Id = 3 };

            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupDetailsAsync(It.IsAny<int>())).ReturnsAsync(group);

            // act
            var result = await _controller.GetGroup(3) as OkNegotiatedContentResult<GroupDto>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.Id);
        }

        #endregion

        #region CreateGroup

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
        public async Task CreateGroup_GroupNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.Name = string.Empty;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.GroupNameRequired, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_GroupNameIsTooLong_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.Name = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis,.";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.GroupNameFieldLimitation, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_EmailOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.Email = "1@1";
            _privilegesRepository
             .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
             .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.EmailFieldLimitation, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_EmailWithoutAtSymbol_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.Email = "testemail.com";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.GroupEmailFormatIncorrect, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_EmailWithMultipleAtSymbols_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.Email = "sp@rk@email.com";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.GroupEmailFormatIncorrect, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_CreateWindowsGroup_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.Source = UserGroupSource.Windows;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.CreationOnlyDatabaseGroup, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_WithViewerLicense_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.LicenseType = LicenseType.Viewer;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.CreationGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task CreateGroup_CreationGroupWithScopeAndLicenseIdSimultaneously_ReturnBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _group.LicenseType = LicenseType.Collaborator;
            _group.ProjectId = 1;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock
                .Setup(repo => repo.AddGroupAsync(It.IsAny<GroupDto>()))
                .ReturnsAsync(_groupId);

            // Act
            try
            {
                await _controller.CreateGroup(_group);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.CreationGroupWithScopeAndLicenseIdSimultaneously, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateGroup_GroupAlreadyExist_ReturnBadRequestResult()
        {
            // Arrange
            _group.LicenseType = LicenseType.Collaborator;
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

        #endregion CreateGroup

        #region Update group
        [TestMethod]
        public async Task UpdateGroup_SuccessfulUpdateOfGroup_ReturnSuccessResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            var existingGroup = new GroupDto { Id = 1 };
            _sqlGroupRepositoryMock
                .Setup(r => r.GetGroupDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(existingGroup);

            // Act
            var result = await _controller.UpdateGroup(_groupId, _group);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateGroup_NoManageGroupsPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // Act
            await _controller.UpdateGroup(_groupId, _group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateGroup_UpdateWindowsGroup_ReturnBadRequestResult()
        {
            // Arrange
            _group.Source = UserGroupSource.Windows;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            var existingGroup = new GroupDto { Id = 1 };
            _sqlGroupRepositoryMock
                .Setup(r => r.GetGroupDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(existingGroup);

            // Act
            await _controller.UpdateGroup(_groupId, _group);

            // Assert
            // Exception
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateGroup_ProjectIdIsNotNull_ReturnBadRequestResult()
        {
            // Arrange
            _group.ProjectId = 1;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            var existingGroup = new GroupDto { Id = 1 };
            _sqlGroupRepositoryMock
                .Setup(r => r.GetGroupDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(existingGroup);

            // Act
            await _controller.UpdateGroup(_groupId, _group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateGroup_LicenseIsViewer_ReturnBadRequestResult()
        {
            // Arrange
            _group.LicenseType = LicenseType.Viewer;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            var existingGroup = new GroupDto { Id = 1 };
            _sqlGroupRepositoryMock
                .Setup(r => r.GetGroupDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(existingGroup);

            // Act
            await _controller.UpdateGroup(_groupId, _group);

            // Assert
            // Exception
        }

        #endregion

        #region GetGroupsAndUsers

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetGroupsAndUsers_PaginationParamsAreNotCorrect_BadRequestResult()
        {
            // arrange

            // act
            await _controller.GetGroupsAndUsers(new Pagination(), new Sorting(), string.Empty, UserId);

            // assert
            // Exception

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetGroupsAndUsers_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            var resultQuery = new QueryResult<GroupUser>();
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupUsersAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(resultQuery);

            // act
            var result = await _controller.GetGroupsAndUsers(_groupsTabularPagination, _groupsSorting, string.Empty, 10) as OkNegotiatedContentResult<QueryResult<GroupUser>>;

            // assert
            // Exception
        }

        [TestMethod]
        public async Task GetGroupsAndUsers_AllParametersAreFine_ReturnGroupsAndUsers()
        {
            // arrange
            var queryResult = new QueryResult<GroupUser>();

            _privilegesRepository
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupUsersAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(queryResult);

            // act
            var result = await _controller.GetGroupsAndUsers(_groupsTabularPagination, _groupsSorting, string.Empty, 10) as OkNegotiatedContentResult<QueryResult<GroupUser>>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(QueryResult<GroupUser>));
        }

        #endregion

        #region GetGroupMembers

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetGroupMembers_PaginationParamsAreNotCorrect_BadRequestResult()
        {
            // arrange

            // act
            await _controller.GetGroupMembers(_groupId, new Pagination(), new Sorting());

            // assert
            // Exception

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetGroupMembers_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            var resultQuery = new QueryResult<GroupUser>();
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupMembersAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()));

            // act
            var result = await _controller.GetGroupMembers(_groupId, _groupsTabularPagination, _groupsSorting) as OkNegotiatedContentResult<QueryResult<GroupUser>>;

            // assert
            // Exception
        }

        [TestMethod]
        public async Task GetGroupMembers_AllParametersAreCorrect_ReturnMembersList()
        {
            // arrange
            var queryResult = new QueryResult<GroupUser>();

            _privilegesRepository
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.GetGroupMembersAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(queryResult);

            // act
            var result = await _controller.GetGroupMembers(_groupId, _groupsTabularPagination, _groupsSorting) as OkNegotiatedContentResult<QueryResult<GroupUser>>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(QueryResult<GroupUser>));
        }

        #endregion

        #region  RemoveMembersFromGroup

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveMembersFromGroup_PaginationParamsAreNotCorrect_BadRequestResult()
        {
            // arrange

            // act
            await _controller.RemoveMembersFromGroup(_groupId, null);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task RemoveMembersFromGroup_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // act
            await _controller.RemoveMembersFromGroup(_groupId, _assignScope);

            // assert
            // Exception
        }

        [TestMethod]
        public async Task RemoveMembersFromGroup_AllParametersAreCorrect_ReturnCountDeletedMembers()
        {
            // arrange
            _privilegesRepository
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);
            _sqlGroupRepositoryMock.Setup(repo => repo.DeleteMembersFromGroupAsync(It.IsAny<int>(), It.IsAny<AssignScope>())).ReturnsAsync(1);

            // act
            var result = await _controller.RemoveMembersFromGroup(_groupId, _assignScope) as OkNegotiatedContentResult<DeleteResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(DeleteResult));
        }

        #endregion

        #region AssignMembers

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignMembers_ParametersAreInvalid_BadRequestResult()
        {
            // arrange

            // act
            await _controller.AssignMembers(_groupId, null);

            // assert   
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task AssignMembers_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            var collection = new List<KeyValuePair<int, UserType>>() { new KeyValuePair<int, UserType>(1, UserType.Group) };
            var scope = new AssignScope() { Members = collection.ToArray() };
            _privilegesRepository
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.None);
            _sqlGroupRepositoryMock.Setup(repo => repo.AssignMembers(It.IsAny<int>(), It.IsAny<AssignScope>(), It.IsAny<string>()));

            // act
            await _controller.AssignMembers(_groupId, scope);

            // assert
        }

        #endregion

        #region Project Groups

        [TestMethod]
        public async Task GetProjectGroupsAsync_AllRequirementsIsSatisfied_ReturnOkNegotiatedResult()
        {
            // Arrange
            var projectId = 100;
            var projectGroups = new List<GroupDto>
            {
                new GroupDto()
                {
                    Name = "Group1"
                },
                new GroupDto()
                {
                    Name = "Group2"
                },
                new GroupDto()
                {
                    Name = "Group3"
                }
            };

            QueryResult<GroupDto> groupsQueryResult = new QueryResult<GroupDto>() { Items = projectGroups, Total = 3 };

            var pagination = new Pagination() { Limit = 5, Offset = 0 };
            var sorting = new Sorting() { Order = SortOrder.Asc, Sort = "Name" };

            _privilegesRepository
            .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            _privilegesRepository
                .Setup(r => r.GetProjectAdminPermissionsAsync(SessionUserId, projectId))
                .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);

            _sqlGroupRepositoryMock
                .Setup(repo => repo.GetProjectGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(groupsQueryResult);

            // Act
            var result = await _controller.GetProjectGroupsAsync(projectId, pagination, sorting) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, groupsQueryResult);

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectGroupsAsync_Failed_NoPermissions_ReturnForbiddenResult()
        {
            // Arrange
            var projectId = 100;
            var projectGroups = new List<GroupDto>
            {
                new GroupDto()
                {
                    Name = "Group1"
                },
                new GroupDto()
                {
                    Name = "Group2"
                },
                new GroupDto()
                {
                    Name = "Group3"
                }
            };

            QueryResult<GroupDto> groupsQueryResult = new QueryResult<GroupDto>() { Items = projectGroups, Total = 3 };

            var pagination = new Pagination() { Limit = 5, Offset = 0 };
            var sorting = new Sorting() { Order = SortOrder.Asc, Sort = "Name" };

            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _privilegesRepository
                .Setup(r => r.GetProjectAdminPermissionsAsync(SessionUserId, projectId)).ReturnsAsync(ProjectAdminPrivileges.None);

            _sqlGroupRepositoryMock
                .Setup(repo => repo.GetProjectGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(groupsQueryResult);

            // Act
            var result = await _controller.GetProjectGroupsAsync(projectId, pagination, sorting) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetProjectGroupsAsync_IncorrectModel_ReturnBadRequestResult()
        {
            // Arrange
            var projectId = 100;
            var projectGroups = new List<GroupDto>
            {
                new GroupDto()
                {
                    Name = "Group1"
                },
                new GroupDto()
                {
                    Name = "Group2"
                },
                new GroupDto()
                {
                    Name = "Group3"
                }
            };

            QueryResult<GroupDto> groupsQueryResult = new QueryResult<GroupDto>() { Items = projectGroups, Total = 3 };

            var sorting = new Sorting() { Order = SortOrder.Asc, Sort = "Name" };

            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _privilegesRepository
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, projectId)).ReturnsAsync(ProjectAdminPrivileges.None);

            _sqlGroupRepositoryMock
                .Setup(repo => repo.GetProjectGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(groupsQueryResult);

            // Act
            var result = await _controller.GetProjectGroupsAsync(projectId, null, sorting) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

        }
        
        #endregion
    }
}
