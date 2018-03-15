using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Helpers
{
    [TestClass]
    public class ArtifactListExceptionHelperTests
    {
        private IReadOnlyList<ProfileColumn> _profileColumns;
        private const int _maxPropertiesToShow = 3;

        [TestInitialize]
        public void Initialize()
        {
            _profileColumns = new List<ProfileColumn>
            {
                new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
            };
        }

        [TestMethod]
        public void InvalidColumnsException_AllParamsIsValid_OneProfileColumn_ReturnBadRequestException()
        {
            var expectedMessage = I18NHelper.FormatInvariant(
                ErrorMessages.ArtifactList.ColumnsSettings.SingleOrSomeInvalidColumns,
                _profileColumns.First().PropertyName);

            var result = ArtifactListExceptionHelper.InvalidColumnsException(_profileColumns);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedMessage, result.Message);
        }

        [TestMethod]
        public void InvalidColumnsException_AllParamsIsValid_TwoOrThreeProfileColumns_ReturnBadRequestException()
        {
            _profileColumns = new List<ProfileColumn>
            {
                new ProfileColumn("Custom1", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2),
                new ProfileColumn("Custom2", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2),
                new ProfileColumn("Custom3", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
            };

            var expectedMessage = I18NHelper.FormatInvariant(
                ErrorMessages.ArtifactList.ColumnsSettings.SingleOrSomeInvalidColumns,
                string.Join(", ", _profileColumns.Take(_maxPropertiesToShow).Select(column => column.PropertyName)));

            var result = ArtifactListExceptionHelper.InvalidColumnsException(_profileColumns);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedMessage, result.Message);
        }

        [TestMethod]
        public void InvalidColumnsException_AllParamsIsValid_MoreThenThreeProfileColumns_ReturnBadRequestException()
        {
            _profileColumns = new List<ProfileColumn>
            {
                new ProfileColumn("Custom1", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2),
                new ProfileColumn("Custom2", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2),
                new ProfileColumn("Custom3", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2),
                new ProfileColumn("Custom4", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
            };

            var expectedMessage = I18NHelper.FormatInvariant(
                ErrorMessages.ArtifactList.ColumnsSettings.MultipleInvalidColumns,
                string.Join(", ", _profileColumns.Take(_maxPropertiesToShow).Select(column => column.PropertyName)));

            var result = ArtifactListExceptionHelper.InvalidColumnsException(_profileColumns);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedMessage, result.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidColumnsException_ProfileColumnsIsEmpty_ThrowArgumentException()
        {
            _profileColumns = new List<ProfileColumn>();
            ArtifactListExceptionHelper.InvalidColumnsException(_profileColumns);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidColumnsException_ProfileColumnsIsNull_ThrowArgumentException()
        {
            _profileColumns = null;
            ArtifactListExceptionHelper.InvalidColumnsException(_profileColumns);
        }
    }
}
