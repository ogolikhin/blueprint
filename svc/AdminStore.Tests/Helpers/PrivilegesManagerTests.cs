using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    [TestClass]
    public class PrivilegesManagerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Helpers.PrivilegesManager")]
        public void Construction_NoPrivilegeProvider_ThrowsArgumentNullException()
        {
            new PrivilegesManager(null);
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
    }
}
