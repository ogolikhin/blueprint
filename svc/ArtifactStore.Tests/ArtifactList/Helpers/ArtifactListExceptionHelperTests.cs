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

        [TestInitialize]
        public void Initialize()
        {
            _profileColumns = new List<ProfileColumn>
            {
                new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
            };
        }

        [TestMethod]
        public void InvalidColumnsException_AllParamsIsValid_ReturnBadRequestException()
        {
            var result = ArtifactListExceptionHelper.InvalidColumnsException(_profileColumns);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.Message));
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
