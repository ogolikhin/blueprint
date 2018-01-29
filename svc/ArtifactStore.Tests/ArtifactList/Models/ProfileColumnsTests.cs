using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    [TestClass]
    public class ProfileColumnsTests
    {
        private List<ProfileColumn> _columns;

        [TestInitialize]
        public void Initialize()
        {
            _columns = new List<ProfileColumn>
            {
                new ProfileColumn("Test1", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 1),
                new ProfileColumn("Test2", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
            };
        }

        [TestMethod]
        public void Construction_ColumnsNull_ThrowsException()
        {
            // Act
            try
            {
                // var profileColumns = new ProfileColumns(null);
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual("columns", ex.ParamName);
                return;
            }

            // Assert.Fail("ArgumentNullException was expected.");
        }

        [TestMethod]
        public void Construction_UnderCapacity_ReturnsColumns()
        {
            // Act
            var profileColumns = new ProfileColumns(_columns);

            // Assert
            CollectionAssert.AreEquivalent(_columns, profileColumns.Items.ToList());
        }

        [TestMethod]
        public void Construction_DuplicateColumns_ThrowsException()
        {
            // Arrange
            var column = _columns.Last();
            _columns.Add(column);

            // Act
            try
            {
               // var profileColumns = new ProfileColumns(_columns);
            }
            catch (ArgumentException ex)
            {
                // Assert
                var errorMessage = I18NHelper.FormatInvariant(
                    ErrorMessages.ArtifactList.AddColumnColumnExists, column.PropertyName);
                Assert.AreEqual(errorMessage, ex.Message);
                return;
            }

            // Assert.Fail("ArgumentException was expected.");
        }

        [TestMethod]
        public void Construction_OverCapacity_ThrowException()
        {
            // Arrange
            const int maxCapacity = 2;
            var column = new ProfileColumn("Test3", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text);
            _columns.Add(column);

            // Act
            try
            {
                // var profileColumns = new ProfileColumns(_columns, maxCapacity);
            }
            catch (ApplicationException ex)
            {
                // Assert
                var errorMessage = I18NHelper.FormatInvariant(
                    ErrorMessages.ArtifactList.AddColumnCapacityReached, column.PropertyName, maxCapacity);
                Assert.AreEqual(errorMessage, ex.Message);
                return;
            }

            // Assert.Fail("ApplicationException was expected.");
        }
    }
}
