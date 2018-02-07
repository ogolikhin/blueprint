using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ArtifactStore.ArtifactList.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
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

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ArtifactStore.ArtifactList.Models.ProfileColumns")]
        [TestMethod]
        public void Construction_ColumnsNull_ThrowsException()
        {
            // Act
            try
            {
                new ProfileColumns(null);
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual("columns", ex.ParamName);
                return;
            }

            Assert.Fail("ArgumentNullException was expected.");
        }

        [TestMethod]
        public void Construction_UnderCapacity_ReturnsColumns()
        {
            // Act
            var profileColumns = new ProfileColumns(_columns);

            // Assert
            CollectionAssert.AreEquivalent(_columns, profileColumns.Items.ToList());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ArtifactStore.ArtifactList.Models.ProfileColumns")]
        [TestMethod]
        public void Construction_DuplicateColumns_ThrowsException()
        {
            // Arrange
            var column = _columns.Last();
            _columns.Add(column);

            // Act
            try
            {
                new ProfileColumns(_columns);
            }
            catch (BadRequestException ex)
            {
                // Assert
                var expectedException = ArtifactListExceptionHelper.DuplicateColumnException(column.PropertyName);
                Assert.AreEqual(expectedException.ErrorCode, ex.ErrorCode);
                Assert.AreEqual(expectedException.Message, ex.Message);
                return;
            }

            Assert.Fail("BadRequestException was expected.");
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ArtifactStore.ArtifactList.Models.ProfileColumns")]
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
                new ProfileColumns(_columns, maxCapacity);
            }
            catch (BadRequestException ex)
            {
                // Assert
                var expectedException = ArtifactListExceptionHelper.ColumnCapacityExceededException(column.PropertyName, maxCapacity);
                Assert.AreEqual(expectedException.ErrorCode, ex.ErrorCode);
                Assert.AreEqual(expectedException.Message, ex.Message);
                return;
            }

            Assert.Fail("BadRequestException was expected.");
        }
    }
}
