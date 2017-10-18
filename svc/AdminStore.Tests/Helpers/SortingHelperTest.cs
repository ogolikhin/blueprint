using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Helpers
{
    [TestClass]
    public class SortingHelperTest
    {
        [TestMethod]
        public void SortProjectRolesAssignments_RoleNameAsc_CorrectResult()
        {
            // Arange
            var sorting = new Sorting { Sort = "roleName", Order = SortOrder.Asc };
            var result = SortingHelper.SortProjectRolesAssignments(sorting);

            // Act
            Assert.AreEqual(result, "roleName");
        }

        [TestMethod]
        public void SortProjectRolesAssignments_RoleNameDesc_CorrectResult()
        {
            // Arange
            var sorting = new Sorting { Sort = "roleName", Order = SortOrder.Desc };
            var result = SortingHelper.SortProjectRolesAssignments(sorting);

            // Act
            Assert.AreEqual(result, "-roleName");
        }

        [TestMethod]
        public void SortProjectRolesAssignments_DefaultSort_CorrectResult()
        {
            // Arange
            var sorting = new Sorting();
            var result = SortingHelper.SortProjectRolesAssignments(sorting);

            // Act
            Assert.AreEqual(result, "groupName");
        }
    }
}
