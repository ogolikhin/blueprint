using System;
using System.Collections.Generic;
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
using ServiceLibrary.Repositories.ConfigControl;

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
    }
}
