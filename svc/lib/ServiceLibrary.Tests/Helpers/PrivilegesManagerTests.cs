using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    [TestClass]
    public class PrivilegesManagerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Helpers.PrivilegesManager")]
        public void Construction_NoPrivilegeProvider_ThrowsArgumentNullException()
        {
            var manager = new PrivilegesManager(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task Demand_SinglePrivilege_UserWithoutPrivilege_ThrowsAuthorizationException()
        {
            // arrange
            const int userId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();
            repositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            await manager.Demand(userId, InstanceAdminPrivileges.ManageUsers);
        }

        [TestMethod]
        public async Task Demand_SinglePrivilege_UserWithPrivilege_NoAuthorizationExceptionsThrown()
        {
            // arrange
            Exception exception = null;
            const int userId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();
            repositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            try
            {
                await manager.Demand(userId, InstanceAdminPrivileges.ManageUsers);
            }
            catch (AuthorizationException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task Demand_MultiplePrivileges_UserWithoutPrivilege_ThrowsAuthorizationException()
        {
            // arrange
            const int userId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();
            repositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            await manager.Demand(userId, InstanceAdminPrivileges.ManageUsers | InstanceAdminPrivileges.AssignAdminRoles);
        }

        [TestMethod]
        public async Task Demand_MultiplePrivileges_UserWithPrivilege_NoAuthorizationExceptionsThrown()
        {
            // arrange
            Exception exception = null;
            const int userId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();
            repositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);
            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            try
            {
                await manager.Demand(userId, InstanceAdminPrivileges.ManageUsers | InstanceAdminPrivileges.AssignAdminRoles);
            }
            catch (AuthorizationException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task DemandAny_UserWithInstancePrivileges_NoAuthorizationExceptionsThrown()
        {
            // arrange
            Exception exception = null;
            const int userId = 1;
            const int projectId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();

            repositoryMock
              .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
              .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);
            repositoryMock
                .Setup(m => m.GetProjectAdminPermissionsAsync(userId, projectId))
                .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);
                       
            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            try
            {
                await manager.DemandAny(userId, projectId, InstanceAdminPrivileges.AccessAllProjectsAdmin, ProjectAdminPrivileges.ManageGroupsAndRoles);
            }
            catch (AuthorizationException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task DemandAny_UserWithProjectAdminPrivileges_NoAuthorizationExceptionsThrown()
        {
            // arrange
            Exception exception = null;
            const int userId = 1;
            const int projectId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();

            repositoryMock
              .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
              .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            repositoryMock
                .Setup(m => m.GetProjectAdminPermissionsAsync(userId, projectId))
                .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            try
            {
                await manager.DemandAny(userId, projectId, InstanceAdminPrivileges.AccessAllProjectsAdmin, ProjectAdminPrivileges.ManageGroupsAndRoles);
            }
            catch (AuthorizationException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [ExpectedException(typeof (AuthorizationException))]
        public async Task DemandAny_UserWithoutAnyPrivilege_ThrowsAuthorizationException()
        {
            // arrange
            const int userId = 1;
            const int projectId = 1;
            var repositoryMock = new Mock<IPrivilegesRepository>();

            repositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            repositoryMock
                .Setup(m => m.GetProjectAdminPermissionsAsync(userId, projectId))
                .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);

            var manager = new PrivilegesManager(repositoryMock.Object);

            // act
            await
                manager.DemandAny(userId, projectId, InstanceAdminPrivileges.AccessAllProjectsAdmin,
                    ProjectAdminPrivileges.ManageGroupsAndRoles);

            //Exception
        }
    }
}
